using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject[] subPanels;
    public GameObject backButton;   
    public GameObject nextIcon;     

    private int currentIndex = 0;

    void Start()
    {
        ShowPanel(currentIndex);
    }

    public void ShowPanel(int index)
    {
        // Matikan semua sub-panel
        for (int i = 0; i < subPanels.Length; i++)
            subPanels[i].SetActive(false);

        // Aktifkan panel sesuai index
        if (index >= 0 && index < subPanels.Length)
        {
            subPanels[index].SetActive(true);
            currentIndex = index;
        }

        UpdateButtons();
    }

    public void NextPanel()
    {
        int nextIndex = currentIndex + 1;
        Debug.Log("Next Index :" + nextIndex);
        if (nextIndex < subPanels.Length)
        {
            ShowPanel(nextIndex);
        }
    }

    public void PreviousPanel()
    {
        int prevIndex = currentIndex - 1;
        if (prevIndex >= 0)
        {
            ShowPanel(prevIndex);
        }
    }

    private void UpdateButtons()
    {
        // Jika di panel pertama → hide backButton
        if (backButton != null)
            backButton.SetActive(currentIndex > 0);

        // Jika di panel terakhir → hide nextIcon
        if (nextIcon != null)
            nextIcon.SetActive(currentIndex < subPanels.Length - 1);
    }
}
