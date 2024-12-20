using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class ChatbotMenuToggle : MonoBehaviour
{
    public GameObject chatbotPanel;
    public TMP_InputField inputField; 
    public void ToggleChatbotMenu()
    {
        bool isActive = chatbotPanel.activeSelf;
        chatbotPanel.SetActive(!isActive);
    }

    void Update()
    {
        if (!inputField.isFocused && Input.GetKeyUp(KeyCode.L))
        {
            bool isActive = chatbotPanel.activeSelf;
            chatbotPanel.SetActive(!isActive);
        }
        
    }
}
