using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public QuizData quizData;

    public GameObject letterFieldPrefab;
    public GameObject letterButtonPrefab;

    public Transform letterFieldParent;
    public Transform letterButtonsParent;

    public Text timerText;
    public float timeLimit = 30f;

    private Text[] letterFields;
    private Button[] letterButtons;

    private int currentFieldIndex = 0;
    private int currentQuizIndex = 0;

    private float timeRemaining;
    private bool isGameActive = true;

    void Start() {
        LoadQuiz(currentQuizIndex);
    }

    void Update() {
        if (isGameActive)
            UpdateTimer();
    }

    void LoadQuiz(int quizIndex) {
        Debug.Log("Loading Quiz: " + quizIndex);

        ResetGame();

        if (quizData == null || quizData.quizzes == null || quizData.quizzes.Length == 0) {
            Debug.LogError("Quiz Data or quizzes is not setup correctly");
            return;
        }

        if (quizIndex < 0 || quizIndex >= quizData.quizzes.Length) {
            Debug.LogError("Invalid quiz index: " + quizIndex);
            return;
        }

        QuizData.Quiz quiz = quizData.quizzes[quizIndex];

        Image image = GameObject.Find("QuizImage")?.GetComponent<Image>();
        if (image == null) {
            Debug.LogError("Quiz Image not found or missing idk");
            return;
        }
        image.sprite = quiz.image;

        CreateLetterFields(quiz.correctWord.Length);
        CreateLetterButtons(quiz.correctWord);

        timeRemaining = timeLimit;
        isGameActive = true;
    }

    void CreateLetterFields(int fieldCount) {
        Debug.Log("Creating Letter Fields: " + fieldCount);

        foreach (Transform child in letterFieldParent) {
            Destroy(child.gameObject);
        }

        letterFields = new Text[fieldCount];

        for (int i = 0; i < fieldCount; i++) {
            GameObject field = Instantiate(letterFieldPrefab, letterFieldParent);
            letterFields[i] = field.GetComponentInChildren<Text>();
            if (letterFields[i] == null) {
                Debug.LogError("Letter field prefab is missing a text component");
            }
        }
    }

    void CreateLetterButtons(string correctWord) {
        Debug.Log("Creating Letter Buttons");

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
                Debug.Log("Letter Button Prefab is missing button component!");
            }
        }
    }

    void OnLetterButtonClick(int buttonIndex) {
        if (currentFieldIndex < letterFields.Length) {
            string letter = letterButtons[buttonIndex].GetComponentInChildren<Text>().text;

            letterFields[currentFieldIndex].text = letter;

            letterButtons[buttonIndex].interactable = false;

            currentFieldIndex++;
        }
    }

    char[] GenerateRandomLetters(int count, char[] excludeLetters) {
        char[] randomLetters = new char[count];
        for (int i = 0; i < count; i++) {
            char randomLetter;

            do {
                randomLetter = (char)('A' + Random.Range(0, 26));
            }
            while(System.Array.Exists(excludeLetters, c  => c == randomLetter));
            randomLetters[i] = randomLetter;
        }

        return randomLetters;
    }

    void ShuffleLetters(char[] letters) {
        for (int i = letters.Length - 1; i > 0; i--) {
            int randomIndex = Random.Range(0, i + 1);
            char temp = letters[i];
            letters[i] = letters[randomIndex];
            letters[randomIndex] = temp;
        }
    }

    public void CheckReply() {
        string playerReply = "";

        foreach (Text field in letterFields) {
            if (field == null) {
                Debug.LogError("Letter field is null");
                return;
            }

            playerReply += field.text;
        }

        if (playerReply == quizData.quizzes[currentQuizIndex].correctWord) {
            Debug.Log("Correct!");

            foreach (Text field in letterFields) {
                field.color = Color.green;
            }

            isGameActive = false;
            Invoke("NextQuiz", 2f);
        }
        
        else {
            Debug.Log("Incorrect!");

            foreach (Text field in letterFields) {
                field.color = Color.red;
            }            
        }
    }

    void NextQuiz() {
        currentQuizIndex++;

        if (currentQuizIndex < quizData.quizzes.Length) {
            LoadQuiz(currentQuizIndex);
        }
        
        else {
            Debug.Log("Game Over,all quizzes completed");
        }
    }

    public void DeleteLastLetter() {
        if (currentFieldIndex > 0) {
            currentFieldIndex--;

            string deletedLetter = letterFields[currentFieldIndex].text;
            foreach (Button button in letterButtons) {
                if (button.GetComponentInChildren<Text>().text == deletedLetter && !button.interactable) {
                    button.interactable = true;
                    break;
                }
            }

            letterFields[currentFieldIndex].text = "";
        }
    }

    void ResetGame() {
        Debug.Log("Resetting Game");

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
                if (button != null) {
                    button.interactable = true;
                }
            }
        }

        currentFieldIndex = 0;
    }

    void UpdateTimer() {
        if (timeRemaining > 0) {
            timeRemaining -= Time.deltaTime;
            timerText.text = "" + Mathf.CeilToInt(timeRemaining).ToString();
        }
        
        else {
            Debug.Log("Time's up!");
            isGameActive = false;
            CheckReply();
        }
    }
}