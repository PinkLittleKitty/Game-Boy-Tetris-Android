using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip clickOkClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip grabClip;

    private Dictionary<GlobalSfx, AudioClip> _clipsDictionary = new Dictionary<GlobalSfx, AudioClip>();
    

    public static AudioManager instance;
    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    void Start()
    {
        _clipsDictionary[GlobalSfx.Click] = clickOkClip;
        _clipsDictionary[GlobalSfx.Error] = errorClip;
        _clipsDictionary[GlobalSfx.Grab] = grabClip;
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
}

public enum GlobalSfx
{
    Click,
    Error,
    Grab
}