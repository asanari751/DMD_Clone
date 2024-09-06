using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class LastKeyPressVisualizer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private string prefix = "Recent key: ";
    [SerializeField] private float displayDuration = 2f;

    private float lastKeyPressTime;
    private string lastKeyPressed = "";

    private Dictionary<Key, string> keyNames = new Dictionary<Key, string>();

    private void Awake()
    {
        if (displayText == null)
        {
            Debug.LogError("Display Text (TMP) is not assigned to LastKeyPressVisualizer!");
        }

        InitializeKeyNames();
    }

    private void OnEnable()
    {
        Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char ch)
    {
        lastKeyPressed = ch.ToString();
        lastKeyPressTime = Time.time;
        UpdateDisplay();
    }

    private void Update()
    {
        foreach (Key key in keyNames.Keys)
        {
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                lastKeyPressed = GetKeyName(key);
                lastKeyPressTime = Time.time;
                UpdateDisplay();
                break;
            }
        }

        if (Time.time - lastKeyPressTime > displayDuration)
        {
            displayText.text = "";
        }
    }

    private void UpdateDisplay()
    {
        displayText.text = prefix + lastKeyPressed;
    }

    private string GetKeyName(Key key)
    {
        if (keyNames.TryGetValue(key, out string keyName))
        {
            return keyName;
        }
        return key.ToString();
    }

    private void InitializeKeyNames()
    {
        keyNames[Key.Enter] = "Enter";
        keyNames[Key.NumpadEnter] = "Enter";
        keyNames[Key.Escape] = "Esc";
        keyNames[Key.LeftCtrl] = "Ctrl";
        keyNames[Key.RightCtrl] = "Ctrl";
        keyNames[Key.LeftShift] = "Shift";
        keyNames[Key.RightShift] = "Shift";
        keyNames[Key.LeftAlt] = "Alt";
        keyNames[Key.RightAlt] = "Alt";
        keyNames[Key.LeftArrow] = "←";
        keyNames[Key.RightArrow] = "→";
        keyNames[Key.UpArrow] = "↑";
        keyNames[Key.DownArrow] = "↓";
        keyNames[Key.Space] = "Space";
        keyNames[Key.Backspace] = "Backspace";
        keyNames[Key.Delete] = "Delete";
        keyNames[Key.Insert] = "Insert";
        keyNames[Key.Home] = "Home";
        keyNames[Key.End] = "End";
        keyNames[Key.PageUp] = "Page Up";
        keyNames[Key.PageDown] = "Page Down";
        keyNames[Key.CapsLock] = "Caps Lock";
        keyNames[Key.W] = "W";
        keyNames[Key.A] = "A";
        keyNames[Key.S] = "S";
        keyNames[Key.D] = "D";
        keyNames[Key.Q] = "Q";
        keyNames[Key.E] = "E";
        keyNames[Key.R] = "R";
        keyNames[Key.F] = "F";
        keyNames[Key.G] = "G";
        keyNames[Key.H] = "H";
        // 필요한 다른 키 이름들을 여기에 추가할 수 있습니다.
    }
}