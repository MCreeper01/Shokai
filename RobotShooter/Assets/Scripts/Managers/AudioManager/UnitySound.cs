using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitySound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
    public AudioSource source;
}
