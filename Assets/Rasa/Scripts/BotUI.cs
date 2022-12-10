﻿using CognitiveServicesTTS;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class contains the gameobjects and methods for interacting with the UI.
/// </summary>
public class BotUI : MonoBehaviour {
    public GameObject       contentDisplayObject;               // Text gameobject where all the conversation is shown
    public InputField       input;                              // InputField gameobject wher user types their message

    public GameObject       userBubble;                         // reference to user chat bubble prefab
    public GameObject       botBubble;                          // reference to bot chat bubble prefab

    private const int       messagePadding = 15;                // space between chat bubbles 
    private int             allMessagesHeight = messagePadding; // int to keep track of where next message should be rendered
    public bool             increaseContentObjectHeight;        // bool to check if content object height should be increased

    public SpeechManager speechManager;
    public Toggle useSDK;
    public Dropdown voicelist;

    public NetworkManager   networkManager;                     // reference to Network Manager script
    public ASP_Script ASP_manager;                              // reference to ASP_manager DLV-K agent

    private void Start()
    {

        List<string> voices = new List<string>();
        foreach (VoiceName voice in System.Enum.GetValues(typeof(VoiceName)))
        {
            voices.Add(voice.ToString());
        }
        voicelist.AddOptions(voices);
        voicelist.value = (int)VoiceName.enUSJessaRUS;
    }

    /// <summary>
    /// This method is used to update the display panel with the user's and bot's messages.
    /// </summary>
    /// <param name="sender">The one who wrote this message</param>
    /// <param name="message">The message</param>
    public void UpdateDisplay (string sender, string message, string messageType) {
        // Create chat bubble and add components
        GameObject chatBubbleChild = CreateChatBubble(sender);
        if(sender == "bot")
        {
            //Message is a command request for agent
            if (message.Contains("COACH"))
            {
                if (message.Contains("RECOMMEND_ACTION"))
                {
                    Debug.Log("UpdateDisplay: " + "Recommend_action");
                    message = ASP_manager.GetNextAction();
                    SpeechPlayback(message);

                }
                else if (message.Contains("TELL_GOALSTATE"))
                {
                    Debug.Log("UpdateDisplay: " + "tell_goalstate");
                    message = ASP_manager.GetGoalStateMessage();
                    SpeechPlayback(message);
                }
                else if (message.Contains("SET_GOALSTATE"))
                {
                    Debug.Log("UpdateDisplay: " + "tell_goalstate");
                    ASP_manager.SetGoalStateToWorldState();
                    message = "OK, new goalstate set!";
                    SpeechPlayback(message);
                }
            }
            else
            {
                SpeechPlayback(message);
            }
        }
        AddChatComponent(chatBubbleChild, message, messageType);

        // Set chat bubble position
        StartCoroutine(SetChatBubblePosition(chatBubbleChild.transform.parent.GetComponent<RectTransform>(), sender));
    }

    /// <summary>
    /// Coroutine to set the position of the chat bubble inside the contentDisplayObject.
    /// </summary>
    /// <param name="chatBubblePos">RectTransform of chat bubble</param>
    /// <param name="sender">Sender who sent the message</param>
    private IEnumerator SetChatBubblePosition (RectTransform chatBubblePos, string sender) {
        // Wait for end of frame before calculating UI transform
        yield return new WaitForEndOfFrame();

        // get horizontal position based on sender
        int horizontalPos = 0;
        if (sender == "user") {
            horizontalPos = -50;
        } else if (sender == "bot") {
            horizontalPos = 50;
        }

        // set the vertical position of chat bubble
        allMessagesHeight += 15 + (int)chatBubblePos.sizeDelta.y;
        chatBubblePos.anchoredPosition3D = new Vector3(horizontalPos, -allMessagesHeight, 0);

        if (allMessagesHeight > 340) {
            // update contentDisplayObject hieght
            RectTransform contentRect = contentDisplayObject.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, allMessagesHeight + messagePadding);
            contentDisplayObject.transform.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0;
        }
    }

    /// <summary>
    /// Coroutine to update chat bubble positions based on their size.
    /// </summary>
    public IEnumerator RefreshChatBubblePosition () {
        // Wait for end of frame before calculating UI transform
        yield return new WaitForEndOfFrame();

        // refresh position of all gameobjects based on size
        int localAllMessagesHeight = messagePadding;
        foreach (RectTransform chatBubbleRect in contentDisplayObject.GetComponent<RectTransform>()) {
            if (chatBubbleRect.sizeDelta.y < 35) {
                localAllMessagesHeight += 35 + messagePadding;
            } else {
                localAllMessagesHeight += (int)chatBubbleRect.sizeDelta.y + messagePadding;
            }
            chatBubbleRect.anchoredPosition3D =
                    new Vector3(chatBubbleRect.anchoredPosition3D.x, -localAllMessagesHeight, 0);
        }

        // Update global message Height variable
        allMessagesHeight = localAllMessagesHeight;
        if (allMessagesHeight > 340) {
            // update contentDisplayObject hieght
            RectTransform contentRect = contentDisplayObject.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, allMessagesHeight + messagePadding);
            contentDisplayObject.transform.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0;
        }
    }

    /// <summary>
    /// This method creates chat bubbles from prefabs and sets their positions.
    /// </summary>
    /// <param name="sender">The sender of message for which bubble is rendered</param>
    /// <returns>Reference to empty gameobject on which message components can be added</returns>
    private GameObject CreateChatBubble (string sender) {
        GameObject chat = null;
        if (sender == "user") {
            // Create user chat bubble from prefabs and set it's position
            chat = Instantiate(userBubble);
            chat.transform.SetParent(contentDisplayObject.transform, false);
        } else if (sender == "bot") {
            // Create bot chat bubble from prefabs and set it's position
            chat = Instantiate(botBubble);
            chat.transform.SetParent(contentDisplayObject.transform, false);
        }

        // Add content size fitter
        ContentSizeFitter chatSize = chat.AddComponent<ContentSizeFitter>();
        chatSize.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        chatSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Add vertical layout group
        VerticalLayoutGroup verticalLayout = chat.AddComponent<VerticalLayoutGroup>();
        if (sender == "user") {
            verticalLayout.padding = new RectOffset(10, 20, 5, 5);
        } else if (sender == "bot") {
            verticalLayout.padding = new RectOffset(20, 10, 5, 5);
        }
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;

        // Return empty gameobject on which chat components will be added
        return chat.transform.GetChild(0).gameObject;
    }

    /// <summary>
    /// This method adds message component to chat bubbles based on message type.
    /// </summary>
    /// <param name="chatBubbleObject">The empty gameobject under chat bubble</param>
    /// <param name="message">message to be shown</param>
    /// <param name="messageType">The type of message (text, image etc)</param>
    private void AddChatComponent (GameObject chatBubbleObject, string message, string messageType) {
        switch (messageType) {
            case "text":
                // Create and init Text component
                Text chatMessage = chatBubbleObject.AddComponent<Text>();
                // add font as it is none at times when creating text component from script
                chatMessage.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                chatMessage.fontSize = 18;
                chatMessage.alignment = TextAnchor.MiddleLeft;
                chatMessage.text = message;
                break;
            case "image":
                // Create and init Image component
                Image chatImage = chatBubbleObject.AddComponent<Image>();
                StartCoroutine(networkManager.SetImageTextureFromUrl(message, chatImage));
                break;
            case "attachment":
                break;
            case "buttons":
                break;
            case "elements":
                break;
            case "quick_replies":
                break;
        }
    }
    /// <summary>
    /// Speech synthesis can be called via REST API or Speech Service SDK plugin for Unity
    /// </summary>
    public async void SpeechPlayback(string msg)
    {
        if (speechManager.isReady)
        {
            //string msg = input.text;
            speechManager.voiceName = (VoiceName)voicelist.value;
            speechManager.VoicePitch = 0;
            if (useSDK.isOn)
            {
                // Required to insure non-blocking code in the main Unity UI thread.
                await Task.Run(() => speechManager.SpeakWithSDKPlugin(msg));
            }
            else
            {
                // This code is non-blocking by default, no need to run in background
                speechManager.SpeakWithRESTAPI(msg);
            }
        }
        else
        {
            Debug.Log("SpeechManager is not ready. Wait until authentication has completed.");
        }
    }
}
