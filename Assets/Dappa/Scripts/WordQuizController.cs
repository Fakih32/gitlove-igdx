using UnityEngine;
using UnityEngine.UI;

// ROMBAK dari GameManager.cs.
// Perubahan dari versi sebelumnya:
// - Update() dihapus total, karena kondisi "waktu habis" sekarang ditangani
//   terpusat oleh LevelSessionManager.HandleTimeUp(), bukan dicek manual di sini
// - Tipe data QuizData -> WordQuizData mengikuti rename
public class WordQuizController : MonoBehaviour {
    [Header("Data & Referensi Scene")]
    public WordQuizData quizData;
    public GameObject letterFieldPrefab;
    public GameObject letterButtonPrefab;
    public Transform letterFieldParent;
    public Transform letterButtonsParent;
    public Image quizImage;

    //[Header("Sound Effects")]
    //public AudioClip clickSfx;
    //public AudioClip correctSfx;
    //public AudioClip wrongSfx;

    [Header("Konfigurasi Mekanik Ini")]
    public int questionsPerLevel = 2;

    private Text[] letterFields;
    private Button[] letterButtons;
    private int currentFieldIndex = 0;
    private int currentQuizIndex = 0;
    private int questionsAnswered = 0;
    private bool isAnswering = true;

    void Start() {
        LoadQuiz(currentQuizIndex);
    }

    void LoadQuiz(int quizIndex) {
        ResetGame();

        if (quizData == null || quizData.quizzes == null || quizData.quizzes.Length == 0) {
            Debug.LogError("Quiz Data belum disetup dengan benar");
            return;
        }

        WordQuizData.Quiz quiz = quizData.quizzes[quizIndex % quizData.quizzes.Length];

        if (quizImage == null) {
            Debug.LogError("Quiz Image belum di-assign di Inspector");
            return;
        }
        quizImage.sprite = quiz.image;

        CreateLetterFields(quiz.correctWord.Length);
        CreateLetterButtons(quiz.correctWord);

        isAnswering = true;
    }

    void CreateLetterFields(int fieldCount) {
        foreach (Transform child in letterFieldParent) {
            Destroy(child.gameObject);
        }

        letterFields = new Text[fieldCount];

        for (int i = 0; i < fieldCount; i++) {
            GameObject field = Instantiate(letterFieldPrefab, letterFieldParent);
            letterFields[i] = field.GetComponentInChildren<Text>();
            if (letterFields[i] == null) {
                Debug.LogError("Letter field prefab tidak punya komponen Text");
            }
        }
    }

    void CreateLetterButtons(string correctWord) {
        foreach (Transform child in letterButtonsParent) {
            Destroy(child.gameObject);
        }

        letterButtons = new Button[8];

        char[] correctLetters = correctWord.ToCharArray();
        char[] wrongLetters = GenerateRandomLetters(8 - correctLetters.Length, correctLetters);

        char[] allLetters = new char[8];
        correctLetters.CopyTo(allLetters, 0);
        wrongLetters.CopyTo(allLetters, correctLetters.Length);

        ShuffleLetters(allLetters);

        for (int i = 0; i < allLetters.Length; i++) {
            GameObject button = Instantiate(letterButtonPrefab, letterButtonsParent);
            button.GetComponentInChildren<Text>().text = allLetters[i].ToString();

            int index = i;
            button.GetComponent<Button>().onClick.AddListener(() => OnLetterButtonClick(index));

            letterButtons[i] = button.GetComponent<Button>();
            if (letterButtons[i] == null) {
                Debug.LogError("Letter Button prefab tidak punya komponen Button");
            }
        }
    }

    void OnLetterButtonClick(int buttonIndex) {
        if (!isAnswering || currentFieldIndex >= letterFields.Length) return;

        //AudioManager.Instance?.PlaySfx(clickSfx);

        string letter = letterButtons[buttonIndex].GetComponentInChildren<Text>().text;
        letterFields[currentFieldIndex].text = letter;
        letterButtons[buttonIndex].interactable = false;
        currentFieldIndex++;
    }

    public void DeleteLastLetter() {
        if (currentFieldIndex <= 0) return;

        currentFieldIndex--;
        string deletedLetter = letterFields[currentFieldIndex].text;

        foreach (Button button in letterButtons) {
            if (!button.interactable && button.GetComponentInChildren<Text>().text == deletedLetter) {
                button.interactable = true;
                break;
            }
        }

        letterFields[currentFieldIndex].text = "";
    }

    public void CheckReply() {
        if (!isAnswering) return;

        string playerReply = "";
        foreach (Text field in letterFields) {
            if (field == null) {
                Debug.LogError("Letter field kosong/null");
                return;
            }
            playerReply += field.text;
        }

        bool isCorrect = playerReply == quizData.quizzes[currentQuizIndex % quizData.quizzes.Length].correctWord;

        if (isCorrect) {
            isAnswering = false; // kunci input, cuma kalau udah benar

            //AudioManager.Instance?.PlaySfx(correctSfx);
            foreach (Text field in letterFields) field.color = Color.green;
            LevelSessionManager.Instance?.AddScore(100);

            Invoke(nameof(NextQuiz), 1.5f); // lanjut ke soal berikutnya cuma di sini
        } else {
            //AudioManager.Instance?.PlaySfx(wrongSfx);
            foreach (Text field in letterFields) field.color = Color.red;
            // isAnswering tetap true -> pemain masih bisa pencet Delete lalu coba lagi
        }
    }

    void NextQuiz() {
        questionsAnswered++;
        currentQuizIndex++;

        if (questionsAnswered < questionsPerLevel) {
            LoadQuiz(currentQuizIndex);
        } else {
            LevelSessionManager.Instance?.OnMechanicComplete();
        }
    }

    char[] GenerateRandomLetters(int count, char[] excludeLetters) {
        char[] randomLetters = new char[count];
        for (int i = 0; i < count; i++) {
            char randomLetter;
            do {
                randomLetter = (char)('A' + Random.Range(0, 26));
            } while (System.Array.Exists(excludeLetters, c => c == randomLetter));
            randomLetters[i] = randomLetter;
        }
        return randomLetters;
    }

    void ShuffleLetters(char[] letters) {
        for (int i = letters.Length - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            (letters[i], letters[randomIndex]) = (letters[randomIndex], letters[i]);
        }
    }

    void ResetGame() {
        currentFieldIndex = 0;

        if (letterFields != null) {
            foreach (Text field in letterFields) {
                if (field != null) {
                    field.text = "";
                    field.color = Color.white;
                }
            }
        }

        if (letterButtons != null) {
            foreach (Button button in letterButtons) {
                if (button != null) button.interactable = true;
            }
        }
    }
}