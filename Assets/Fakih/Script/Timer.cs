using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    public static TimerScript instance;
    public float timer = 10f;
    public Slider timeslide;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeslide.maxValue = timer;
        timeslide.value = timer;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timeslide != null)
        {
            timeslide.value = timer;
        }
        
    }
}
