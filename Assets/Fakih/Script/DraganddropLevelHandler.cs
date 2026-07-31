using UnityEngine;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using System.Threading;
public enum Scoresenum{
    onestar,
    twostar,
    threestar
}

public class DraganddropLevelHandler : MonoBehaviour
{
    public bool islevel1;
    public static DraganddropLevelHandler instance;
    [Header("Batas Waktu Permainan")]
    public float timer;
    [Header("Tempat Posisi Yang Benar")]
    public List<Transform> Listtujuan;
    [HideInInspector]public int tujuancount;
    [HideInInspector]public int tujuanmax;
    private bool gameEnded = false;
    [Header("Poin Yang Didapat")]
    [HideInInspector] public float currentpoint = 0;
    public float poin_got;

   
    [Header("Game Objek Yang Ingin Di drag")]
    public Image firstimg;
    public Image secondimg;
    public Image thirdimg;
    [Header("Posisi X masing-masing tujuan gambar")]
     public float firstimagexpos;
     public float secondimagexpos;
     public float thirdmagexpos;
     [Header("Posisi y masing-masing tujuan gambar")]
     public float firstimageypos;
     public float secondimageypos;
     public float thirdmageypos;
    
     [Header("Waktu yang dibutuhkan untuk mencapai poin")]
  
     public float onestartime;
     public float twostartime;
     public float threestartime; 
     private float timetotal;
    private Scoresenum skortype;
    [Header("Panel menang kalah")]
    [Header("Panel Menang")]
    public GameObject winpanel;
    [Header("Bintang")]
    public GameObject firststar;
    public GameObject secondstar;
     public GameObject thirdstar;
     [Header("Panel Kalah")]
     public GameObject losepanel;
    public void Addpoint()
    {
       
    currentpoint += poin_got;
     Debug.Log(currentpoint);  
    }
    
    void Gamelost(){
        
        losepanel.SetActive(true);
    }
    void Checkwin(){
        
        float waktuterpakai = timetotal-TimerScript.instance.timer;
        

       if (waktuterpakai<=threestartime)
        {
            Debug.Log("solving under"+waktuterpakai+"seconds");
             skortype = Scoresenum.threestar; 
             Gamewin();
             
        }
         else if (waktuterpakai<=twostartime)
        {
              Debug.Log("solving under"+waktuterpakai+"seconds");
             skortype = Scoresenum.twostar;
             Gamewin(); 
             
        }
         else if (waktuterpakai<=onestartime)
        {
              Debug.Log("solving under"+waktuterpakai+"seconds");
             skortype = Scoresenum.onestar; 
             Gamewin();
             
        }
        
    }
    public void Gamewin(){
        winpanel.SetActive(true);
        if (skortype == Scoresenum.onestar){
            firststar.SetActive(true);
            secondstar.SetActive(false);
            thirdstar.SetActive(false);
        }
        else if(skortype == Scoresenum.twostar){
            firststar.SetActive(true);
            secondstar.SetActive(true);
            thirdstar.SetActive(false);
        }
        else if (skortype == Scoresenum.threestar)
        {
            firststar.SetActive(true);
            thirdstar.SetActive(true);
            secondstar.SetActive(true);
        }
    }
    
    void Awake(){
        if (instance==null){
            instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        tujuanmax = Listtujuan.Count;
        timetotal = TimerScript.instance.timer;
      
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameEnded) return;
        
        if (TimerScript.instance.timer > 0f)
        {
            if (tujuancount == tujuanmax)
            {
                gameEnded = true;
                Checkwin();
                TimerScript.instance.timer = 0;
            }
        }
        else if (TimerScript.instance.timer <= 0f)
        {
            gameEnded = true;
            Gamelost();
        }
    }
}
