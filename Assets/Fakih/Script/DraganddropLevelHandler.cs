using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
    [Header("Data Setiap Level")]
    public DraganddropLevelScriptable leveldata;
   
    [Header("Game Objek Yang Ingin Di drag")]
    public Image firstimg;
    public Image secondimg;
    public Image thirdimg;
    [Header("Posisi  masing-masing tujuan gambar")]
      public Vector2 firstImagePos;
public Vector2 secondImagePos;
public Vector2 thirdImagePos;
[Header("Level saat ini")]
public int curlevel;
    
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
     [Header("Panel tujuan")]
     public GameObject tujuan1;
     public GameObject tujuan2;
      public GameObject tujuan3;
    public void Addpoint()
    {
       
    currentpoint += poin_got;
     Debug.Log(currentpoint);  
    }
    public void loadlevel(){
         foreach (var data in leveldata.levels)
        {
            if (data.Levels == curlevel)
            {
                firstimg.sprite = data.firstimage;
                secondimg.sprite = data.secondimage;
                thirdimg.sprite = data.thirdimage;

               tujuan1.transform.position = data.firstImagePos;
                tujuan2.transform.position = data.secondImagePos;
                 tujuan3.transform.position= data.thirdImagePos;

                return;
            }
        }

        Debug.LogWarning($"Level {curlevel} not found.");
    }
    
    
    void Gamelost(){
        
        losepanel.SetActive(true);
    }

    void Checkwin(){
        float waktuterpakai = timetotal - TimerScript.instance.timer;
        Debug.Log($"solving under {waktuterpakai} seconds");

        if (waktuterpakai <= threestartime)
        {
            skortype = Scoresenum.threestar;
        }
        else if (waktuterpakai <= twostartime)
        {
            skortype = Scoresenum.twostar;
        }
        else
        {
            skortype = Scoresenum.onestar;
        }

        Gamewin();
    }
    public void Gamewin(){
        winpanel.SetActive(true);
        firststar.SetActive(false);
        secondstar.SetActive(false);
        thirdstar.SetActive(false);

        if (skortype == Scoresenum.onestar){
            firststar.SetActive(true);
        }
        else if(skortype == Scoresenum.twostar){
            firststar.SetActive(true);
            secondstar.SetActive(true);
        }
        else if (skortype == Scoresenum.threestar)
        {
            firststar.SetActive(true);
            secondstar.SetActive(true);
            thirdstar.SetActive(true);
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
        loadlevel();
        
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
