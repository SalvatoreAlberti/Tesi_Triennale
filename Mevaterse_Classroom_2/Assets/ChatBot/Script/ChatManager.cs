using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ChatManager : MonoBehaviour
{
    private string posLavagna = "Sinistra";
    private string colParete = "Originale";
    private bool isAwaitingInput = false;

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
        AddMessage("Ciao, come posso aiutarti?", false);
        AddMessage("Posso spostare la lavanga a destra o a sinistra. Posso Cambiare il colore della parete in rosso, blu, verde, rosa, giallo o in colore originale", false);
    }

public void SendMessage()
{
    string userMessage = inputField.text.Trim();
    if (string.IsNullOrEmpty(userMessage))
    {
        Debug.LogWarning("Nessun messaggio inserito.");
        return;
    }

    AddMessage(userMessage, true);

    if (isAwaitingInput)
    {
        isAwaitingInput = false;
        return;
    }

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
            {"sposta la lavagna", "Move the board" },
            {"cambia il colore della parete", "Change the color of the wall"},
            {"cambia colore", "Change color"},
            {"muovi", "Move"},
            {"sposta", "Move"},
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
            {"parete", "wall"},
            {"la parete", "the wall"},
            {"la", "the"},
            {"il", "the"},
            {"destra", "right"},
            {"sinistra", "left"},
            {"Destra", "right"},
            {"Sinistra", "left"}
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
        else if (azione == "Move")
        {
            yield return StartCoroutine(GetOggetto(translatedMessage, (response) => oggetto = response));
            if (oggetto == "Wall")
            {
                AddMessage("Al momento è possibile spostare solo la lavagna", false);
                yield break;
            }
            else if (oggetto == "Board")
            {
                yield return StartCoroutine(TrovaParametroPosizione(translatedMessage, (response) => parametro = response));
                
                if (parametro == "Right")
                {
                    if(posLavagna == "Destra"){
                        AddMessage("La lavagna si trova già a destra", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(posLavagna == "Sinistra"){
                        AddMessage("Lavagna spostata correttamente a destra", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        posLavagna = "Destra";
                        yield break;
                    }
                    
                }
                else if (parametro == "Left")
                {
                    if(posLavagna == "Sinistra"){
                        AddMessage("La lavagna si trova già a sinistra", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(posLavagna == "Destra"){
                        AddMessage("Lavagna spostata correttamente a sinistra", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        posLavagna = "Sinistra";
                        yield break;
                    }
                }
            }
        }else if (azione == "Change color"){
            
            yield return StartCoroutine(GetOggetto(translatedMessage, (response) => oggetto = response));
            if (oggetto == "Board")
            {
                AddMessage("Al momento è possibile cambiare il colore solo della parete", false);
                yield break;
            }else if (oggetto == "Wall"){
                
                yield return StartCoroutine(TrovaParametroColore(translatedMessage, (response) => parametro = response));
                Debug.Log($"parametro in uscita: {parametro}");

                if (parametro == "Red")
                {
                    if(colParete == "Rosso"){
                        AddMessage("Il colore della parete è già rosso", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;    
                    }else if(colParete != "Rosso"){
                        AddMessage("Colore della parete modificato correttamente in rosso", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        colParete = "Rosso";
                        yield break;
                    }
                }else if (parametro == "Blue")
                {
                    if(colParete == "Blu"){
                        AddMessage("Il colore della parete è già blu", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Blue"){
                        AddMessage("Colore della parete modificato correttamente in blu", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        colParete = "Blu";
                        yield break;
                    }

                }else if (parametro == "Green")
                {
                    if(colParete == "Green"){
                        AddMessage("Il colore della parete è già verde", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Green"){
                        AddMessage("Colore della parete modificato correttamente in verde", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        colParete = "Verde";
                        yield break;
                    }

                }else if (parametro == "Pink")
                {
                    if(colParete == "Rosa"){
                        AddMessage("Il colore della parete è già rosa", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Rosa"){
                        AddMessage("Colore della parete modificato correttamente in rosa", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        colParete = "Rosa";
                        yield break;
                    }

                }else if (parametro == "Yellow")
                {
                    if(colParete == "Giallo"){
                        AddMessage("Il colore della parete è già giallo", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Giallo"){
                        AddMessage("Colore della parete modificato correttamente in giallo", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        colParete = "Giallo";
                        yield break;
                    }

                }else if (parametro == "Original")
                {
                    if(colParete == "Originale"){
                        AddMessage("Il colore della parete è già con il suo colore originale", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Originale"){
                        AddMessage("Colore della parete modificato correttamente con il suo colore originale", false);
                        AddMessage("Posso aiutarti in quale altro modo?", false);
                        colParete = "Originale";
                        yield break;
                    }
                }
            }
        }
    }

    private IEnumerator GetAzione(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of three actions: 'Move' or 'Change color' or 'Invalid Action'.\n"+
        $"-Use 'Move' when the input specifically refers to 'move', such as in the example 'Move the board'. 'What to do? Response: Move'\n"+
        $"- Use 'Change color' only for altering colors like 'Change the wall color'.\n"+
        $"- If the input contains an action that is neither 'Move' nor 'Change color', respond with 'Invalid action'.\n"+
        $"- If you are unsure of the appropriate action, default to 'Invalid action'.\n\n"+
        $"User Input: '{userMessage}'";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    private IEnumerator GetOggetto(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of three categories: 'Board', 'Wall', or 'Invalid Object'.\n"+
        $"- Use 'Board' when the input specifically refers to a 'board', such as in the example 'Move the board'. 'Move who? Response: the board'.\n"+
        $"- Use 'Wall' when the input specifically refers to a 'wall', such as in the example 'Change the color of the wall'. Change the color who? Wall.\n"+
        $" 'Move the wall'. 'Move who? Response: Wall'.\n\n"+
        $" Input: '{userMessage}'.";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    private IEnumerator GetParametroPosizione(string userMessage, System.Action<string> callback)
{
    string prompt = "Classify the following user input into one of three categories: 'Right', 'Left', or 'Invalid Parameter'.\n\n" +
        "Rules:\n- Use 'Right' if and only if the input explicitly mentions the exact word 'right'. Examples:\n" +
        "  - 'Move the board to the right'.'Right'- 'Move the chair on the right'. 'Right'\n\n" +
        "- Use 'Left' if and only if the input explicitly mentions the exact word 'left'. Examples:\n" +
        "  - 'Move the board to the left'. 'Left'\n  - 'Move the chair to the left'.'Left'\n\n" +
        "- Use 'Invalid Parameter' if the input contains none of the valid options, is unclear or is incomplete . Examples:\n" +
        "'Move the board'.'Invalid Parameter'.\n'Move the board on the fnsdi'.'Invalid Parameter'.\n" +
        "'Move the board on the center'.'Invalid Parameter'\n- Always default to 'Invalid Parameter' if you are unsure about the classification.\n\n" +
        $"Input: '{userMessage}'";

    yield return StartCoroutine(huggingFaceAPI.SendRequestParametroPosizione(prompt, callback));
}


    
    private IEnumerator GetParametroColore(string userMessage, System.Action<string> callback)
    {
        string prompt = $"Classify the following user input into one of seven categories: 'Red', 'Green', 'Blue', 'Pink', 'Yellow', 'Original', or 'Invalid Parameter'.\n\n"+
        $"Rules:\n- Use 'Red' if the input explicitly mentions 'red'. Examples:\n  - 'Change the color of the wall in red'.\n  - 'How do you change the color? in red'.\n\n"+
        $"- Use 'Green' if the input explicitly mentions 'green'. Examples:\n  - 'Change the color of the wall in green'.\n  - 'How do you change the color? in green'.\n\n"+
        $"- Use 'Blue' if the input explicitly mentions 'blue'. Examples:\n  - 'Change the color of the wall in blue'.\n  - 'How do you change the color? in blue'.\n\n"+
        $"- Use 'Pink' if the input explicitly mentions 'pink'. Examples:\n  - 'Change the color of the wall in pink'.\n  - 'How do you change the color? in pink'.\n\n"+
        $"- Use 'Yellow' if the input explicitly mentions 'yellow'. Examples:\n  - 'Change the color of the wall in yellow'.\n  - 'How do you change the color? in yellow'.\n\n"+
        $"- Use 'Original' if the input explicitly mentions 'original'. Examples:\n  - 'Change the color of the wall in the original color'.\n  - 'How do you change the color? in the original color'.\n\n"+
        $"- Use 'Invalid Parameter' if the input contains none of the valid colors or is unclear. Examples:\n  - 'Change the color of the wall in black'. Invalid Parameter.\n  - 'Change the color of the wall in fnsdi'. Invalid Parameter.\n\n"+
        $"- Always default to 'Invalid Parameter' if you are unsure about the classification.\n\n"+
        $" User Input: '{userMessage}'";
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, callback));
    }

    IEnumerator TrovaParametroPosizione(string translatedMessage, System.Action<string> callback)
    {
        string parametroTemp = string.Empty;
        bool isValid = false;

        while (!isValid)
        {
            yield return StartCoroutine(GetParametroPosizione(translatedMessage, (response) => parametroTemp = response));

            if (parametroTemp == "Right" || parametroTemp == "Left")
            {
                isValid = true;
                callback(parametroTemp);
            }
            else
            {
                AddMessage("Dove vuoi spostare la lavagna? (Destra o Sinistra)", false);

                // Aspetta il nuovo input dell'utente
                isAwaitingInput = true;
                yield return new WaitUntil(() => !isAwaitingInput);

                string newMessage = inputField.text.Trim();
                translatedMessage = TranslateToEnglish(newMessage);
                
                ClearInputField();
            }

        }
    }

    IEnumerator TrovaParametroColore(string translatedMessage, System.Action<string> callback)
    {
        string parametroTemp = string.Empty;
        bool isValid = false;

        while (!isValid)
        {

            yield return StartCoroutine(GetParametroColore(translatedMessage, (response) => parametroTemp = response));

            if (parametroTemp == "Red" || parametroTemp == "Blue" || parametroTemp == "Green" || 
                parametroTemp == "Pink" || parametroTemp == "Yellow" || parametroTemp == "Original")
            {
                isValid = true;
                callback(parametroTemp);
            }
            else
            {
                AddMessage("Di che colore vuoi la parete?\n(Rosso, Blu, Verde, Rosa, Giallo, Originale)", false);

                isAwaitingInput = true;
                yield return new WaitUntil(() => !isAwaitingInput);

                string newMessage = inputField.text.Trim();
                translatedMessage = TranslateToEnglish(newMessage);

                ClearInputField();
            }
        }

    }



    private void ClearInputField()
    {
        inputField.text = string.Empty;
    }
}