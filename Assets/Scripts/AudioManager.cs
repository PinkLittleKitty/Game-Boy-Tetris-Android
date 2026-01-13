using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip clickOkClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip rotateClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string lowpassParameterName = "Lowpass";
    [SerializeField] private float muffledLowPassCutoff = 100f;
    [SerializeField] private float normalLowPassCutoff = 5000f;

    private Dictionary<GlobalSfx, AudioClip> _clipsDictionary = new Dictionary<GlobalSfx, AudioClip>();
    
    private bool _isMuffled = false;
    public static AudioManager instance;
    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);

        _isMuffled = PlayerPrefs.GetInt("SoundMuffled", 0) == 1;
    }
    void Start()
    {
        _clipsDictionary[GlobalSfx.Click] = clickOkClip;
        _clipsDictionary[GlobalSfx.Move] = moveClip;
        _clipsDictionary[GlobalSfx.Rotate] = rotateClip;
        _clipsDictionary[GlobalSfx.Land] = landClip;
        _clipsDictionary[GlobalSfx.LevelUp] = levelUpClip;
        _clipsDictionary[GlobalSfx.GameOver] = gameOverClip;

        if (_isMuffled)
        {
            ApplyMuffledVolume();
        }
        else
        {
            RestoreNormalVolume();
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        sfxAudioSource.PlayOneShot(clip);
    }

    public void ChangeMusic(AudioClip clip)
    {
        musicAudioSource.clip = clip;
        musicAudioSource.Play();
    }

    public void PlaySfx(GlobalSfx clipKey)
    {
        sfxAudioSource.pitch = 1f;
        _clipsDictionary.TryGetValue(clipKey, out AudioClip clip);
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlaySfxWithPitch(GlobalSfx clipKey, float pitch)
    {
        sfxAudioSource.pitch = pitch;
        _clipsDictionary.TryGetValue(clipKey, out AudioClip clip);
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlaySfxRandomPitch(AudioClip clip)
    {
        //sfxAudioSource.pitch = Mathf.Lerp(minRandomPitch, maxRandomPitch,Random.value);
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlaySoundAtPosition(GlobalSfx clipKey, Vector3 pos)
    {
        _clipsDictionary.TryGetValue(clipKey, out AudioClip clip);
        AudioSource.PlayClipAtPoint(clip, pos);
    }

    public bool IsMuffled()
    {
        return _isMuffled;
    }

    public void ToggleMuffledSound()
    {
        _isMuffled = !_isMuffled;
        
        if (_isMuffled)
        {
            ApplyMuffledVolume();
        }
        else
        {
            RestoreNormalVolume();
        }
        
        PlayerPrefs.SetInt("SoundMuffled", _isMuffled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("Muffled sound toggled: " + _isMuffled);
    }

    private void ApplyMuffledVolume()
    {
        bool success = audioMixer.SetFloat(lowpassParameterName, muffledLowPassCutoff);
        Debug.Log("Applied muffled volume. Success: " + success + ", Value: " + muffledLowPassCutoff);
    }
    
    private void RestoreNormalVolume()
    {
        bool success = audioMixer.SetFloat(lowpassParameterName, normalLowPassCutoff);
        Debug.Log("Restored normal volume. Success: " + success + ", Value: " + normalLowPassCutoff);
    }
}

public enum GlobalSfx
{
    Click,
    Move,
    Rotate,
    Land,
    LevelUp,
    GameOver
}