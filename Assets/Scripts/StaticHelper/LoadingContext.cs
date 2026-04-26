public static class LoadingContext
{
    public static string targetScene    = "Game"; // default seperti semula
    public static bool waitForFirebase  = false;
    public static bool firebaseDone     = false;

    public static void PrepareLoad(string scene, bool needFirebase = false)
    {
        targetScene     = scene;
        waitForFirebase = needFirebase;
        firebaseDone    = false;
    }

    public static void NotifyFirebaseDone()
    {
        firebaseDone = true;
    }

    // Reset ke default (opsional, bisa dipanggil saat masuk Home)
    public static void Reset()
    {
        targetScene    = "Game";
        waitForFirebase = false;
        firebaseDone   = false;
    }
}