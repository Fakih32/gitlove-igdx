using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class LevelSelectionHandler : MonoBehaviour
{
    public static LevelSelectionHandler Instance { get; private set; }
    [Header("gambar level terkunci")]
    public List<Sprite> lockedLevelImages;
    [Header("gambar level terbuka")]
    public List<Sprite> unlockedLevelImages;

    [Header("Tombol Pilih Level")]
    public List<Button> levelButtons;
    [Header("Game Scene")]
    public string gameScene;
   
   
    [Header("Level Data")]
    public int currentlevel;
    public int maxLevel;
    public int totalunlockedLevel;
  
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void selectlevel(GameObject button)
    {
        int buttonIndex = levelButtons.IndexOf(button.GetComponent<Button>());
        if (buttonIndex >= 0 && buttonIndex < totalunlockedLevel)
        {
            currentlevel = buttonIndex + 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
            UnityEngine.Debug.Log("Selected Level: " + currentlevel);
         
        }
        else
        {
            UnityEngine.Debug.LogWarning("Level " + (buttonIndex + 1) + " is locked or invalid.");
        }
       
    }
    public void UnlockednewLevel()
    {
        totalunlockedLevel++;
        PlayerPrefs.SetInt("TotalUnlockedLevel", totalunlockedLevel);
    }
        public void setupLevelButtons()
    {
       
        for (int i = 0; i < levelButtons.Count; i++)
        {
                int index = i;
                if (i < totalunlockedLevel)
                {
                    levelButtons[i].gameObject.GetComponent<Image>().sprite = unlockedLevelImages[i];
                }
                else
                {
                    levelButtons[i].GetComponent<Image>().sprite = lockedLevelImages[i];
                    levelButtons[i].interactable = false;
                }
                levelButtons[i].onClick.AddListener(() => selectlevel(levelButtons[index].gameObject));
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if (PlayerPrefs.HasKey("TotalUnlockedLevel"))
        //{
       //     totalunlockedLevel = PlayerPrefs.GetInt("TotalUnlockedLevel");
       // }
        //else
       // {
         //  totalunlockedLevel = 1;
       //     PlayerPrefs.SetInt("TotalUnlockedLevel", totalunlockedLevel);
        //}

        setupLevelButtons();
        //setupallbuttons();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
