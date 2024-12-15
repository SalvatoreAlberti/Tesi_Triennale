using System.Collections;
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

    private void SendMessage()
    {
        string userMessage = inputField.text.Trim();

        if (!string.IsNullOrEmpty(userMessage))
        {
            Debug.Log($"Messaggio inviato dall'utente: {userMessage}");

            AddMessage(userMessage, true);

            // Invia il messaggio all'API
            StartCoroutine(GetChatbotResponse(userMessage));

            inputField.text = string.Empty;
            Debug.Log("Campo di input ripulito.");
        }
        else
        {
            Debug.LogWarning("Nessun messaggio inserito.");
        }
    }

    private void AddMessage(string message, bool isUser)
    {
        GameObject prefab = isUser ? messageBubbleUtentePrefab : messageBubbleChatPrefab;

        GameObject messageBubble = Instantiate(prefab, chatContent.transform);

        // Assicura che sia posizionato correttamente
        messageBubble.transform.localPosition = Vector3.zero;
        messageBubble.transform.localScale = Vector3.one;

        TMP_Text messageText = messageBubble.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = message;
            Debug.Log("Testo aggiornato nella bolla.");
        }

        // Aggiorna i layout per assicurare che la bolla si ridimensioni correttamente
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();

        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        // Imposta la posizione della ScrollView per mostrare l'ultimo messaggio
        ScrollRect scrollRect = chatContent.GetComponentInParent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0f; // Scorri fino in fondo
    }

    private IEnumerator GetChatbotResponse(string userMessage)
    {
        // Crea il prompt
        string prompt =  $"L'ambiente virtuale contiene: una classe con una lavagna posizionata sulla sinistra ed una parete frontale di colore bianco. " +
                    $"Azioni disponibili: " +
                    $"1. Spostare la lavagna in direzioni specifiche: 'destra', 'sinistra', 'in fondo', 'davanti', 'al centro'. " +
                    $"2. Cambiare il colore delle pareti con un colore casuale o un colore specifico indicato dall'utente. " +
                    $"2. Ripristinare il colore delle pareti con il colore originale o la posizione della lavagna nella posizione originale " +
                    $"Formato di risposta: {{ 'azione': 'nome_azione', 'oggetto': 'nome_oggetto', 'parametro': 'valore' }}. " +
                    $"Input: 'Sposta la lavagna a destra e davanti alla classe.' Output: {{'azione': 'sposta', 'oggetto': 'lavagna', 'parametro': {{'direzione': ['destra', 'davanti']}}}}. " +
                    $"Input: 'Sposta la lavagna al centro e in fondo alla classe.' Output: {{'azione': 'sposta', 'oggetto': 'lavagna', 'parametro': {{'direzione': ['al centro', 'in fondo']}}}}. " +
                    $"Input: 'Ripristina il colore originale delle pareti' Output: {{'azione': 'ripristina', 'oggetto': 'pareti', 'parametro': {{'colore': 'bianco'}}}}. " +
                    $"L'utente dice: '{userMessage}'. Rispondi con un JSON che descriva l'azione da eseguire.";

        // Invia il prompt all'API
        yield return StartCoroutine(huggingFaceAPI.SendRequest(prompt, (response) =>
        {
            if (!string.IsNullOrEmpty(response))
            {
                Debug.Log($"Risposta dal modello: {response}");
                AddMessage(response, false); // Aggiungi il messaggio del chatbot
            }
            else
            {
                Debug.LogError("La risposta del modello è vuota.");
                AddMessage("Errore: non ho capito la tua richiesta.", false);
            }
        }));
    }
}