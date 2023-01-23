using UnityEngine;
using UnityEngine.Audio;
using System;

// Audio feature managing sounds played during the game
public class AudioManager : MonoBehaviour
{
    // Array holding sound clips used in the game
    public Sound[] sounds;

    public static AudioManager instance;

    // Ensures there is only one instance of the AudioManager at a time
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
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.pitch = s.pitch;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
        }

    }

    // Finds a sound based on its name and plays it
    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    // Finds a sound based on its name and stops it
    public void Stop (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }
}

