using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameSettings : MonoBehaviour
{
    [System.Serializable]
    public class Resolution
    {
        public string label;
        public int width;
        public int height;
        public int refreshRate;

        public override string ToString()
        {
            return $"{width}x{height}#{refreshRate}";
        }
    }

    [System.Serializable]
    public class KeyBindingProfile
    {
        public string profileName;
        public InputActionReference movement;
        public InputActionReference interact;
        public InputActionReference autoAttack;
        public InputActionReference dash;
        public InputActionReference showInfo;
    }

    [Header("Settings Panels")]
    [SerializeField] private GameObject videoSettingsPanel;
    [SerializeField] private GameObject soundSettingsPanel;
    [SerializeField] private GameObject keyBindingSettingsPanel;

    [SerializeField] private Button videoSettingsButton;
    [SerializeField] private Button soundSettingsButton;
    [SerializeField] private Button keyBindingSettingsButton;

    [Header("Resolution")]
    [SerializeField] private TMP_Text resolutionText;
    [SerializeField] private Slider resolutionSlider;
    [SerializeField] private Resolution[] availableResolutions;

    [Header("Fullscreen")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Image fullscreenFill;

    [Header("Frame Rate")]
    [SerializeField] private TMP_Text frameRateText;
    [SerializeField] private Slider frameRateSlider;
    [SerializeField] private int minFrameRate = 30;
    [SerializeField] private int maxFrameRate = 144;

    [Header("Volume")]
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text ambientVolumeText;
    [SerializeField] private Slider ambientVolumeSlider;
    [SerializeField] private TMP_Text bgmVolumeText;
    [SerializeField] private Slider bgmVolumeSlider;

    [Header("Key Bindings")]
    [SerializeField] private KeyBindingProfile defaultProfile;
    [SerializeField] private TMP_Text[] keyBindingTexts;
    [SerializeField] private Button[] rebindButtons;

    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;

    [Header("Pop Up Window")]
    [SerializeField] private GameObject confirmationPopup;
    [SerializeField] private Button popupApply;
    [SerializeField] private Button popupCancle;

    private float tempResolutionIndex;
    private bool tempFullscreen;
    private float tempFrameRate;
    private float tempMasterVolume;
    private float tempSFXVolume;
    private float tempAmbientVolume;
    private float tempBGMVolume;

    // Panel =================================================================

    public void ShowVideoSettings()
    {
        videoSettingsPanel.SetActive(true);
        soundSettingsPanel.SetActive(false);
        keyBindingSettingsPanel.SetActive(false);
    }

    public void ShowSoundSettings()
    {
        videoSettingsPanel.SetActive(false);
        soundSettingsPanel.SetActive(true);
        keyBindingSettingsPanel.SetActive(false);
    }

    public void ShowKeyBindingSettings()
    {
        keyBindingSettingsPanel.SetActive(true);
        videoSettingsPanel.SetActive(false);
        soundSettingsPanel.SetActive(false);
    }

    // =======================================================================

    private void Start()
    {
        if (videoSettingsPanel != null)
            videoSettingsPanel.SetActive(false);

        if (soundSettingsPanel != null)
            soundSettingsPanel.SetActive(false);

        if (keyBindingSettingsPanel != null)
            keyBindingSettingsPanel.SetActive(false);

        applyButton.onClick.AddListener(ApplySettings);
        resetButton.onClick.AddListener(LoadSettings);
        popupApply.onClick.AddListener(OnConfirmExit);
        popupCancle.onClick.AddListener(() => confirmationPopup.SetActive(false));

        InitializeSettings();
        InitializeKeyBindings();
        LoadSettings();
    }

    private void InitializeSettings()
    {
        videoSettingsButton.onClick.AddListener(ShowVideoSettings);
        soundSettingsButton.onClick.AddListener(ShowSoundSettings);
        keyBindingSettingsButton.onClick.AddListener(ShowKeyBindingSettings);

        resolutionSlider.maxValue = availableResolutions.Length - 1;
        resolutionSlider.wholeNumbers = true; // 정수
        resolutionSlider.onValueChanged.AddListener(OnResolutionChanged);

        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        frameRateSlider.minValue = minFrameRate;
        frameRateSlider.maxValue = maxFrameRate;
        frameRateSlider.wholeNumbers = true;
        frameRateSlider.onValueChanged.AddListener(OnFrameRateChanged);

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
    }

    private void InitializeKeyBindings()
    {
        for (int i = 0; i < rebindButtons.Length; i++)
        {
            int index = i;
            rebindButtons[i].onClick.AddListener(() => StartRebinding(index));
        }
        UpdateKeyBindingUI();
    }

    private void LoadSettings()
    {
        tempResolutionIndex = PlayerPrefs.GetFloat("ResolutionIndex", 1);
        tempFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        tempFrameRate = PlayerPrefs.GetFloat("FrameRate", 300f);
        tempMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        tempSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        tempAmbientVolume = PlayerPrefs.GetFloat("AmbientVolume", 1f);
        tempBGMVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);

        // UI 업데이트
        resolutionSlider.value = tempResolutionIndex;
        fullscreenToggle.isOn = tempFullscreen;
        frameRateSlider.value = tempFrameRate;
        masterVolumeSlider.value = tempMasterVolume;
        sfxVolumeSlider.value = tempSFXVolume;
        ambientVolumeSlider.value = tempAmbientVolume;
        bgmVolumeSlider.value = tempBGMVolume;
    }

    private void OnResolutionChanged(float value)
    {
        tempResolutionIndex = value;
        int index = Mathf.RoundToInt(value);
        if (index < availableResolutions.Length)
        {
            Resolution res = availableResolutions[index];
            resolutionText.text = $"해상도        {res}";
        }
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        tempFullscreen = isFullscreen;
        fullscreenFill.enabled = isFullscreen;
    }

    private void OnFrameRateChanged(float value)
    {
        tempFrameRate = value;
        int frameRate = Mathf.Max(minFrameRate, Mathf.RoundToInt(value));
        frameRateText.text = $"최대 프레임     {frameRate} FPS";
    }

    // private void OnResolutionChanged(float value)
    // {
    //     tempResolutionIndex = value;
    //     int index = Mathf.RoundToInt(value);
    //     if (index < availableResolutions.Length)
    //     {
    //         Resolution res = availableResolutions[index];
    //         resolutionText.text = $"해상도        {res}";

    //         FullScreenMode fullScreenMode = fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

    //         Screen.SetResolution(res.width, res.height, fullScreenMode);

    //         Debug.Log($"해상도 변경: {res.width}x{res.height} @{res.refreshRate}Hz");
    //         PlayerPrefs.SetFloat("ResolutionIndex", value);
    //     }
    // }

    // private void OnFullscreenChanged(bool isFullscreen)
    // {
    //     tempFullscreen = isFullscreen;
    //     Screen.fullScreen = isFullscreen;
    //     fullscreenFill.enabled = isFullscreen;
    //     Debug.Log($"전체 화면: {(isFullscreen ? "활성화" : "비활성화")}");
    //     PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    // }

    // private void OnFrameRateChanged(float value)
    // {
    //     tempFrameRate = value;
    //     int frameRate = Mathf.Max(minFrameRate, Mathf.RoundToInt(value));
    //     frameRateText.text = $"최대 프레임     {frameRate} FPS";
    //     Application.targetFrameRate = frameRate;
    //     PlayerPrefs.SetFloat("FrameRate", value);
    // }

    // Audio ===============================================

    private void OnMasterVolumeChanged(float value)
    {
        masterVolumeText.text = $"전체 음량      {Mathf.RoundToInt(value * 100)}%";
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        sfxVolumeText.text = $"효과음       {Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void OnAmbientVolumeChanged(float value)
    {
        ambientVolumeText.text = $"환경음       {Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.SetFloat("AmbientVolume", value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        bgmVolumeText.text = $"배경음       {Mathf.RoundToInt(value * 100)}%";
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    // Key =================================

    private void StartRebinding(int actionIndex)
    {
        InputAction action = GetActionByIndex(actionIndex);
        action.Disable();

        rebindButtons[actionIndex].GetComponentInChildren<TextMeshProUGUI>().text = "[...]";

        // Composite
        int bindingIndex = actionIndex switch
        {
            0 => action.bindings.IndexOf(x => x.name == "up"),
            1 => action.bindings.IndexOf(x => x.name == "down"),
            2 => action.bindings.IndexOf(x => x.name == "left"),
            3 => action.bindings.IndexOf(x => x.name == "right"),
            _ => 0
        };

        var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => CompleteRebinding(operation, actionIndex))
            .Start();
    }

    private void CompleteRebinding(InputActionRebindingExtensions.RebindingOperation operation, int actionIndex)
    {
        try
        {
            InputAction action = GetActionByIndex(actionIndex);
            int bindingIndex = actionIndex switch
            {
                0 => action.bindings.IndexOf(x => x.name == "up"),
                1 => action.bindings.IndexOf(x => x.name == "down"),
                2 => action.bindings.IndexOf(x => x.name == "left"),
                3 => action.bindings.IndexOf(x => x.name == "right"),
                _ => 0
            };

            string newBinding = action.bindings[bindingIndex].effectivePath;

            // 충돌 검사
            for (int i = 0; i < keyBindingTexts.Length; i++)
            {
                if (i == actionIndex) continue;

                InputAction otherAction = GetActionByIndex(i);
                int otherBindingIndex = i switch
                {
                    0 => otherAction.bindings.IndexOf(x => x.name == "up"),
                    1 => otherAction.bindings.IndexOf(x => x.name == "down"),
                    2 => otherAction.bindings.IndexOf(x => x.name == "left"),
                    3 => otherAction.bindings.IndexOf(x => x.name == "right"),
                    _ => 0
                };

                if (otherAction.bindings[otherBindingIndex].effectivePath == newBinding)
                {
                    // NULL 로 초기화
                    otherAction.ApplyBindingOverride(otherBindingIndex, "<Empty>");
                    Debug.Log($"키 충돌 감지: {i}번 바인딩이 초기화됨");
                }
            }

            operation.Dispose();
            UpdateKeyBindingUI();
            SaveKeyBindings();
            action.Enable();
        }

        finally
        {
            operation.Dispose();
        }

    }

    private InputAction GetActionByIndex(int index)
    {
        switch (index)
        {
            case 0: return defaultProfile.movement.action; // U
            case 1: return defaultProfile.movement.action; // D
            case 2: return defaultProfile.movement.action; // L
            case 3: return defaultProfile.movement.action; // R
            case 4: return defaultProfile.interact.action; // E
            case 5: return defaultProfile.autoAttack.action; // Q
            case 6: return defaultProfile.dash.action; // Shift
            case 7: return defaultProfile.showInfo.action; // Tab
            default: return null;
        }
    }

    private void UpdateKeyBindingUI()
    {
        for (int i = 0; i < keyBindingTexts.Length; i++)
        {
            InputAction action = GetActionByIndex(i);
            int bindingIndex = i switch
            {
                0 => action.bindings.IndexOf(x => x.name == "up"),
                1 => action.bindings.IndexOf(x => x.name == "down"),
                2 => action.bindings.IndexOf(x => x.name == "left"),
                3 => action.bindings.IndexOf(x => x.name == "right"),
                _ => 0
            };

            string keyName = InputControlPath.ToHumanReadableString(
                action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);

            // 키 네임 수정
            keyName = keyName switch
            {
                "Left Shift" => "LShift",
                "Right Shift" => "RShift",
                _ => keyName
            };

            // NULL
            string displayText;
            if (action.bindings[bindingIndex].effectivePath == "<Empty>")
            {
                displayText = "[   ]";
            }
            else
            {
                displayText = $"[{keyName}]";
            }

            rebindButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = displayText;

            string actionName = i switch
            {
                0 => "위로 이동",
                1 => "아래로 이동",
                2 => "왼쪽으로 이동",
                3 => "오른쪽으로 이동",
                4 => "상호작용",
                5 => "자동 공격",
                6 => "대쉬",
                7 => "스킬 정보",
                _ => "알 수 없음"
            };
            keyBindingTexts[i].text = actionName;
        }
    }

    private void SaveKeyBindings()
    {
        for (int i = 0; i < keyBindingTexts.Length; i++)
        {
            InputAction action = GetActionByIndex(i);
            if (action == null) continue;

            int bindingIndex = i switch
            {
                0 => action.bindings.IndexOf(x => x.name == "up"),
                1 => action.bindings.IndexOf(x => x.name == "down"),
                2 => action.bindings.IndexOf(x => x.name == "left"),
                3 => action.bindings.IndexOf(x => x.name == "right"),
                _ => 0
            };

            string bindingPath = action.bindings[bindingIndex].effectivePath;
            PlayerPrefs.SetString($"KeyBinding_{i}", bindingPath);
        }
    }

    private void LoadKeyBindings()
    {
        for (int i = 0; i < keyBindingTexts.Length; i++)
        {
            string savedBinding = PlayerPrefs.GetString($"KeyBinding_{i}", string.Empty);
            if (!string.IsNullOrEmpty(savedBinding))
            {
                InputAction action = GetActionByIndex(i);
                var bindingIndex = action.GetBindingIndexForControl(action.controls[0]);
                action.ApplyBindingOverride(bindingIndex, savedBinding);
            }
        }
        UpdateKeyBindingUI();
    }

    private void ApplySettings()
    {
        // 해상도 적용
        int index = Mathf.RoundToInt(tempResolutionIndex);
        if (index < availableResolutions.Length)
        {
            Resolution res = availableResolutions[index];
            Screen.SetResolution(res.width, res.height, tempFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        // 프레임레이트 적용
        Application.targetFrameRate = Mathf.RoundToInt(tempFrameRate);

        // 볼륨 적용
        AudioListener.volume = tempMasterVolume;

        // PlayerPrefs에 저장
        PlayerPrefs.SetFloat("ResolutionIndex", tempResolutionIndex);
        PlayerPrefs.SetInt("Fullscreen", tempFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("FrameRate", tempFrameRate);
        PlayerPrefs.SetFloat("MasterVolume", tempMasterVolume);
        PlayerPrefs.SetFloat("SFXVolume", tempSFXVolume);
        PlayerPrefs.SetFloat("AmbientVolume", tempAmbientVolume);
        PlayerPrefs.SetFloat("BGMVolume", tempBGMVolume);

        PlayerPrefs.Save();
    }

    // =====
    // 옵션 설정 확인 프로세스
    // =====

    public void OnSettingsExit()
    {
        if (HasUnsavedChanges())
        {
            confirmationPopup.SetActive(true);
        }
        else
        {
            CloseSettings();
        }
    }

    private bool HasUnsavedChanges()
    {
        return tempResolutionIndex != PlayerPrefs.GetFloat("ResolutionIndex") ||
               tempFullscreen != (PlayerPrefs.GetInt("Fullscreen") == 1) ||
               tempFrameRate != PlayerPrefs.GetFloat("FrameRate") ||
               tempMasterVolume != PlayerPrefs.GetFloat("MasterVolume") ||
               tempSFXVolume != PlayerPrefs.GetFloat("SFXVolume") ||
               tempAmbientVolume != PlayerPrefs.GetFloat("AmbientVolume") ||
               tempBGMVolume != PlayerPrefs.GetFloat("BGMVolume");
    }

    private void OnConfirmExit()
    {
        LoadSettings();
        confirmationPopup.SetActive(false);
        CloseSettings();
    }

    private void CloseSettings()
    {
        videoSettingsPanel.SetActive(false);
        soundSettingsPanel.SetActive(false);
        keyBindingSettingsPanel.SetActive(false);
        gameObject.SetActive(false);
    }
}
