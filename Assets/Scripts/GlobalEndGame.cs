using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A static class accessible anywhere for easily closing the game, used only for debugging and prototype versions of game
/// </summary>
public static class GlobalEndGame
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        InputSystem.onAfterUpdate += CheckQuit;
    }
    
    private static void CheckQuit()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Quit();
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}