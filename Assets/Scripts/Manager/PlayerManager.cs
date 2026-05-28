using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// #if !UNITY_WEBGL || UNITY_EDITOR
// using Firebase.Firestore;
// #endif

public static class PlayerManager
{
    public static string playerName = "Guest";
    public static string userId = "";
    public static bool isAuthenticated = false;

    private static string _playerName = "Tiki-" + Random.Range(1, 99);

    private const int MaxLength = 10;

    // Karakter yang diizinkan: huruf, angka, spasi, underscore, titik, tanda hubung
    private const string AllowedPattern = @"^[a-zA-Z0-9 _.\-]+$";

    public static string GetPlayerName() => _playerName;

    public static string SetPlayerName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Nama tidak boleh kosong.";

        input = input.Trim();

        if (input.Length < 1)
            return "Nama tidak boleh kosong.";

        if (input.Length > MaxLength)
            return $"Nama maksimal {MaxLength} karakter.";

        if (!System.Text.RegularExpressions.Regex.IsMatch(input, AllowedPattern))
            return "Nama hanya boleh huruf, angka, spasi, . _ -";

        _playerName = input;
        PlayerPrefs.SetString("LastPlayerName", input);
        PlayerPrefs.Save();
        return null; 
    }

    public static void SetAuthenticatedUser(string uid, string displayName)
    {
        userId = uid;
        playerName = displayName;
        isAuthenticated = true;
    }

    public static void Clear()
    {
        playerName = "Guest";
        userId = "";
        isAuthenticated = false;
    }
}