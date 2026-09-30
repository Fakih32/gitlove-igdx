using System.Collections;
using UnityEngine;

public class WinAndLoseEffect : MonoBehaviour
{
    public static WinAndLoseEffect instance;
    [SerializeField] public GameObject WinEffect;
     [SerializeField] public GameObject Lostffect;
    [Header("Tempat Masuk Keluar")]
    public Transform winstart;
    public Transform tengahmenang;
    public Transform tengahkalah;
    public Transform loststart;
    public Transform lostend;
    public Transform winend;
    [Header("durasi")]
    public float durasiefekmasuk;
     public float durasiefektengah ;
     public float durasikeluar;
    [Header("Kecepatan")]
    public int Kecepatanefek;
    void Awake()
    {
        
     if (instance != null && instance != this)
      {
        Destroy(gameObject);
      
       }
        else
        {
              instance = this;
        }
   
     

    }
    public Coroutine efekmenang()
    {
        if (WinEffect != null) WinEffect.SetActive(true);
        return StartCoroutine(wineffectnumerator(durasiefekmasuk,durasiefektengah,durasikeluar));
    }
    public void efekkalah()
    {
        if (Lostffect != null) Lostffect.SetActive(true);
        StartCoroutine(Losteffectnumerator(durasiefekmasuk,durasiefektengah,durasikeluar));
    }
    IEnumerator wineffectnumerator(float insecond,float slient,float outsecond)
    {
        float speed = Kecepatanefek > 0 ? Kecepatanefek : 500f; // fallback kecepatan
        
        // Pindah ke tengah secara perlahan
        while (Vector2.Distance(WinEffect.transform.position, tengahmenang.position) > 0.1f)
        {
            WinEffect.transform.position = Vector2.MoveTowards(WinEffect.transform.position, tengahmenang.position, speed * Time.deltaTime);
            yield return null;
        }
        
        WinEffect.transform.position = tengahmenang.position;
        AudioManager.Instance?.PlaySfx(AudioScript.instance.arkanbenar);
        
        yield return new WaitForSeconds(slient);
        
        // Pindah ke akhir secara perlahan
        while (Vector2.Distance(WinEffect.transform.position, winend.position) > 0.1f)
        {
            WinEffect.transform.position = Vector2.MoveTowards(WinEffect.transform.position, winend.position, speed * Time.deltaTime);
            yield return null;
        }
        
        WinEffect.transform.position = winstart.position;
        
        if (WinEffect != null) WinEffect.SetActive(false);
    }

     IEnumerator Losteffectnumerator(float insecond,float slient,float outsecond)
    {
        float speed = Kecepatanefek > 0 ? Kecepatanefek : 500f; // fallback kecepatan
        
        // Pindah ke tengah secara perlahan
        while (Vector2.Distance(Lostffect.transform.position, tengahkalah.position) > 0.1f)
        {
            Lostffect.transform.position = Vector2.MoveTowards(Lostffect.transform.position, tengahkalah.position, speed * Time.deltaTime);
            yield return null;
        }
        
        Lostffect.transform.position = tengahkalah.position;
        AudioManager.Instance?.PlaySfx(AudioScript.instance.kakeksalah);
        
        yield return new WaitForSeconds(slient);
        
        // Pindah ke akhir secara perlahan
        while (Vector2.Distance(Lostffect.transform.position, lostend.position) > 0.1f) // asumsi menggunakan lostend
        {
            Lostffect.transform.position = Vector2.MoveTowards(Lostffect.transform.position, lostend.position, speed * Time.deltaTime);
            yield return null;
        }
        
        Lostffect.transform.position = loststart.position;
        
        if (Lostffect != null) Lostffect.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
}
