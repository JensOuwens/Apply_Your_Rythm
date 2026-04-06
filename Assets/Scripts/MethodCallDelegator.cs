using UnityEngine;
using UnityEngine.Events;

public class MethodCallDelegator : MonoBehaviour
{
    public UnityEvent toCall;
    public void Call() => toCall.Invoke();
}
