using System.Diagnostics;
using UnityEngine;

public class DataLevelHandler : MonoBehaviour
{
    public static DataLevelHandler Instance { get; private set; }
   
        [HideInInspector] public int totalunlockedLevel;
        [HideInInspector] public int currentlevel;
        [HideInInspector] public int maxLevel;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(LevelSelectionHandler.Instance != null)
        {
        totalunlockedLevel = LevelSelectionHandler.Instance.totalunlockedLevel;
        currentlevel = LevelSelectionHandler.Instance.currentlevel;
        maxLevel = LevelSelectionHandler.Instance.maxLevel;
        }
        
    }
}
