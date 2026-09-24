using UnityEngine;
using UnityEngine.UI;

// ROMBAK dari versi sebelumnya.
// Perubahan: field "questionsPerLevel" yang di-set manual di Inspector
// DIHAPUS. Sebelumnya ini duplikasi data dari WordQuizData -- dua tempat
// nyimpen "berapa soal per level" yang harus disinkronkan manual, dan
// begitu lupa disamakan, soal jadi berulang (modulo) sebelum akhirnya
// OnMechanicComplete() kepanggil. Sekarang jumlah soal SELALU sama
// dengan currentLevelQuizzes.Length -- pemain menjawab semua soal yang
// ada di WordQuizData untuk level itu, tidak lebih tidak kurang, dan
// tidak ada modulo/pengulangan sama sekali. Level design bisa bebas
// atur 2, 3, 5, atau berapa pun soal per level cukup lewat WordQuizData,
// tanpa perlu sentuh Inspector WordQuizController lagi.
public class WordQuizController : MonoBehaviour {
    [Header("Data & Referensi Scene")]
    public WordQuizData quizData;
    public GameObject letterFieldPrefab;
    public GameObject letterButtonPrefab;
    public Transform letterFieldParent;
    public Transform letterButtonsParent;
    public Image quizImage;
    public Image backgroundImage;

    private WordQuizData.Quiz[] currentLevelQuizzes;
    private int questionsPerLevel;
    private Text[] letterFields;
    private Button[] letterButtons;
    private int currentFieldIndex = 0;
    private int currentQuizIndex = 0;
    private int questionsAnswered = 0;
    private bool isAnswering = true;

    void Start() {
        int levelIndex = ResolveCurrentLevelIndex(LevelSessionManager.Instance?.currentLevel);
        currentLevelQuizzes = quizData != null ? quizData.GetQuizzesForLevel(levelIndex) : null;

        if (currentLevelQuizzes == null || currentLevelQuizzes.Length == 0) {
            Debug.LogError($"WordQuizController: tidak ada quiz untuk level index {levelIndex} di WordQuizData");
            return;
        }

        questionsPerLevel = GetQuestionsPerLevel(currentLevelQuizzes);

        ApplyBackground(levelIndex);
        LoadQuiz(currentQuizIndex);
    }

    void ApplyBackground(int levelIndex) {
        if (backgroundImage == null) {
            Debug.LogWarning("WordQuizController: backgroundImage tidak di-assign di Inspector, dilewati.");
            return;
        }

        Sprite background = quizData.GetBackgroundForLevel(levelIndex);
        if (background != null) {
            backgroundImage.sprite = background;
        }
    }

    void LoadQuiz(int quizIndex) {
        ResetGame();

        // Tidak lagi pakai modulo -- quizIndex selalu valid karena
        // questionsPerLevel == currentLevelQuizzes.Length, jadi loop
        // NextQuiz() akan berhenti tepat sebelum index keluar batas.
        WordQuizData.Quiz quiz = currentLevelQuizzes[quizIndex];

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

        int totalSlots = Mathf.Max(9, correctWord.Length);
        letterButtons = new Button[totalSlots];

        char[] correctLetters = correctWord.ToCharArray();
        char[] wrongLetters = GenerateRandomLetters(totalSlots - correctLetters.Length, correctLetters);

        char[] allLetters = new char[totalSlots];
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

        string letter = letterButtons[buttonIndex].GetComponentInChildren<Text>().text;
        letterFields[currentFieldIndex].text = letter;
        letterButtons[buttonIndex].interactable = false;
        currentFieldIndex++;
         AudioManager.Instance?.PlaySfx(AudioScript.instance.Clicking);
    }

    public void DeleteLastLetter() {
        if (currentFieldIndex <= 0) return;
        AudioManager.Instance?.PlaySfx(AudioScript.instance.Clicking);
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

        bool isCorrect = playerReply == currentLevelQuizzes[currentQuizIndex].correctWord;

        if (isCorrect) {
            isAnswering = false;

            foreach (Text field in letterFields) field.color = Color.green;
            LevelSessionManager.Instance?.AddScore(100);
             AudioManager.Instance?.PlaySfx(AudioScript.instance.correctaudio);
            Invoke(nameof(NextQuiz), 1.5f);
        } else {
            foreach (Text field in letterFields) field.color = Color.red;
            AudioManager.Instance?.PlaySfx(AudioScript.instance.wronganswer);
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

    public static int ResolveCurrentLevelIndex(LevelData currentLevel) {
        if (currentLevel == null) {
            Debug.LogWarning("WordQuizController: LevelSessionManager.currentLevel null, fallback ke level index 0");
            return 0;
        }
        return currentLevel.levelIndex;
    }

    public static int GetQuestionsPerLevel(WordQuizData.Quiz[] quizzes) {
        return quizzes?.Length ?? 0;
    }
}