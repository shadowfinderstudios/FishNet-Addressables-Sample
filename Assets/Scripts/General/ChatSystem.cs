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

    private void Update()
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
        if (text == "") return;

        if (_history.Count >= _maxLines) _history.RemoveAt(0);
        _history.Add("You: " + text);

        _chatText.text = string.Join("\n", _history.ToArray());
        _inputField.text = "";

        if (_history.Count > _linesToScrollAt)
            _scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}
