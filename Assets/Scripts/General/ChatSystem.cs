using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem : MonoBehaviour
{
    [SerializeField] TMP_Text _chatText;
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] int _linesToScrollAt = 19;
    [SerializeField] int _maxLines = 33;

    List<string> _history = new();

    void Awake()
    {
        if (!String.IsNullOrEmpty(_chatText.text))
        {
            _history = _chatText.text.Split('\n').ToList();
            if (_history.Count > _linesToScrollAt)
                _scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }

    void OnEnable()
    {
        InstanceFinder.ClientManager.RegisterBroadcast<ChatBroadcast>(OnClientBroadcast);
        InstanceFinder.ServerManager.RegisterBroadcast<ChatBroadcast>(OnServerBroadcast, false);
    }

    void OnDisable()
    {
        InstanceFinder.ClientManager?.UnregisterBroadcast<ChatBroadcast>(OnClientBroadcast);
        InstanceFinder.ServerManager?.UnregisterBroadcast<ChatBroadcast>(OnServerBroadcast);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _inputField.ActivateInputField();
            _inputField.Select();
            _inputField.text = "";
        }
    }

    public void OnEndEdit(string text)
    {
        var msg = new ChatBroadcast() { Message = text };
        InstanceFinder.ClientManager.Broadcast(msg);
    }

    bool UpdateChat(string username, string message)
    {
        if (string.IsNullOrEmpty(username)) return false;
        if (string.IsNullOrEmpty(message)) return false;

        if (_history.Count >= _maxLines) _history.RemoveAt(0);
        _history.Add(username + ": " + message);

        _chatText.text = string.Join("\n", _history.ToArray());
        _inputField.text = "";

        if (_history.Count > _linesToScrollAt)
            _scrollRect.normalizedPosition = new Vector2(0, 0);
        return true;
    }

    private void OnServerBroadcast(NetworkConnection conn, ChatBroadcast msg, Channel channel)
    {
        var nob = conn.FirstObject;
        if (nob == null) return;
        msg.Username = GetClientUsername(conn);
        InstanceFinder.ServerManager.Broadcast(nob, msg, true);
    }

    private void OnClientBroadcast(ChatBroadcast msg, Channel channel)
    {
        UpdateChat(msg.Username, msg.Message);
    }

    private string GetClientUsername(NetworkConnection conn)
    {
        var generator = new ElvenNameGenerator.ElvenNameGenerator(conn.ClientId);
        return generator.GenerateName();
    }
}
