using UnityEngine;
using UnityEngine.UI;

// ROMBAK dari Timer.cs (class TimerScript).
// Perubahan utama: script ini TIDAK LAGI punya angka timer sendiri.
// Dia cuma "membaca" LevelSessionManager.Instance.timeRemaining
// setiap frame dan menampilkannya. Karena sumber datanya persisten,
// tampilan ini otomatis lanjut walau scene berganti.
public class TimerDisplay : MonoBehaviour {
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Text timerText;

    void Update() {
        if (LevelSessionManager.Instance == null || LevelSessionManager.Instance.currentLevel == null) return;

        float timeRemaining = LevelSessionManager.Instance.timeRemaining;
        float timeLimit = LevelSessionManager.Instance.currentLevel.timeLimit;

        if (timerFillImage != null) {
            timerFillImage.fillAmount = Mathf.Clamp01(timeRemaining / timeLimit);
        }

        if (timerText != null) {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }
}
