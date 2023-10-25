using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    string currentMusic;

    Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        GetSoundsFromResources();
        AddAudioSources();
    }

    void GetSoundsFromResources()
    {
        Sound[] effects = Resources.LoadAll("AudioManager/Sounds/Effects", typeof(Sound)).Cast<Sound>().ToArray();
        Sound[] music = Resources.LoadAll("AudioManager/Sounds/Music", typeof(Sound)).Cast<Sound>().ToArray();
        sounds = effects.Concat(music).ToArray();
    }

    void AddAudioSources()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
        }
    }

    Sound FindSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            Debug.LogWarning("Sound: " + name + " not found!");
        else
            Debug.Log("Sound: " + name + " is finded!");
        return s;
    }

    #region Effects
    public void Play(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
            s.source.Play();
    }

    public void PlayOneShot(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
            if (!s.source.isPlaying)
                s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
            s.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
            s.source.Pause();
    }

    public void UnPause(string name)
    {
        Sound s = FindSound(name);
        if (s != null)
            s.source.UnPause();
    }

    #region Random

    public void PlayRandomPitch(string name, float pitchBase = 1)
    {
        Sound s = FindSound(name);
        if (s != null)
        {
            s.source.pitch = pitchBase + UnityEngine.Random.Range(-0.1f, 0.1f);
            PlayOneShot(name);
            s.source.pitch = 1;
        }
    }

    public void PlayRandomSound(string[] names)
    {
        List<Sound> list = new List<Sound>();
        foreach (var item in names)
        {
            Sound s = FindSound(item);
            if (s != null)
                list.Add(s);
        }

        int rnd = UnityEngine.Random.Range(0, list.Count);
        Play(list[rnd].name);
    }

    public void PlayRandomSoundAndPitch(string[] names, float pitchBase = 1)
    {
        List<Sound> list = new List<Sound>();
        foreach (var item in names)
        {
            Sound s = FindSound(item);
            if (s != null)
                list.Add(s);
        }

        int rnd = UnityEngine.Random.Range(0, list.Count);
        Sound s1 = (list[rnd]);

        s1.source.pitch = pitchBase + UnityEngine.Random.Range(-0.1f, 0.1f);
        PlayOneShot(name);
        s1.source.pitch = 1;
    }

    #endregion

    #endregion

    #region Music
    public void PlayMusic(string name)
    {
        if (currentMusic != string.Empty && currentMusic != name)
            Stop(currentMusic);

        Sound s = FindSound(name);
        if (s != null)
            if (!s.source.isPlaying)
            {
                currentMusic = name;
                s.source.Play();
            }
    }

    public void StopMusic()
    {
        Stop(currentMusic);
    }

    public void PauseMusic()
    {
        Pause(currentMusic);
    }

    public void UnPauseMusic()
    {
        UnPause(currentMusic);
    }
    #endregion

    public void AdjustVolume(string name, float volume)
    {
        Sound s = FindSound(name);
        if (s != null)
        {
            volume = Mathf.Clamp(volume, 0, 1);
            s.source.volume = volume;
        }
    }


}