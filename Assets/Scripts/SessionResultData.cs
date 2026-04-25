public static class SessionResultData
{
    public static bool shouldShowResultPanel = false;
    public static int finalScore = 0;
    public static int finalLevel = 1;
    public static int totalQuestion = 0;
    public static int totalCorrect = 0;

    public static void SetResult(int score, int level, int question, int correct)
    {
        shouldShowResultPanel = true;
        finalScore = score;
        finalLevel = level;
        totalQuestion = question;
        totalCorrect = correct;
    }

    public static void Clear()
    {
        shouldShowResultPanel = false;
        finalScore = 0;
        finalLevel = 1;
        totalQuestion = 0;
        totalCorrect = 0;
    }
}