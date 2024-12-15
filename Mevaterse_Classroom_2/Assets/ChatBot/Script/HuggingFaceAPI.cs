using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

public class HuggingFaceAPI : MonoBehaviour
{
    private string apiUrl = "https://api-inference.huggingface.co/models/gpt2";
    private string apiKey = "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"; 

    public IEnumerator SendRequest(string prompt, System.Action<string> callback)
    {
        // Prepara il payload (input per il modello)
        var payload = new
        {
            inputs = prompt
        };

        // Serializza il payload in JSON
        string jsonPayload = JsonConvert.SerializeObject(payload);

        // Stampa il payload per il controllo
        Debug.Log("Payload inviato: " + jsonPayload);

        // Configura la richiesta HTTP
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Invia la richiesta e attendi la risposta
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            Debug.Log("Risposta ricevuta: " + response);
            callback(response); // Restituisce la risposta al callback
        }
        else
        {
            Debug.LogError("Errore nella richiesta: " + request.error);
            Debug.LogError("Dettagli della risposta: " + request.downloadHandler.text);
        }
    }
}

