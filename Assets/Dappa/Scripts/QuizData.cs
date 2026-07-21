using UnityEngine;

[CreateAssetMenu(fileName = "QuizData", menuName = "Word Quiz/Quiz Data")]
public class QuizData : ScriptableObject {
    [System.Serializable]
    public class Quiz {
        public Sprite image;
        public string correctWord;
    }

    public Quiz[] quizzes;
}