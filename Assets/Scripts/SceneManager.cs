using UnityEngine;

/// <summary>
/// Simple class for easily changing the scene based on serialized scene names
/// </summary>
public class SceneManager : MonoBehaviour
{
    [SerializeField]
    private string startMenuScene = "TitleScreen";
    [SerializeField]
    private string gameScene = "SampleScene";
    [SerializeField]
    private string endScene = "EndScene";
    
    public void LoadStartMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene(startMenuScene);
    public void LoadGame() => UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
    public void LoadEndScene() => UnityEngine.SceneManagement.SceneManager.LoadScene(endScene);
}
