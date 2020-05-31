using UnityEngine.Audio;
using System;
using UnityEngine;
using FMODUnity;
//using FMOD;

public class AudioManager : AController {

    public Sound[] sounds;

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

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<StudioEventEmitter>();

            s.source.EventInstance.setVolume(s.volume);
            s.source.EventInstance.setPitch(s.pitch);

            //s.source.EventInstance. = s.loop;
        }
    }

    public void StartGame()
    {
        
    }
  
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + " not found.");
            return;
        }
        s.source.Play();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + " not found.");
            return;
        }
        //s.source.Event;
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + " not found.");
            return;
        }
        s.source.Stop();
    }

    public void PlayAll()
    {
        foreach (Sound s in sounds)
        {
            s.source.Play();
        }
    }

    public void PauseAll()
    {
        foreach (Sound s in sounds)
        {
            s.source.EventInstance.setPaused(true);
        }
    }

    public void UnpauseAll()
    {
        foreach (Sound s in sounds)
        {
            s.source.EventInstance.setPaused(false);
        }
    }

    public void StopAll()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }

    public bool isPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + " not found.");
            return false;
        }
        return s.source.IsPlaying();
    }
    
    /*public void ChangeTime (string name, float time)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + " not found.");
            return;
        }
        s.source.time = time;
    }*/

    public void SetAllVolume(float volume)
    {
        foreach (Sound s in sounds)
        {
            s.source.EventInstance.setVolume(volume);
        }
    }
}
