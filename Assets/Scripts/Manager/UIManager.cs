using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject panel;

    public void ShowPanel()
    {
        if(panel)
        {
            panel.SetActive(true);
        }
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
