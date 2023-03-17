using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SystemMessage : MonoBehaviour
{
    [SerializeField] private GameObject systemMessage;
    [SerializeField] private TMP_Text systemMessageText;
    [SerializeField] private Button systemMessageButton;
    void Awake()
    {
        systemMessageButton.onClick.AddListener(CloseMessage);
        CloseMessage();
    }
    public void ShowMessage(string message)
    {
        systemMessage.SetActive(true);
        systemMessageText.text = message;
    }
    public void CloseMessage()
    {
        systemMessage.SetActive(false);
    }
}
