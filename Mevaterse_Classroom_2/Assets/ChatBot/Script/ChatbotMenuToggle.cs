using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatbotMenuToggle : MonoBehaviour
{
    public GameObject chatbotPanel;

    public void ToggleChatbotMenu()
    {
        bool isActive = chatbotPanel.activeSelf;
        chatbotPanel.SetActive(!isActive);
    }
    void Update()
        {
            if(Input.GetKeyUp(KeyCode.L)) {
                bool isActive = chatbotPanel.activeSelf;
                chatbotPanel.SetActive(!isActive);
            }
        }

}
