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
    
    private List<InputDevice> registeredInputDevices = new();
    private bool[] shouldWaitForRelease;

    private void OnValidate()
    {
        inputActionMap = inputActionAsset?.FindActionMap(nameActionMap);
        enabled = !(inputActionAsset == null || inputActionMap == null || playerCount == 0);
    }

    private void Awake()
    {
        playerHitBeatAction = inputActionMap.FindAction(nameHitBeatAction);
        playerHitBeatAction.performed += OnPlayerPressed;
        shouldWaitForRelease = new bool[playerCount];
    }

    private void Update() => CheckReleasedAction();

    private void CheckReleasedAction()
    {
        if (!playerHitBeatAction.WasReleasedThisFrame()) return;
        foreach (var inputControl in playerHitBeatAction.controls)
        {
            var id = GetDeviceId(inputControl.device);
            if (shouldWaitForRelease[id] && !inputControl.IsPressed())
                OnPlayerReleased(id);
        }
    }

    private void OnPlayerPressed(InputAction.CallbackContext obj)
    {
        CheckRegistrationDevice(obj.control.device);
        var id = GetDeviceId(obj.control.device);
        onPlayerPressed.Invoke(id);
        shouldWaitForRelease[id] = true;
        //Debug.Log($"Player {id} Pressed"); // TODO: remove debug log
    }

    private void OnPlayerReleased(int id)
    {
        onPlayerReleased.Invoke(id);
        shouldWaitForRelease[id] = false;
        //Debug.Log($"Player {id} Released"); // TODO: remove debug log
    }

    private void CheckRegistrationDevice(InputDevice controlDevice)
    {
        if (!registeredInputDevices.Contains(controlDevice))
            registeredInputDevices.Add(controlDevice);
    }
    
    private int GetDeviceId(InputDevice device) => registeredInputDevices.FindIndex(x => x.deviceId == device.deviceId);
}
