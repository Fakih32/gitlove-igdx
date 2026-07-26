using UnityEngine;
using System.Collections.Generic;
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
    [Header("Poin Yang Didapat")]
    [HideInInspector] public float currentpoint = 0;
    public float poin_got;
    public float poin_waktu;
    [Header("Waktu Yang diperlukan mencapai poin tambahan")]
    public float time_need;
    [Header("Poin Minimal")]
    public float onestarpoint;
    public float twostarpoint;
    public float threestarpoint;

    [Header("Game Objek Yang Ingin Di drag")]
    public List<GameObject> dragobject;
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
        
        if (currentpoint< onestarpoint){
            Gamelost();
        }

       if (currentpoint >= onestarpoint && currentpoint< twostarpoint)
        {
             skortype = Scoresenum.onestar; 
             Gamewin();
             
        }
         else if (currentpoint >= twostarpoint && currentpoint< threestarpoint)
        {
             skortype = Scoresenum.twostar;
             Gamewin(); 
             
        }
         else if (currentpoint >= threestarpoint)
        {
             skortype = Scoresenum.threestar; 
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
public void Addpointpertimer(){
     InvokeRepeating("addpointtime", 1f ,time_need);
     Debug.Log(currentpoint); 
}
    void addpointtime(){
        if (TimerScript.instance.timer> time_need){
            currentpoint += poin_waktu;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        tujuanmax = Listtujuan.Count;
      
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TimerScript.instance.timer> 0f){
          
        
        if (tujuancount == tujuanmax){
            skortype = Scoresenum.threestar; 
            Gamewin();
            TimerScript.instance.timer = 0f;
            
        }
        }
        else if (TimerScript.instance.timer<= 0f){
            Checkwin();
        }
        
    }
}
