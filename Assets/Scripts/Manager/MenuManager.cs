using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_Text PanelName;
    public void setPanelName(string name) 
    {
        PanelName.text = name;
    }

    public void loadScene(string sceneName) 
    {
        SceneManager.LoadScene(sceneName);
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
