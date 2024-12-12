using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatContent; // Il contenitore della chat (Vertical Layout Group)
    public TMP_InputField inputField; // L'InputField per inserire messaggi
    public Button sendButton; // Il bottone per inviare il messaggio
    public GameObject messageBubbleUtentePrefab; // Prefab per i messaggi dell'utente
    public GameObject messageBubbleChatPrefab; // Prefab per i messaggi del chatbot

    private void Start()
    {
        // Aggiunge l'evento al pulsante "Invia"
        sendButton.onClick.AddListener(SendMessage);
        Debug.Log("Pulsante 'Invia' configurato correttamente.");
    }

    private void SendMessage()
    {
        string userMessage = inputField.text.Trim();

        if (!string.IsNullOrEmpty(userMessage))
        {
            Debug.Log($"Messaggio inviato dall'utente: {userMessage}");

            // Aggiungi il messaggio dell'utente nella chat
            AddMessage(userMessage, true);

            // Simula una risposta del chatbot
            string chatbotResponse = GetChatbotResponse(userMessage);
            Debug.Log($"Risposta del chatbot: {chatbotResponse}");

            // Aggiungi la risposta del chatbot nella chat
            AddMessage(chatbotResponse, false);

            // Pulisce il campo di input
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
    // Scegli il prefab corretto (utente o chatbot)
    GameObject prefab = isUser ? messageBubbleUtentePrefab : messageBubbleChatPrefab;

    // Crea un'istanza della bolla
    GameObject messageBubble = Instantiate(prefab, chatContent.transform);

    // Assicura che sia posizionato correttamente
    messageBubble.transform.localPosition = Vector3.zero;
    messageBubble.transform.localScale = Vector3.one;

    // Trova il componente TMP_Text all'interno del prefab e imposta il testo
    TMP_Text messageText = messageBubble.GetComponentInChildren<TMP_Text>();
    if (messageText != null)
    {
        messageText.text = message;
        Debug.Log("Testo aggiornato nella bolla.");
    }

    // Forza l'aggiornamento del layout
    Canvas.ForceUpdateCanvases();
    LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent.GetComponent<RectTransform>());

    // Aggiorna la posizione della ScrollView
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
        // Qui puoi simulare una risposta o integrare il chatbot vero e proprio
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



