using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource uiSource;

    [SerializeField] private Sound[] sfxSounds;
    [SerializeField] private Sound[] bgmSounds;
    [SerializeField] private Sound[] ambientSounds;
    [SerializeField] private Sound[] uiSounds;

    // 각각의 Dictionary 추가
    private Dictionary<string, Sound> sfxDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> bgmDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> ambientDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> uiDictionary = new Dictionary<string, Sound>();

    private Dictionary<string, int> currentPlayingSounds = new Dictionary<string, int>();
    private const int MAX_SIMULTANEOUS_SOUNDS = 3;

    private string currentBGM = "";
    string newBGM = "";

    private void Awake()
    {
        foreach (Sound sfx in sfxSounds)
        {
            sfxDictionary[sfx.name] = sfx;
        }

        foreach (Sound bgm in bgmSounds)
        {
            bgmDictionary[bgm.name] = bgm;
        }

        foreach (Sound ambient in ambientSounds)
        {
            ambientDictionary[ambient.name] = ambient;
        }

        foreach (Sound ui in uiSounds)
        {
            uiDictionary[ui.name] = ui;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "0_Title":
                newBGM = "B1";
                break;
            case "1_Hub":
                newBGM = "B2";
                break;
            case "2_Stage_Test":
                newBGM = "B3";
                break;

            default:
                break;
        }

        if (newBGM != currentBGM)
        {
            currentBGM = newBGM;
            PlayBGM(currentBGM);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName != "2_Stage_Test")
            {
                PlayUI("U1");
            }
        }
    }

    public void PlaySFX(string soundName)
    {
        // 현재 재생 중인 사운드 수 확인
        if (!currentPlayingSounds.ContainsKey(soundName))
        {
            currentPlayingSounds[soundName] = 0;
        }

        // 최대 재생 수를 초과하면 재생하지 않음
        if (currentPlayingSounds[soundName] >= MAX_SIMULTANEOUS_SOUNDS)
        {
            return;
        }

        if (sfxDictionary.TryGetValue(soundName, out Sound sound))
        {
            currentPlayingSounds[soundName]++;
            sfxSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            sfxSource.PlayOneShot(sound.clip);

            // 사운드 종료 후 카운트 감소
            StartCoroutine(DecreaseSoundCount(soundName, sound.clip.length));
        }
    }

    public void PlayBGM(string soundName)
    {
        if (bgmDictionary.TryGetValue(soundName, out Sound sound))
        {
            bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];  // BGM 믹서 그룹 사용
            bgmSource.clip = sound.clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayAmbient(string soundName)
    {
        if (ambientDictionary.TryGetValue(soundName, out Sound sound))
        {
            ambientSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Ambient")[0];
            ambientSource.PlayOneShot(sound.clip);
        }
    }

    public void PlayUI(string soundName)
    {
        if (uiDictionary.TryGetValue(soundName, out Sound sound))
        {
            uiSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];  // UI 믹서 그룹 사용
            uiSource.PlayOneShot(sound.clip);
        }
    }

    // =========================================================

    private IEnumerator DecreaseSoundCount(string soundName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentPlayingSounds.ContainsKey(soundName))
        {
            currentPlayingSounds[soundName]--;
        }
    }
}
