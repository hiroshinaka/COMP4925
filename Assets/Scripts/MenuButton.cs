using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    // Name of your landing page scene
    [SerializeField] private string landingSceneName = "landingScene";

    // Called when the Menu button is clicked
    public void OnMenuClicked()
    {
        Debug.Log("Menu button clicked → loading landing scene...");
        SceneManager.LoadScene(landingSceneName);
    }
}
