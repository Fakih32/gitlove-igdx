using UnityEngine;

public class MovetoScene : MonoBehaviour
{
    
    void Start()
    {
        
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
