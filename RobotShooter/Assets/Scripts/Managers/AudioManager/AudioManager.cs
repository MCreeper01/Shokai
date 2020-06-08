using UnityEngine.Audio;
using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class AudioManager : AController {

    public Sound[] sounds;
    private List<EventInstance> eventList;
    private List<SoundManagerMovingSound> positionEvents;

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
    }

    public void StartGame()
    {
        eventList = new List<EventInstance>();
        positionEvents = new List<SoundManagerMovingSound>();
    }

    public void Update() //Actualitzar posició sons 3D
    {
        if (positionEvents != null && positionEvents.Count > 0)
        {
            for (int i = 0; i < positionEvents.Count; i++)
            {
                PLAYBACK_STATE state;
                EventInstance eventInst = positionEvents[i].GetEventInstance();
                eventInst.getPlaybackState(out state);
                if (state == PLAYBACK_STATE.STOPPED)
                {
                    positionEvents.RemoveAt(i);
                }
                else
                {
                    eventInst.set3DAttributes(RuntimeUtils.To3DAttributes(positionEvents[i].GetTransform().position));
                }
            }
        }
    }
    
    public EventInstance PlayEvent(string name, Vector3 pos)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        EventInstance soundEvent = RuntimeManager.CreateInstance(s.path);
        if (!soundEvent.Equals(null))
        {
            soundEvent.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
            soundEvent.start();
            eventList.Add(soundEvent);
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
            soundEvent.start();
            SoundManagerMovingSound movingSound = new SoundManagerMovingSound(t, soundEvent);
            positionEvents.Add(movingSound);
            eventList.Add(soundEvent);
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
            soundEvent.start();
            SoundManagerMovingSound movingSound = new SoundManagerMovingSound(t, soundEvent);
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
            soundEvent.start();
            soundEvent.release();
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

    public void Pause(EventInstance soundEvent)
    {        
        if (eventList.Contains(soundEvent))
        {
            soundEvent.setPaused(true);
        }        
    }


    public void Resume(EventInstance soundEvent)
    {
        if (eventList.Contains(soundEvent))
        {
            soundEvent.setPaused(true);
        }
    }

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
    }

    public void PlayAll()
    {
        foreach (EventInstance e in eventList)
        {
            e.start();
        }
    }

    public void PauseAll()
    {
        foreach (EventInstance e in eventList)
        {
            e.setPaused(true);
        }
    }

    public void ResumeAll()
    {
        foreach (EventInstance e in eventList)
        {
            e.setPaused(false);
        }
    }

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
    }

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
    
    public SoundManagerMovingSound(Transform transform, EventInstance eventIns)
    {
        this.transform = transform;
        this.eventIns = eventIns;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public EventInstance GetEventInstance()
    {
        return eventIns;
    }
}
