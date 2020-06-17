using UnityEngine.Audio;
using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : AController {

    public Sound[] sounds;
    private List<SoundEvents> eventList;
    private List<SoundManagerMovingSound> positionEvents;

    public AudioClip[] clips;
    public UnitySound[] unitySounds;

    public static AudioManager instance;

    [HideInInspector] public float musicVolume = 0.5f;
    [HideInInspector] public float fXVolume = 0.5f;
    [HideInInspector] public float masterVolume = 0.5f;

    [HideInInspector] public List<AudioSource> unitySources;
    public AudioSource crowdSource;

    public UnitySound[] musics;
    [HideInInspector] public int musicClipIndex = 0;

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

        unitySources.Add(crowdSource);
        crowdSource.volume = PlayerPrefs.GetFloat("fxVolume") * PlayerPrefs.GetFloat("generalVolume") * unitySounds[0].volume;
        crowdSource.Play();        
    }

    public void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            musicVolume = PlayerPrefs.GetFloat("musicVolume");
            masterVolume = PlayerPrefs.GetFloat("generalVolume");
            fXVolume = PlayerPrefs.GetFloat("fxVolume");
            StartCoroutine(FadeIn(0, 5));
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
    /*
    public void PlaySound(string sound, AudioSource source)
    {
        UnitySound s = Array.Find(unitySounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        
        source.volume = PlayerPrefs.GetFloat("generalVolume") * PlayerPrefs.GetFloat("fxVolume") * s.volume;

        source.Play();
    }*/

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

        for (int i = 0; i < musics.Length; i++)
        {
            musics[i].source.Pause();
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

        for (int i = 0; i < musics.Length; i++)
        {
            musics[i].source.volume = musics[i].volume * musicVolume * masterVolume;
            musics[i].source.UnPause();
        }
    }

    public IEnumerator FadeOut(UnitySound sound, float fadeTime)
    {
        float startVolume = sound.source.volume;

        while (sound.source.volume > 0)
        {
            sound.source.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        sound.source.Stop();
        sound.source.volume = 0;
    }

    public IEnumerator FadeIn(int sound, float FadeTime)
    {
        UnitySound s = musics[sound];
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            yield return null;
        }

        float startVolume = 0.2f;

        s.source.volume = 0;
        s.source.clip = s.clip;
        s.source.Play();

        while (s.source.volume < PlayerPrefs.GetFloat("generalVolume") * PlayerPrefs.GetFloat("musicVolume") * s.volume)
        {
            s.source.volume += startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        s.source.volume = PlayerPrefs.GetFloat("generalVolume") * PlayerPrefs.GetFloat("musicVolume") * s.volume;

    }

    public void PlayNextSong()
    {
        musicClipIndex++;
        if (musicClipIndex >= musics.Length - 1)
        {
            musicClipIndex = 1;
        }
        StartCoroutine(FadeIn(musicClipIndex, 5));
    }
    public void StopCurrentSong()
    {
        StartCoroutine(FadeOut(musics[musicClipIndex], 5));
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
