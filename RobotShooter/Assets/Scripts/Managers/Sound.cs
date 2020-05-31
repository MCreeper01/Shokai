using UnityEngine.Audio;
using UnityEngine;
using FMODUnity;

[System.Serializable]
public class Sound {

    public string name;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public StudioEventEmitter source;
}
