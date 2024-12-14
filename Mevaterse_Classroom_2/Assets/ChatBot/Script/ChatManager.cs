using System.Collections;
using System.Collections.Generic;
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

            // Simula una risposta del chatbot
            string chatbotResponse = GetChatbotResponse(userMessage);
            Debug.Log($"Risposta del chatbot: {chatbotResponse}");

            AddMessage(chatbotResponse, false);

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



    private string GetChatbotResponse(string userMessage)
    {
        // Il sistema attuale (Provissiorio) simula una risposta
        if (userMessage.ToLower().Contains("ciao"))
        {
            return "Ciao! Come posso aiutarti?";
        }
        else if (userMessage.ToLower().Contains("lavagna"))
        {
            return "Vuoi spostare la lavagna? Specifica dove!";
        }
        else if (userMessage.ToLower().Contains("colore"))
        {
            return "Che colore preferisci per le pareti?";
        }
        else
        {
            return "Non ho capito, puoi riformulare?";
        }
    }
}