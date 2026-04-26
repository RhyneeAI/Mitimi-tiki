using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingList : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private RankingListItem itemPrefab;

    void OnEnable()
    {
        BuildFromCache();
    }

    private void BuildFromCache()
    {
        // bersihkan dulu
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        List<RankingEntryData> dataList = LeaderboardContext.cachedEntries;

        if (dataList == null || dataList.Count == 0)
        {
            Debug.Log("[RankingList] No leaderboard data in cache.");
            return;
        }

        for (int i = 0; i < dataList.Count; i++)
        {
            RankingListItem item = Instantiate(itemPrefab, contentParent, false);
            item.gameObject.SetActive(true);
            item.name = $"#{i + 1}";
            item.Setup(i + 1, dataList[i]);
            item.transform.SetSiblingIndex(i);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentParent);
    }
}