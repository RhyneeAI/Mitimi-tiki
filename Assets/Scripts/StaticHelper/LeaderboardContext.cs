using System.Collections.Generic;

public static class LeaderboardContext
{
    // Data mentah yang sudah di-load dari Firebase
    public static List<RankingEntryData> cachedEntries = new List<RankingEntryData>();
}