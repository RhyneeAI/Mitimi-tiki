using System.Collections.Generic;

public static class SessionResultData
{
    public static bool shouldShowResultPanel = false;
    public static int finalScore = 0;
    public static int finalLevel = 1;
    public static int totalQuestion = 0;
    public static int totalCorrect = 0;
    public static float performanceIndex = 0;
    public static List<QuestionResult> questionHistory = new List<QuestionResult>();

    public static void SetResult(int score, int level, int question, int correct, float pi, List<QuestionResult> history)
    {
        shouldShowResultPanel = true;
        finalScore = score;
        finalLevel = level;
        totalQuestion = question;
        totalCorrect = correct;
        performanceIndex = pi;
        questionHistory = new List<QuestionResult>(history);
    }

    public static void Clear()
    {
        shouldShowResultPanel = false;
        finalScore = 0;
        finalLevel = 1;
        totalQuestion = 0;
        totalCorrect = 0;
        performanceIndex = 0;
        questionHistory.Clear();
    }
}