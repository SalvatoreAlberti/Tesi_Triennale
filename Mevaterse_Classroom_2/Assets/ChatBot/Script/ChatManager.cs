using System.Collections;
using System.Collections.Generic; // Aggiunto per Dictionary
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatContent;
    public TMP_InputField inputField;
    public Button sendButton;
    public GameObject messageBubbleUtentePrefab;
    public GameObject messageBubbleChatPrefab;

    [Header("API References")]
    public HuggingFaceAPI huggingFaceAPI; // Riferimento all'oggetto API

    private void Start()
    {
        sendButton.onClick.AddListener(SendMessage);
        Debug.Log("Pulsante 'Invia' configurato correttamente.");
    }

    public void SendMessage()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage))
        {
            Debug.LogWarning("Nessun messaggio inserito.");
            return;
        }

        // Aggiunge il messaggio dell'utente
        AddMessage(userMessage, true);

        // Processa il messaggio normalmente per comandi validi
        StartCoroutine(GetChatbotResponse(userMessage));
        ClearInputField();
    }

    private void AddMessage(string message, bool isUser)
    {
        // Scegli il prefab in base a chi è il mittente
        GameObject prefab = isUser ? messageBubbleUtentePrefab : messageBubbleChatPrefab;
        GameObject messageBubble = Instantiate(prefab, chatContent.transform);

        // Posiziona e configura il messaggio
        messageBubble.transform.localPosition = Vector3.zero;
        messageBubble.transform.localScale = Vector3.one;

        TMP_Text messageText = messageBubble.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = message;
            Debug.Log("Testo aggiornato nella bolla.");
        }

        // Aggiorna il layout per mostrare correttamente il messaggio
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        ScrollRect scrollRect = chatContent.GetComponentInParent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0f; // Scorri fino in fondo
    }

    private string TranslateToEnglish(string text)
    {
        Dictionary<string, string> translationDictionary = new Dictionary<string, string>()
        {
            {"sposta la lavagna", "move the board" },
            {"cambia il colore della parete", "change the color of the wall"},
            {"cambia colore", "change color"},
            {"muovi", "move"},
            {"sposta", "move"},
            {"sulla destra", "on the right"},
            {"a destra", "on the right"},
            {"sulla sinistra", "on the left"},
            {"a sinistra", "on the left"},
            {"rosso", "red"},
            {"blu", "blue"},
            {"verde", "green"},
            {"giallo", "yellow"},
            {"rosa", "pink"},
            {"colore originale", "original color"},
            {"colore", "color"},
            {"lavagna", "board"},
            {"parete", "wall"}
        };

        text = text.ToLower(); // Converti a minuscolo per uniformità
        bool recognized = false;

        foreach (var entry in translationDictionary)
        {
            if (text.Contains(entry.Key))
            {
                text = text.Replace(entry.Key, entry.Value);
                recognized = true;
            }
        }

        if (!recognized)
        {
            Debug.LogWarning("Comando non riconosciuto.");
            return "invalid action";
        }

        return text;
    }

    private IEnumerator GetChatbotResponse(string userMessage)
    {
        string azione = string.Empty;
        string oggetto = string.Empty;
        string parametro = string.Empty;

        // Traduci il messaggio in inglese
        string translatedMessage = TranslateToEnglish(userMessage);
        Debug.Log($"Messaggio tradotto: {translatedMessage}");

        yield return StartCoroutine(GetAzione(translatedMessage, (response) => azione = response));
        if (azione == "Invalid Action")
        {
            AddMessage("Non ho capito cosa devo fare, puoi ripetere?", false);
            yield break;
        }

        if (azione == "Move")
        {
            yield return StartCoroutine(GetOggetto(translatedMessage, (response) => oggetto = response));
            if (oggetto == "Wall")
            {
                AddMessage("Al momento è possibile spostare solo la lavagna.", false);
                yield break;
            }

            if (oggetto == "Board")
            {
                yield return StartCoroutine(GetParametroPosizione(translatedMessage, (response) => parametro = response));
                if (parametro == "Invalid Parameter")
                {
                    AddMessage("Puoi ripetere dove vuoi spostare la lavagna? (destra o sinistra)", false);
                    yield break;
                }

                AddMessage($"Lavagna spostata correttamente a {parametro}.", false);
            }
        }
        else if (azione == "Change color")
        {
            yield return StartCoroutine(GetOggetto(translatedMessage, (response) => oggetto = response));
            if (oggetto == "Board")
            {
                AddMessage("Al momento è possibile cambiare il colore solo della parete.", false);
                yield break;
            }

            if (oggetto == "Wall")
            {
                yield return StartCoroutine(GetParametroColore(translatedMessage, (response) => parametro = response));
                if (parametro == "Invalid Parameter")
                {
                    AddMessage("Puoi ripetere il colore desiderato per la parete? (rosso, blu, verde, rosa, giallo, originale)", false);
                    yield break;
                }

                AddMessage($"Colore della parete modificato correttamente a {parametro}.", false);
            }
        }
    }

    private IEnumerator GetAzione(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of three actions: 'Move' or 'Change color' or 'Invalid Action'."+
        $"- Use 'Move' only for repositioning objects like 'Move the board'."+
        $"- Use 'Change color' only for altering colors like 'Change the wall color'."+
        $"- If the input contains an action that is neither 'Move' nor 'Change color', respond with 'Invalid action'."+
        "- If you are unsure of the appropriate action, default to 'Invalid action'."+
        $"User Input: '{userMessage}'";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    private IEnumerator GetOggetto(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of three categories: 'Board', 'Wall', or 'Invalid Object'."+
        $"- Use 'Board' when the input specifically refers to a 'board', such as in the example 'Move the board'. 'Move who? Response: the board'."+
        $"- Use 'Wall' when the input specifically refers to a 'wall', such as in the example 'Change the color of the wall' or 'Move the wall'. 'Move who? Response: the wall'"+
        $" User Input: '{userMessage}'";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    private IEnumerator GetParametroColore(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of seven categories: 'Red', 'Green', 'Blue', 'Pink', 'Yellow' , 'Original' or 'Invalid Parameter'."+
        $"- Use 'Red' if the input explicitly mentions 'red', such as in these examples:\n  - 'Change the color of the wall in red'.\n  -'How you change color? Paramter: in red'."+
        $"- Use 'Green' if the input explicitly mentions 'green', such as in these examples:\n  - 'Change the color of the wall in green'.\n  -'How you change color? Parameter: in green'."+
        $"- Use 'Blue' if the input explicitly mentions 'blue', such as in these examples:\n  - 'Change the color of the wall in blue'.\n  -'How you change color? Parameter: in blue'."+
        $"- Use 'Pink' if the input explicitly mentions 'pink', such as in these examples:\n  - 'Change the color of the wall in pink'.\n  -'How you change color? Parameter: pink'."+
        $"- Use 'Yellow' if the input explicitly mentions 'yellow', such as in these examples:\n  - 'Change the color of the wall in yellow'.\n  -'How you change color? Parameter: in yellow'."+
        $"- Use 'Original' if the input explicitly mentions 'original', such as in these examples:\n  - 'Change the color of the wall in the original color'.\n  -'How you change color? Parameter: in 'Original'color'."+
        $"- Use 'Invalid Parameter' if the input contains neither 'red' nor 'blue' nor ' green' nor 'random', such as in these examples:\n  - 'Change the color of the wall in black'. Parameter: 'Invalid Parameter'.\n  - 'Change the color of the wall in fnsdi'. Parameter: 'Invalid Parameter'."+
        $"- If you are unsure about the classification, always default to 'Invalid Parameter'."+
        $" User Input: '{userMessage}'";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    private IEnumerator GetParametroPosizione(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of three categories: 'Left', 'Right', or 'Invalid Parameter'."+
        $"- Use 'Left' if the input explicitly mentions 'left', such as in these examples:"+
        $"- 'Move the board on the left'.\n  - 'Move where? Response: on the left'."+
        $"- Use 'Right' if the input explicitly mentions 'right', such as in these examples:"+
        $"- 'Move the board on the right'.\n  - 'Move where? Response: on the right'."+
        $"- Use 'Invalid Parameter' if the input contains neither 'left' nor 'right', such as in these examples:"+
        $"- 'Move the board on the center'. Response: 'Invalid Parameter'.\n  - 'Move the board on the ijdncb'. Response: 'Invalid Parameter'."+
        $"- If you are unsure about the classification, always default to 'Invalid Parameter'."+
        $" User Input: '{userMessage}'";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    private void ClearInputField()
    {
        inputField.text = string.Empty;
    }
}
