using TMPro;
using UnityEngine;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] private TMP_Text informationMessage; 

    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>(); 
        string previousName = PlayerPrefs.GetString("LastPlayerName", "");
        if(previousName != "")
        {
            inputField.text = previousName;
        }
    }

    public bool TrySubmit()
    {
        string error = PlayerManager.SetPlayerName(inputField.text);

        if (error != null)
        {
            informationMessage.text = error;
            return false;
        }

        informationMessage.text = "";
        return true;
    }

    public void ClearMessage()
    {
        informationMessage.text = "";
    }
}