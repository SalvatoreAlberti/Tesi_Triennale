using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



public class HuggingFaceAPI : MonoBehaviour
{
    private string apiUrl = "https://api-inference.huggingface.co/models/google/flan-t5-base";
    private string apiKey = "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";

    public IEnumerator SendRequest(string prompt, System.Action<string> callback)
    {
        var payload = new
        {
            inputs = prompt
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        Debug.Log("Payload inviato: " + jsonPayload);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        bool retry = true;
        int maxRetries = 5;
        int retryCount = 0;

        while (retry && retryCount < maxRetries)
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log("Risposta ricevuta: " + response);

                // Controlla se il modello è ancora in fase di caricamento
                if (response.Contains("\"error\":\"Model") && response.Contains("is currently loading"))
                {
                    Debug.LogWarning("Il modello è in fase di caricamento. Riprovo...");
                    retryCount++;
                    yield return new WaitForSeconds(10); // Aspetta 10 secondi prima di riprovare
                }
                else
                {
                    // Parse della risposta JSON
                    try
                    {
                        JArray jsonResponse = JArray.Parse(response);
                        string generatedText = jsonResponse[0]["generated_text"]?.ToString();
                        Debug.Log("Testo generato: " + generatedText);
                        callback(generatedText);
                    }
                    catch (JsonException e)
                    {
                        Debug.LogError("Errore nel parsing della risposta JSON: " + e.Message);
                        callback(null);
                    }
                    retry = false; // Successo, esce dal ciclo
                }
            }
            else
            {
                Debug.LogError("Errore nella richiesta: " + request.error);
                Debug.LogError("Dettagli della risposta: " + request.downloadHandler.text);
                retry = false; // Esci se c'è un errore critico
            }
        }

        if (retryCount == maxRetries)
        {
            Debug.LogError("Superato il numero massimo di tentativi. Riprova più tardi.");
            callback(null);
        }
    }
}