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

    private void Start() {
        LoadQuiz(currentQuizIndex);
    }

    private void Update() {
        if(isGameActive)
            UpdateTimer();
    }

    private void LoadQuiz(int quizIndex) {
        Debug.Log("Loading Quiz: " + quizIndex);

        if(quizData == null || quizData.quizzes == null || quizData.quizzes.Length == 0) {
            Debug.LogError("Quiz Data or quizzes is not setup correctly");
            return;
        }

        if(quizIndex < 0 || quizIndex >= quizData.quizzes.Length) {
            Debug.LogError("Invalid quiz index: " + quizIndex);
            return;
        }

        QuizData.Quiz quiz = quizData.quizzes[quizIndex];

        Image image = GameObject.Find("QuizImage")?.GetComponent<Image>();
        if(image == null) {
            Debug.LogError("Quiz Image not found or missing idk");
            return;
        }
        image.sprite = quiz.image;

        CreateLetterFields(quiz.correctWord.Length);
        //CreateLetterButtons(quiz.correctWord);

        timeRemaining = timeLimit;
        isGameActive = true;
    }

    private void CreateLetterFields(int fieldCount) {
        Debug.Log("Creating Letter Fields: " + fieldCount);

        foreach(Transform child in letterFieldParent) {
            Destroy(child.gameObject);
        }

        letterFields = new Text[fieldCount];

        for(int i = 0; i < fieldCount; i++) {
            GameObject field = Instantiate(letterFieldPrefab, letterFieldParent);
            letterFields[i] = field.GetComponentInChildren<Text>();
            if(letterFields[i] == null) {
                Debug.LogError("Letter field prefab is missing a text component");
            }
        }
    }

    private void CreateLetterButtons(string correctWord) {
        Debug.Log("Creating Letter Buttons");

        foreach(Transform child in letterButtonsParent) {
            Destroy(child.gameObject);
        }

        letterButtons = new Button[0];

        char[] correctLetters = correctWord.ToCharArray();
        //char[] wrongLetters = GenerateRandomLetters(8 - correctLetters.Length, correctLetters);

        char[] allLetters = new char[8];
        correctLetters.CopyTo(allLetters, 0);
        //wrongLetters.CopyTo(allLetters, correctLetters.Length);
    }

    char[] GenerateRandomLetters(int count, char[] excludeLetters) {
        char[] randomLetters = new char[count];
        for(int i = 0; i < count; i++) {
            char randomLetter;

            do {
                randomLetter = (char)('A' + Random.Range(0, 26));
            }
            while(System.Array.Exists(excludeLetters, c  => c == randomLetter));
            randomLetters[i] = randomLetter;
        }

        return randomLetters;
    }

    private void UpdateTimer() {

    }
}