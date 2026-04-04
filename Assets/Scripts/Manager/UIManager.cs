using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject mainPanel;

    public void ShowPanel()
    {
        mainPanel.SetActive(true);
    }

    public void HidePanel()
    {
        mainPanel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
