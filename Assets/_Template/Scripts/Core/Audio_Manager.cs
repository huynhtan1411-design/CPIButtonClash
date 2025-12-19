using System;
using UISystems;
using UnityEngine;
using System.Collections;


public enum AudioType { Music, Sound }

public class Audio_Manager : MonoBehaviour
{
    public Sound[] musics;
    public Sound[] sounds;
    public AudioSource audioSourceBG = null;
    public bool soundMute = false, musicMute = false;
    public bool playStartRandom = false;
    private Sound currentPlayingMusic = null;

    #region singleton
    public static Audio_Manager instance;
    private static bool isInitialized = false;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Init();
        }
    }

    #endregion

    void Start()
    {


    }

    void Init()
    {
        isInitialized = true;
        // Initialize music sources
        foreach (Sound s in musics)
        {
            if (s == null)
                continue;
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = s.Clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            source.playOnAwake = false;
            s.Source = source;
        }
        // Initialize sound sources
        foreach (Sound s in sounds)
        {
            if (s == null)
                continue;
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = s.Clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            source.playOnAwake = false;
            s.Source = source;
        }
    }

    public void GetData()
    {
        soundMute = PlayerPrefsManager.GetSoundState() == 0 ? true : false;
        musicMute = PlayerPrefsManager.GetMusicState() == 0 ? true : false;
        if (musicMute)
            StopMusic();
        else
            PlayMusic();
    }

    public void play(string soundName)
    {
        // Try to find and play music first
        Sound music = Array.Find(musics, m => m.name == soundName);
        if (music != null && !musicMute)
        {
            // If this music is already playing, don't do anything
            if (currentPlayingMusic == music && music.Source.isPlaying)
                return;

            // Stop current music if any
            if (currentPlayingMusic != null && currentPlayingMusic.Source != null)
            {
                currentPlayingMusic.Source.Stop();
            }

            if (playStartRandom)
            {
                float randomStartTime = UnityEngine.Random.Range(0f, music.Source.clip.length);
                music.Source.time = randomStartTime;
            }
            music.Source.Play();
            currentPlayingMusic = music;
            return;
        }

        // If not found in music, try to play as sound
        Sound s = Array.Find(sounds, so => so.name == soundName);
        if (s == null || s.Source == null)
            return;

        if (!soundMute && s.instanceCount < 15)
        {
            s.instanceCount++;
            s.Source.PlayOneShot(s.Clip);
            StartCoroutine(DecrementInstanceCountAfterPlay(s, s.Clip.length));
        }
    }

    private System.Collections.IEnumerator DecrementInstanceCountAfterPlay(Sound sound, float delay)
    {
        yield return new WaitForSeconds(delay);
        sound.instanceCount = Mathf.Max(0, sound.instanceCount - 1);
    }

    public void stop(string soundName)
    {

        Sound s = Array.Find(sounds, so => so.name == soundName);

        if (s == null)
        {
            Debug.LogError("Sound with name " + soundName + " doesn't exist!");
            return;
        }

        s.Source.Stop();

    }

    public void StopMusic()
    {
        if (currentPlayingMusic != null && currentPlayingMusic.Source != null)
        {
            currentPlayingMusic.Source.volume = 0f;
        }
    }

    public void PlayMusic()
    {
        if (musicMute)
            return;

        // If no music is currently assigned, start the first one
        if (currentPlayingMusic == null && musics.Length > 0 && musics[0] != null && musics[0].Source != null)
        {
            Sound music = musics[0];
            if (playStartRandom)
            {
                float randomStartTime = UnityEngine.Random.Range(0f, music.Source.clip.length);
                music.Source.time = randomStartTime;
            }
            music.Source.Play();
            currentPlayingMusic = music;
        }

        // Restore volume if music exists
        if (currentPlayingMusic != null && currentPlayingMusic.Source != null)
        {
            currentPlayingMusic.Source.volume = currentPlayingMusic.volume;
        }
    }

    public void SwitchStateMusic(bool value)
    {
        musicMute = value;
        PlayerPrefsManager.SetMusicState(musicMute ? 0 : 1);
        if (musicMute)
            StopMusic();
        else
            PlayMusic();
    }

    public void SwitchStateSound(bool value)
    {
        soundMute = value;
        PlayerPrefsManager.SetSoundState(soundMute ? 0 : 1);
    }

}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip Clip;


    //[HideInInspector]
    public AudioSource Source;

    public float volume = 1;
    public float pitch = 1;

    public bool loop = false;
    public bool playOnStart = false;

    [NonSerialized]
    public int instanceCount = 0;
}






