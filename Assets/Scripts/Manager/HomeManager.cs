using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class HomeManager: MonoBehaviour
{
    [SerializeField] private FirebaseManager firebaseManager;

    public void GoToLeaderboard()
    {
        if (firebaseManager == null)
            firebaseManager = FirebaseManager.Instance;

        if (firebaseManager == null)
        {
            Debug.LogError("[HomeManager] FirebaseManager not found.");
            // fallback: langsung ke leaderboard tanpa data (atau pakai data lama)
            LoadingContext.PrepareLoad("Ranking", false);
            SceneManager.LoadScene("Loading");
            return;
        }

        // 1) Set target scene dan minta LoadingManager menunggu Firebase
        LoadingContext.PrepareLoad("Ranking", true);
        LoadingContext.firebaseDone = false;

        // 2) Mulai proses load leaderboard (tanpa bikin UI di scene ini)
        firebaseManager.LoadLeaderboard(entries =>
        {
            // Simpan ke context global
            LeaderboardContext.cachedEntries = entries ?? new List<RankingEntryData>();

            // Beri tahu LoadingManager bahwa load leaderboard sudah selesai
            LoadingContext.NotifyFirebaseDone();
        });

        // 3) Pindah ke scene Loading
        SceneManager.LoadScene("Loading");
    }
}