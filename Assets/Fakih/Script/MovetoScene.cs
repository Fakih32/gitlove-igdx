using UnityEngine;

public class MovetoScene : MonoBehaviour
{
    
    void Start()
    {
        
    }
public void Gotoscene(string scene){
  UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
}
 public void CompletedCutscene()
    {
        LevelSessionManager .Instance.OnMechanicComplete();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
