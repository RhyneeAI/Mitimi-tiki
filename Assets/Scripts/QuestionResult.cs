using System;

[System.Serializable]
public class QuestionResult
{
    public int questionNumber;
    public string question;

    public int answerA;
    public int answerB;
    public int answerC;
    public int answerD;

    public int correctAnswer;
    public int playerAnswer;

    public float pi;
    public int score;
    public int level;
    public int life;

    public float time;
    public float timeUsed;

    public bool isCorrect;
    public bool isTimeout;

    public string created;
}