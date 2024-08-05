using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class uiMessage : MonoBehaviour
{
    public static uiMessage instance { get; private set; }
    TextMeshProUGUI textBox;
    List<string> messages = new();
    StringBuilder displayText;
    RectTransform rectTransform;
    float resetTimeCurrent;
    float 
        targetYPos,
        startYPos;
    [SerializeField] bool 
        debugMessageSenders;
    [SerializeField] float 
        yPerMsg = 42.5f,
        animSpeed = 5f, 
        resetTime = 3f;
    [SerializeField] int
        maxLines = 5;
    void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        textBox = GetComponent<TextMeshProUGUI>();
        //startYPos = textBox.margin.y - yPerMsg;
    }
    void Update()
    {
        displayText = new StringBuilder();
        // gathers all valid messages and displays them as one block of text
        int validMessageCount = 0;
        for (int i = messages.Count > maxLines ? messages.Count - maxLines : 0; i < messages.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(messages[i])) { continue; }
            displayText.Append(messages[i]).Append("\n");
            validMessageCount++;
        }

        textBox.text = displayText.ToString();

        // depending on the current number of messages,
        // the text box lerps up to make it look like the message is sliding up onto the screen
        targetYPos = validMessageCount * yPerMsg;
        textBox.margin = new(0, Mathf.Lerp(textBox.margin.y, -targetYPos, animSpeed * Time.unscaledDeltaTime), 0, 0);

        // every time a new message is added this timer is reset, if the timer reaches 0 then all messages are cleared
        if (resetTimeCurrent > 0) { resetTimeCurrent -= Time.unscaledDeltaTime; }
        else 
        { 
            resetTimeCurrent = 0;
            messages.Clear();
        }
    }
    /// <summary>
    /// Displays a new temporary message on the screen, the message will disappear if no messages have been added for 2 seconds
    /// </summary>
    /// <param name="text"></param>
    public void New(string text, string sender = "Undefined Sender")
    {
        messages.Add(text);
        if (resetTime > resetTimeCurrent) { resetTimeCurrent = resetTime; }
        if (debugMessageSenders) { Debug.Log(sender + ": " + text); }
    }
    public void SetTimer(float seconds) => resetTimeCurrent = seconds;
    public void ResetTimer()
    {
        resetTimeCurrent = resetTime;
    }
}