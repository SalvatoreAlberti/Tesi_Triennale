using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using TMPro; // Usa TMPro per il componente TextMeshProUGUI

public class TestMessage : MonoBehaviour
{
    public TextMeshProUGUI messageText; // Riferimento al componente TextMeshProUGUI
    public RectTransform bubbleRectTransform; // Riferimento al RectTransform della bolla
    public float padding = 1f; // Padding ridotto attorno al testo
    public float maxWidth = 60f; // Larghezza massima della bolla (puoi impostare un limite)

    void Start()
    {
        // Imposta un messaggio iniziale
        messageText.text = "Questo è un messaggio di prova!";
        // Ridimensiona la bolla in base al contenuto
        ResizeBubble();
    }

    // Metodo per cambiare dinamicamente il testo e ridimensionare la bolla
    public void ChangeMessage(string newMessage)
    {
        messageText.text = newMessage;
        // Ridimensiona la bolla in base al nuovo testo
        ResizeBubble();
    }

    // Funzione che ridimensiona la bolla in base al testo
    private void ResizeBubble()
    {
        // Ottieni le dimensioni preferite del testo
        float preferredWidth = messageText.preferredWidth;
        float preferredHeight = messageText.preferredHeight;

        // Applica il padding al testo (modificato a 10 unità)
        float newWidth = preferredWidth + padding;
        float newHeight = preferredHeight + padding;

        // Limita la larghezza massima della bolla
        if (newWidth > maxWidth)
        {
            newWidth = maxWidth;
        }

        // Imposta la larghezza e l'altezza della bolla
        bubbleRectTransform.sizeDelta = new Vector2(newWidth, newHeight);
    }
}
