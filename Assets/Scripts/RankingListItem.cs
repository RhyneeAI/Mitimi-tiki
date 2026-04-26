using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingListItem : MonoBehaviour
{
    [Header("Rank Item")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private Image rankIcon;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite silverIcon;
    [SerializeField] private Sprite bronzeIcon;

    [Header("Fields")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text totalQuestionText;
    [SerializeField] private TMP_Text totalCorrectText;
    [SerializeField] private TMP_Text scoreText;

    public void Setup(int rank, RankingEntryData data)
    {
        if (playerNameText != null)
            playerNameText.text = data.playerName;

        if (dateText != null)
            dateText.text = data.playedAt;

        if (levelText != null)
            levelText.text = "Lv. " + data.finalLevel;

        if (totalQuestionText != null)
            totalQuestionText.text = data.totalQuestion.ToString();

        if (totalCorrectText != null)
            totalCorrectText.text = data.totalCorrect.ToString();

        if (scoreText != null)
            scoreText.text = FormatScore(data.finalScore);

        SetupRank(rank);
    }

    private void SetupRank(int rank)
    {
        bool useIcon = rank >= 1 && rank <= 3;

        if (rankIcon != null)
            rankIcon.gameObject.SetActive(useIcon);

        if (rankText != null)
            rankText.gameObject.SetActive(!useIcon);

        if (useIcon && rankIcon != null)
        {
            switch (rank)
            {
                case 1:
                    rankIcon.sprite = goldIcon;
                    break;
                case 2:
                    rankIcon.sprite = silverIcon;
                    break;
                case 3:
                    rankIcon.sprite = bronzeIcon;
                    break;
            }
        }
        else if (rankText != null)
        {
            rankText.text = rank.ToString();
        }
    }

    private string FormatScore(int value)
    {
        if (value >= 1000000000)
            return (value / 1000000000f).ToString("0.#") + "B";

        if (value >= 1000000)
            return (value / 1000000f).ToString("0.#") + "M";

        if (value >= 1000)
            return (value / 1000f).ToString("0.#") + "K";

        return value.ToString();
    }
}