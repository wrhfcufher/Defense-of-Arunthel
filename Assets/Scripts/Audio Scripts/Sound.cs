using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
// Audio feature to hold AudioClips with associated name, pitch, volume, source, and loop settings
public class Sound
{
    public AudioClip clip;

    public string name;

    // Allows for adjusting pitch and volume in a range in the inspector
    [Range(0f, 1f)]
    public float pitch;
    [Range(.1f, 3f)]
    public float volume;

    // Determines whether the sound will play over and over
    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
