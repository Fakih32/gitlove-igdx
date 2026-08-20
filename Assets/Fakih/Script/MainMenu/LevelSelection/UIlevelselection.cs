using UnityEngine;
using UnityEngine.UI;
public class OtherUIlevelselection : MonoBehaviour
{
     public static LevelSelectionHandler Instance { get; private set; }
     [Header(" Kembali ke Main Menu")]
    public Button backButton;
    public string mainMenuScene;
    [Header("Tombol Lain")]
    public Button SettingButton;
    public Button CloseSettingButton;
    public Button closeBookButton;
    public Button Bookbutton;
    [Header("Panel Setting")]
    public GameObject settingPanel;
    [Header("Game Scene")]
    public string gameScene;
   
    [Header("Panel Book")]
     public GameObject bookPanel;
       
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void backToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
    }
    public void openSettingPanel()
    {
        settingPanel.SetActive(true);
    }
    public void closeSettingPanel()
    {
        settingPanel.SetActive(false);
    }
    public void openBookPanel()
    {
        bookPanel.SetActive(true);
    }
    public void closeBookPanel()
    {
        bookPanel.SetActive(false);
    }
    public void setupallbuttons()
    {
        backButton.onClick.AddListener(backToMainMenu);
        SettingButton.onClick.AddListener(openSettingPanel);
        CloseSettingButton.onClick.AddListener(closeSettingPanel);
        Bookbutton.onClick.AddListener(openBookPanel);
        closeBookButton.onClick.AddListener(closeBookPanel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
