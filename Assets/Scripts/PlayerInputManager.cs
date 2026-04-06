using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Takes in a inputAsset, and the name for the input used by every player to hit a beat
/// Manages hit/pressed as well as the release event for held beat presses, these events send out a int with their player id
/// Manages registered devices internally TODO: move this functionality to separate component at start of the game
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [Header("Input config")]
    [SerializeField]
    private InputActionAsset inputActionAsset;
    [SerializeField]
    private string nameActionMap = "RhythmPlayer";
    [SerializeField]
    private string nameHitBeatAction = "HitBeat";
    [SerializeField, HideInInspector]
    private InputActionMap inputActionMap;
    private InputAction playerHitBeatAction;
    [Header("Events")]    
    public UnityEvent<int> onPlayerPressed = new();
    public UnityEvent<int> onPlayerReleased = new();
    [SerializeField, Space]
    private int playerCount = 0;
    public int PlayerCount { get => playerCount; private set => playerCount = value; }
    
    private List<PlayerRegistration> registeredPlayers = new();
    private bool[] shouldWaitForRelease;

    private class PlayerRegistration
    {
        private readonly int deviceId;
        private readonly string controlPath;

        public PlayerRegistration(int deviceId, string controlPath)
        {
            this.deviceId = deviceId;
            this.controlPath = controlPath;
        }
        
        public override bool Equals(object other) =>
            other is PlayerRegistration r &&
            deviceId == r.deviceId &&
            controlPath == r.controlPath;

        public override int GetHashCode()
        {
            return HashCode.Combine(deviceId, controlPath);
        }
    }

    private void OnValidate()
    {
        inputActionMap = inputActionAsset?.FindActionMap(nameActionMap);
        enabled = !(inputActionAsset == null || inputActionMap == null || playerCount == 0);
    }

    private void Awake()
    {
        playerHitBeatAction = inputActionMap.FindAction(nameHitBeatAction);
        playerHitBeatAction.Enable();
        playerHitBeatAction.performed += OnPlayerPressed;
        shouldWaitForRelease = new bool[playerCount];

        var neededPlayers = playerHitBeatAction.controls.Select(inputControl => new PlayerRegistration(inputControl.device.deviceId, inputControl.name)).ToArray();
        for (var i = 0; i < playerCount; i++) CheckRegistrationDevice(neededPlayers[i]);
    }

    private void Update() => CheckReleasedAction();

    private void CheckReleasedAction()
    {
        if (!playerHitBeatAction.WasReleasedThisFrame()) return;
        foreach (var inputControl in playerHitBeatAction.controls)
        {
            var possiblePlayer = new PlayerRegistration(inputControl.device.deviceId, inputControl.name);
            var playerId = GetPlayerId(possiblePlayer);
            if (playerId < 0 || playerId >= registeredPlayers.Count)
                continue;
            if (shouldWaitForRelease[playerId] && !inputControl.IsPressed())
                OnPlayerReleased(playerId);
        }
    }

    private void OnPlayerPressed(InputAction.CallbackContext obj)
    {
        var player = new PlayerRegistration(obj.control.device.deviceId, obj.control.name);
        CheckRegistrationDevice(player);
        var playerId = GetPlayerId(player);
        if (playerId < 0 || playerId >= registeredPlayers.Count)
            return;
        onPlayerPressed.Invoke(playerId);
        shouldWaitForRelease[playerId] = true;
    }

    private void OnPlayerReleased(int playerId)
    {
        if (playerId < 0 || playerId >= registeredPlayers.Count)
            return;
        onPlayerReleased.Invoke(playerId);
        shouldWaitForRelease[playerId] = false;
    }

    private void CheckRegistrationDevice(PlayerRegistration player)
    {
        if (!registeredPlayers.Contains(player))
            registeredPlayers.Add(player);
    }
    
    private int GetPlayerId(PlayerRegistration player) => registeredPlayers.FindIndex(x => Equals(x, player));
}
