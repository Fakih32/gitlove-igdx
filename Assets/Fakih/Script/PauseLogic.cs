using UnityEngine;

public class PauseLogic : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public GameObject pauseMenu;
    
  public void TogglePause()
    {
        
            pauseMenu.SetActive(true);
            Time.timeScale = 0f; 
        
    }
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        }
    public void Restart(string restartwhat)
{
    Time.timeScale = 1f;
    UnityEngine.SceneManagement.SceneManager.LoadScene(restartwhat);
}
public void BacktoLevelSelection(string Scene)
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(Scene);
    }    
    // Update is called once per frame
    void Update()
    {
        
    }
}
