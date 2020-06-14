using UnityEngine.Audio;
using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : AController {

    public Sound[] sounds;
    private List<SoundEvents> eventList;
    private List<SoundManagerMovingSound> positionEvents;

    public AudioClip[] clips;

    public static AudioManager instance;

    [HideInInspector] public float musicVolume = 0.5f;
    [HideInInspector] public float fXVolume = 0.5f;
    [HideInInspector] public float masterVolume = 0.5f;

    [HideInInspector] public List<AudioSource> unitySources;
    public AudioSource source;

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
        unitySources = new List<AudioSource>();
        //source = GetComponent<AudioSource>();
        
    }

    public void StartGame()
    {
        eventList = new List<SoundEvents>();
        positionEvents = new List<SoundManagerMovingSound>();

        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            unitySources.Add(source);
            source.volume *= fXVolume * masterVolume;
            //source.Play();
        }        
    }
    /*
    public void Update() //Actualitzar posició sons 3D
    {
        if (positionEvents != null && positionEvents.Count > 0)
        {
            for (int i = 0; i < positionEvents.Count; i++)
            {
                PLAYBACK_STATE state;
                Sound s = positionEvents[i].GetSound();
                EventInstance eventInst = positionEvents[i].GetEventInstance();
                eventInst.getPlaybackState(out state);
                if (state == PLAYBACK_STATE.STOPPED)
                {
                    positionEvents.RemoveAt(i);
                }
                else
                {
                    //eventInst.setVolume(s.volume * fXVolume * masterVolume);
                    eventInst.set3DAttributes(RuntimeUtils.To3DAttributes(positionEvents[i].GetTransform().position));
                }
            }
        }
    }*/
    
    public EventInstance PlayEvent(string name, Vector3 pos)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        EventInstance soundEvent = RuntimeManager.CreateInstance(s.path);
        if (!soundEvent.Equals(null))
        {
            soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
            soundEvent.setVolume(s.volume * fXVolume * masterVolume);
            soundEvent.start();
            SoundEvents e = new SoundEvents();
            e.eventInstance = soundEvent;
            e.volume = s.volume;
            eventList.Add(e);
        }
        return soundEvent;
    }

    public EventInstance PlayEvent(string name, Transform t) //Utilitzem per objectes en moviment que actualitzen la posició
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        EventInstance soundEvent = RuntimeManager.CreateInstance(s.path);
        if (!soundEvent.Equals(null))
        {
            soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(t.position));
            soundEvent.setVolume(s.volume * fXVolume * masterVolume);
            soundEvent.start();
            SoundManagerMovingSound movingSound = new SoundManagerMovingSound(t, soundEvent, s);
            positionEvents.Add(movingSound);
            SoundEvents e = new SoundEvents();
            e.eventInstance = soundEvent;
            e.volume = s.volume;
            eventList.Add(e);
        }
        return soundEvent;
    }

    public EventInstance PlayOneShotSound(string name, Transform t) //Utilitzem per objectes en moviment que actualitzen la posició
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        EventInstance soundEvent = RuntimeManager.CreateInstance(s.path);
        if (!soundEvent.Equals(null))
        {
            soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(t.position));
            soundEvent.setVolume(s.volume * fXVolume * masterVolume);
            soundEvent.start();
            SoundManagerMovingSound movingSound = new SoundManagerMovingSound(t, soundEvent, s);
            positionEvents.Add(movingSound);
            soundEvent.release();
        }
        return soundEvent;
    }

    public EventInstance PlayOneShotSound(string name, Vector3 pos) //Utilitzem per objectes que no actualitzen la posició
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        EventInstance soundEvent = RuntimeManager.CreateInstance(s.path);
        if (!soundEvent.Equals(null))
        {
            soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            soundEvent.setVolume(s.volume * fXVolume * masterVolume);
            soundEvent.start();
            soundEvent.release();
            if (s.name == "TurretShot")
            {
                SoundEvents e = new SoundEvents();
                e.eventInstance = soundEvent;
                e.volume = s.volume;
                eventList.Add(e);
            }            
        }
        return soundEvent;
    }

    // Usamos esta para objetos con parámetros
    public EventInstance PlayOneShotSound(string name, Vector3 pos, List<SoundManagerParameter> parameters = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        EventInstance soundEvent = RuntimeManager.CreateInstance(s.path);
        if (!soundEvent.Equals(null))
        {
            if (parameters != null)
                for (int i = 0; i < parameters.Count; i++)
                    soundEvent.setParameterByName(parameters[i].GetName(), parameters[i].GetValue());

            soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
            soundEvent.setVolume(s.volume * fXVolume * masterVolume);
            soundEvent.start();
            soundEvent.release();
        }
        return soundEvent;
    }

    public void UpdateEventParameter(EventInstance soundEvent, SoundManagerParameter parameter)
    {
        soundEvent.setParameterByName(parameter.GetName(), parameter.GetValue());
    }

    public void UpdateEventParameters(EventInstance soundEvent, List<SoundManagerParameter> parameters)
    {
        for (int i = 0; i < parameters.Count; i++)
            soundEvent.setParameterByName(parameters[i].GetName(), parameters[i].GetValue());
    }
    /*
    public void Pause(EventInstance soundEvent)
    {        
        if (eventList.Contains(soundEvent))
        {
            soundEvent.setPaused(true);
        }        
    }*/

    /*
    public void Resume(EventInstance soundEvent)
    {
        if (eventList.Contains(soundEvent))
        {
            soundEvent.setPaused(true);
        }
    }*/
    /*
    public void Stop(EventInstance soundEvent, bool fadeout = true)
    {
        soundEvent.clearHandle();
        if (eventList.Remove(soundEvent))
        {
            if (fadeout) soundEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            else
            {
                soundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }*/

    public void PlayAll()
    {
        foreach (SoundEvents e in eventList)
        {
            e.eventInstance.start();
        }
    }

    public void PauseAll()
    {
        foreach (SoundEvents e in eventList)
        {
            e.eventInstance.setPaused(true);
        }

        foreach (AudioSource a in unitySources)
        {
            a.Pause();
        }
    }

    public void ResumeAll()
    {                
        for (int i = eventList.Count - 1; i >= 0; i--)
        {
            PLAYBACK_STATE state;
            eventList[i].eventInstance.getPlaybackState(out state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                eventList.Remove(eventList[i]);
            }
        }

        foreach (SoundEvents e in eventList)
        {
            e.eventInstance.setVolume(e.volume * fXVolume * masterVolume);
            e.eventInstance.setPaused(false);
        }

        foreach (AudioSource a in unitySources)
        {
            a.volume = a.volume * fXVolume * masterVolume;
            a.UnPause();
        }
    }
    /*
    public void StopAll(bool fadeout = true)
    {
        foreach (EventInstance e in eventList)
        {
            e.clearHandle();            
            if (fadeout) e.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            else
            {
                e.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }           
        }
        eventList.Clear();
    }*/

    public bool isPlaying(EventInstance soundEvent)
    {
        PLAYBACK_STATE state;
        soundEvent.getPlaybackState(out state);
        return !state.Equals(PLAYBACK_STATE.STOPPED);
    }

    public void SetChannelVolume(string channel, float volume)
    {
        VCA vca;
        if (RuntimeManager.StudioSystem.getVCA("vca:/" + channel, out vca) != FMOD.RESULT.OK)
        {
            return;
        }
        vca.setVolume(volume);
    }
}

public class SoundManagerParameter
{
    string name;
    float value;

    public SoundManagerParameter(string name, float value)
    {
        this.name = name;
        this.value = value;
    }

    public string GetName()
    {
        return name;
    }

    public float GetValue()
    {
        return value;
    }
}

class SoundManagerMovingSound
{
    Transform transform;
    EventInstance eventIns;
    Sound sound;
    
    public SoundManagerMovingSound(Transform transform, EventInstance eventIns, Sound sound)
    {
        this.transform = transform;
        this.eventIns = eventIns;
        this.sound = sound;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public EventInstance GetEventInstance()
    {
        return eventIns;
    }

    public Sound GetSound()
    {
        return sound;
    }
}

class SoundEvents
{
    public EventInstance eventInstance;
    public float volume;
}
