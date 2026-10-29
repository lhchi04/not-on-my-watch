using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform messageContainer;   // Scroll View > Viewport > Content
    [SerializeField] private ScrollRect scrollRect;

    [Header("Bubble Prefabs")]
    [SerializeField] private GameObject userBubblePrefab;
    [SerializeField] private GameObject aiBubblePrefab;

    [Header("AI Settings")]
    [SerializeField] private string apiKey;
    [SerializeField] private string model = "claude-haiku-4-5-20251001";

    private const string API_URL = "https://api.anthropic.com/v1/messages";

    private const string SYSTEM_PROMPT =
        "You are a first aid assistant embedded in a VR emergency training simulation. " +
        "Only answer questions about first aid, CPR, AED use, wound care, burns, " +
        "choking, fractures, bleeding control, and emergency medical response. " +
        "Keep answers concise and actionable — no more than 3 short sentences. " +
        "If a question is unrelated to first aid, respond only with: " +
        "\"I can only help with first aid questions.\"";

    private readonly List<MsgEntry> history = new();
    private bool busy;

    private void Start()
    {
        sendButton.onClick.AddListener(Send);
        inputField.onSubmit.AddListener(OnSubmit);
        inputField.onSelect.AddListener(OnInputFieldSelected);

        var xrKeyboard = GlobalNonNativeKeyboard.instance?.keyboard;
        if (xrKeyboard != null)
        {
            xrKeyboard.onTextUpdated.AddListener(OnKeyboardTextUpdated);
            xrKeyboard.onTextSubmitted.AddListener(OnKeyboardSubmitted);
        }
    }

    private void OnDestroy()
    {
        sendButton.onClick.RemoveListener(Send);
        inputField.onSubmit.RemoveListener(OnSubmit);
        inputField.onSelect.RemoveListener(OnInputFieldSelected);

        var xrKeyboard = GlobalNonNativeKeyboard.instance?.keyboard;
        if (xrKeyboard != null)
        {
            xrKeyboard.onTextUpdated.RemoveListener(OnKeyboardTextUpdated);
            xrKeyboard.onTextSubmitted.RemoveListener(OnKeyboardSubmitted);
        }
    }

    private void OnInputFieldSelected(string current)
    {
        if (GlobalNonNativeKeyboard.instance != null)
            GlobalNonNativeKeyboard.instance.ShowKeyboard(inputField);
    }

    private void OnKeyboardTextUpdated(KeyboardTextEventArgs args)
    {
        inputField.text = args.keyboardText;
    }

    private void OnKeyboardSubmitted(KeyboardTextEventArgs args)
    {
        inputField.text = args.keyboardText;
        Send();
    }

    private void OnSubmit(string _) => Send();

    public void Send()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text) || busy) return;

        inputField.text = string.Empty;
        inputField.ActivateInputField();

        SpawnBubble(text, isUser: true);
        history.Add(new MsgEntry("user", text));
        StartCoroutine(FetchResponse());
    }

    private IEnumerator FetchResponse()
    {
        busy = true;
        sendButton.interactable = false;

        GameObject typing = SpawnBubble("...", isUser: false);

        byte[] body = Encoding.UTF8.GetBytes(BuildJson());

        using var req = new UnityWebRequest(API_URL, "POST");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("x-api-key", apiKey);
        req.SetRequestHeader("anthropic-version", "2023-06-01");

        yield return req.SendWebRequest();

        Destroy(typing);

        string reply;
        if (req.result == UnityWebRequest.Result.Success)
        {
            var data = JsonUtility.FromJson<APIResponse>(req.downloadHandler.text);
            reply = (data != null && data.content != null && data.content.Length > 0)
                ? data.content[0].text.Trim()
                : "No response received.";
        }
        else
        {
            reply = "Connection error — please try again.";
            Debug.LogWarning($"[ChatManager] {req.error}\n{req.downloadHandler.text}");
        }

        history.Add(new MsgEntry("assistant", reply));
        SpawnBubble(reply, isUser: false);

        busy = false;
        sendButton.interactable = true;
    }

    private GameObject SpawnBubble(string text, bool isUser)
    {
        GameObject prefab = isUser ? userBubblePrefab : aiBubblePrefab;
        GameObject bubble = Instantiate(prefab, messageContainer);
        bubble.GetComponentInChildren<TMP_Text>().text = text;
        StartCoroutine(ScrollToBottom());
        return bubble;
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = Vector2.zero;
    }

    private string BuildJson()
    {
        var sb = new StringBuilder();
        sb.Append("{\"model\":\"").Append(model)
          .Append("\",\"max_tokens\":300")
          .Append(",\"system\":\"").Append(Escape(SYSTEM_PROMPT)).Append("\"")
          .Append(",\"messages\":[");

        for (int i = 0; i < history.Count; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append("{\"role\":\"").Append(history[i].role)
              .Append("\",\"content\":\"").Append(Escape(history[i].content)).Append("\"}");
        }

        sb.Append("]}");
        return sb.ToString();
    }

    private static string Escape(string s) =>
        s.Replace("\\", "\\\\")
         .Replace("\"", "\\\"")
         .Replace("\n", "\\n")
         .Replace("\r", "");

    // ── Serialisable inner types for JSON ────────────────────────────────────
    [Serializable] private class MsgEntry
    {
        public string role, content;
        public MsgEntry(string r, string c) { role = r; content = c; }
    }

    [Serializable] private class APIResponse  { public ContentBlock[] content; }
    [Serializable] private class ContentBlock { public string type; public string text; }
}
