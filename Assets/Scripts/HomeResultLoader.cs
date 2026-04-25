using TMPro;
using UnityEngine;

public class HomeResultLoader : MonoBehaviour
{
    public UIManager uiManager;
    public GameObject resultPanel;

    public TMP_Text scoreText;
    public TMP_Text levelText;
    public TMP_Text questionText;
    public TMP_Text correctText;

    void Start()
    {
        if (SessionResultData.shouldShowResultPanel)
        {
            scoreText.text = "Score : " + SessionResultData.finalScore;
            levelText.text = "Lv " + SessionResultData.finalLevel;
            questionText.text = SessionResultData.totalQuestion.ToString();
            correctText.text = SessionResultData.totalCorrect.ToString();

            if (uiManager != null)
            {
                uiManager.ShowPanel(resultPanel);
            }

            SessionResultData.Clear();
        }
    }
}