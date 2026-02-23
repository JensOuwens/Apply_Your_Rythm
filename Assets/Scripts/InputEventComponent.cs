using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputEventComponent : MonoBehaviour
{
    [Header("Input config")]
    [SerializeField]
    private InputActionAsset inputActionAsset;
    [SerializeField]
    private string nameActionMap = "RhythmPlayer";
    [SerializeField]
    private string nameAction = "HitBeat";
    [SerializeField, HideInInspector]
    private InputActionMap inputActionMap;
    private InputAction playerAction;
    [Header("Events"), Space] 
    [SerializeField]
    private bool singleUse = false;
    public UnityEvent onPlayerPressed = new();
    
    private void OnValidate()
    {
        inputActionMap = inputActionAsset?.FindActionMap(nameActionMap);
        enabled = !(inputActionAsset == null || inputActionMap == null);
    }

    private void OnEnable()
    {
        playerAction = inputActionMap.FindAction(nameAction);
        playerAction.Enable();
        playerAction.performed += OnPlayerPressed;
    }

    private void OnPlayerPressed(InputAction.CallbackContext obj)
    {
        onPlayerPressed.Invoke();
        if (singleUse)
        {
            enabled = false;
        }
    }

    private void OnDestroy() => playerAction.performed -= OnPlayerPressed;
    private void OnDisable() => playerAction.performed -= OnPlayerPressed;
}
