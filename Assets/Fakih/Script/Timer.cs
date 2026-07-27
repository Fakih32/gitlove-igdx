using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    public static TimerScript instance;

    [SerializeField] private Image timerImage;
    [SerializeField] private float duration = 10f;

    [HideInInspector]public float timer;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        timer = duration;
        timerImage.fillAmount = 1f;
    }

    private void Start()
    {
        if (DraganddropLevelHandler.instance != null && DraganddropLevelHandler.instance.timer > 0f)
        {
            duration = DraganddropLevelHandler.instance.timer;
            timer = duration;
        }
        else
        {
            timer = duration;
        }

        UpdateTimerFill();
    }

    private void Update()
    {
        if (timer <= 0f)
            return;

        timer -= Time.deltaTime;
        timer = Mathf.Max(timer, 0f);

        UpdateTimerFill();

        if (timer <= 0f)
        {
            timer = 0f;
            UpdateTimerFill();
            Debug.Log("Timer Finished");
        }
    }

    private void UpdateTimerFill()
    {
        if (timerImage != null)
        {
            timerImage.fillAmount = Mathf.Clamp01(timer / duration);
        }
    }
}