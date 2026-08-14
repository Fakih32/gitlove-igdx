using UnityEngine;
using UnityEngine.UI;
public class MainMenuHandler : MonoBehaviour
{
     [Header("Panel Main Menu")]
    [SerializeField] Button mainMenuButton;
    [SerializeField] Button QuitButton;
    [Header("Level Selection Scene")]
    [SerializeField] string levelSelectionScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void gotoLevelSelectionScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelSelectionScene);
    }
    void quitGame()
    {
        Application.Quit();
    }
    void Start()
    {
        mainMenuButton.onClick.AddListener(gotoLevelSelectionScene);
        QuitButton.onClick.AddListener(quitGame);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
