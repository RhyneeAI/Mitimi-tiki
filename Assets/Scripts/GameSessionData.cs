using System.Collections.Generic;

[System.Serializable]
public class GameSessionData
{
    public string playerName;
    public float pi;
    public int finalScore;
    public int finalLevel;
    public int totalQuestion;
    public int totalCorrect;
    public string playedAt;

    public List<QuestionResult> questions;
}