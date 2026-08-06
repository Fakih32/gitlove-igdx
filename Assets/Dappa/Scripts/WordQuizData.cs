using UnityEngine;

[CreateAssetMenu(fileName = "WordQuizData", menuName = "Word Quiz/Quiz Data")]
public class WordQuizData : ScriptableObject {
    [System.Serializable]
    public class Quiz {
        public Sprite image;
        public string correctWord;
    }
    public Quiz[] quizzes;
}