using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlInfoHanlder : MonoBehaviour
{
    private Image background;
    private TMP_Text text;
    public TMP_InputField ChatBotinputField;

    private void Start()
    {
        background = gameObject.GetComponent<Image>();
        text = gameObject.GetComponentInChildren<TMP_Text>();
        ChatBotinputField = GameObject.Find("InputField (Utente)").GetComponent<TMPro.TMP_InputField>();


        background.enabled = false;
        text.enabled = false;
    }
    void Update()
    {
        if(!ChatBotinputField.isFocused && Input.GetKeyUp(KeyCode.Z)) {
            background.enabled = !background.enabled;
            text.enabled = !text.enabled;
        }
    }
}
