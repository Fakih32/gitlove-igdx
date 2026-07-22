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
    private bool isGameActice = true;

    private void Start() {
        LoadQuiz(currentQuizIndex);
    }

    private void Update() {
        if (isGameActice)
            UpdateTimer();
    }

    private void LoadQuiz(int quizIndex) {
        Debug.Log("Loading Quiz: " + quizIndex);

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

        //CreateLetterFields(quiz.correctWord.Length);
        //CreateLetterButtons(quiz.correctWord);

        timeRemaining = timeLimit;
        isGameActice = true;
    }

    private void CreateLetterFields() {

    }

    private void CreateLetterButtons() {

    }

    private void UpdateTimer() {

    }
}