using UnityEngine;
using UnityEngine.UI;

public class WordQuizController : MonoBehaviour {
    [Header("Data & Referensi Scene")]
    public WordQuizData quizData;
    public GameObject letterFieldPrefab;
    public GameObject letterButtonPrefab;
    public Transform letterFieldParent;
    public Transform letterButtonsParent;
    public Image quizImage;

    [Header("Konfigurasi Mekanik Ini")]
    public int questionsPerLevel = 2;

    private WordQuizData.Quiz[] currentLevelQuizzes;
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

        if (questionsPerLevel > currentLevelQuizzes.Length) {
            Debug.LogWarning($"WordQuizController: questionsPerLevel ({questionsPerLevel}) lebih besar dari jumlah quiz yang tersedia ({currentLevelQuizzes.Length}) untuk level ini -- soal akan berulang.");
        }

        LoadQuiz(currentQuizIndex);
    }

    void LoadQuiz(int quizIndex) {
        ResetGame();

        if (currentLevelQuizzes == null || currentLevelQuizzes.Length == 0) {
            Debug.LogError("Quiz Data belum disetup dengan benar");
            return;
        }

        WordQuizData.Quiz quiz = currentLevelQuizzes[quizIndex % currentLevelQuizzes.Length];

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

        letterButtons = new Button[9];

        char[] correctLetters = correctWord.ToCharArray();
        char[] wrongLetters = GenerateRandomLetters(9 - correctLetters.Length, correctLetters);

        char[] allLetters = new char[9];
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

        bool isCorrect = playerReply == currentLevelQuizzes[currentQuizIndex % currentLevelQuizzes.Length].correctWord;

        if (isCorrect) {
            isAnswering = false;

            foreach (Text field in letterFields) field.color = Color.green;
            LevelSessionManager.Instance?.AddScore(100);

            Invoke(nameof(NextQuiz), 1.5f);
        } else {
            foreach (Text field in letterFields) field.color = Color.red;
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
}