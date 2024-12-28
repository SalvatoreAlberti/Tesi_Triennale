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
    public HuggingFaceAPI huggingFaceAPI;

    [Header("Scene References")]
    public GameObject wall;
    public GameObject board;    

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private void Start()
    {
        sendButton.onClick.AddListener(SendMessage);

         // Salva la posizione e la rotazione originali della lavagna
        originalPosition = board.transform.position;
        originalRotation = board.transform.rotation;

        AddMessage("Ciao, come posso aiutarti?\nPosso spostare la lavanga a destra o a sinistra. Posso Cambiare il colore della parete in rosso, blu, verde, rosa, giallo o in colore originale", false);
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
        scrollRect.verticalNormalizedPosition = 0f;
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
            {"originale", "original"},
            {"con il suo colore originale", "with the original color"},
            {"con il colore originale", "with the original color"},
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

        text = text.ToLower();
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
                        AddMessage("La lavagna si trova già a destra\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(posLavagna == "Sinistra"){
                        MoveBoard(parametro);
                        AddMessage("Lavagna spostata correttamente a destra\n Posso aiutarti in quale altro modo?", false);
                        posLavagna = "Destra";
                        yield break;
                    }
                    
                }
                else if (parametro == "Left")
                {
                    if(posLavagna == "Sinistra"){
                        AddMessage("La lavagna si trova già a sinistra\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(posLavagna == "Destra"){
                        MoveBoard(parametro);
                        AddMessage("Lavagna spostata correttamente a sinistra\n Posso aiutarti in quale altro modo?", false);
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
                        AddMessage("Il colore della parete è già rosso\n Posso aiutarti in quale altro modo?", false);
                        yield break;    
                    }else if(colParete != "Rosso"){
                        ChangeWallColor(parametro);
                        AddMessage("Colore della parete modificato correttamente in rosso\n Posso aiutarti in quale altro modo?", false);
                        colParete = "Rosso";
                        yield break;
                    }
                }else if (parametro == "Blue")
                {
                    if(colParete == "Blu"){
                        AddMessage("Il colore della parete è già blu\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Blue"){
                        ChangeWallColor(parametro);
                        AddMessage("Colore della parete modificato correttamente in blu\n Posso aiutarti in quale altro modo?", false);
                        colParete = "Blu";
                        yield break;
                    }

                }else if (parametro == "Green")
                {
                    if(colParete == "Green"){
                        AddMessage("Il colore della parete è già verde\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Green"){
                        ChangeWallColor(parametro);
                        AddMessage("Colore della parete modificato correttamente in verde\n Posso aiutarti in quale altro modo?", false);
                        colParete = "Verde";
                        yield break;
                    }

                }else if (parametro == "Pink")
                {
                    if(colParete == "Rosa"){
                        AddMessage("Il colore della parete è già rosa\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Rosa"){
                        ChangeWallColor(parametro);
                        AddMessage("Colore della parete modificato correttamente in rosa\n Posso aiutarti in quale altro modo?", false);
                        colParete = "Rosa";
                        yield break;
                    }

                }else if (parametro == "Yellow")
                {
                    if(colParete == "Giallo"){
                        AddMessage("Il colore della parete è già giallo\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Giallo"){
                        ChangeWallColor(parametro);
                        AddMessage("Colore della parete modificato correttamente in giallo\n Posso aiutarti in quale altro modo?", false);
                        colParete = "Giallo";
                        yield break;
                    }

                }else if (parametro == "Original")
                {
                    if(colParete == "Originale"){
                        AddMessage("Il colore della parete è già con il suo colore originale\n Posso aiutarti in quale altro modo?", false);
                        yield break;
                    }else if(colParete != "Originale"){
                        ChangeWallColor(parametro);
                        AddMessage("Colore della parete modificato correttamente con il suo colore originale\n Posso aiutarti in quale altro modo?", false);
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

    private void ChangeWallColor(string color)
    {
        Color newColor;
        switch (color)
        {
            case "Red":
                newColor = Color.red;
                break;
            case "Blue":
                newColor = Color.blue;
                break;
            case "Green":
                newColor = Color.green;
                break;
            case "Pink":
                newColor = new Color(1f, 0.75f, 0.8f); 
                break;
            case "Yellow":
                newColor = Color.yellow;
                break;
            case "Original":
            default:
                newColor = Color.white; 
                break;
        }

        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer != null)
        {
            wallRenderer.material.color = newColor;
        }
        else
        {
            Debug.LogWarning("Renderer della parete non trovato!");
        }
    }

    private void MoveBoard(string direction)
    {
        Vector3 newPosition;
        Quaternion newRotation;

        switch (direction)
        {
            case "Right":
                newPosition = new Vector3(6.3f, board.transform.position.y, 9.0f);
                newRotation = Quaternion.Euler(0, 20, 0); 
                break;

            case "Left":
                newPosition = originalPosition;
                newRotation = originalRotation;
                break;

            default:
                Debug.LogWarning("Direzione non valida per lo spostamento.");
                return;
        }

        board.transform.position = newPosition;
        board.transform.rotation = newRotation;

        Debug.Log($"Lavagna spostata a {direction}. Posizione: {newPosition}, Rotazione: {newRotation.eulerAngles}");
    }

}