using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    private void OnEnable()
    {
        EventManager.OnPlaySound += Play;
    }

    private void OnDisable()
    {
        EventManager.OnPlaySound -= Play;
    }
    private void Awake()
    {
        
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }
        
        s.source.Play();
        Debug.Log(s.name);
    }
}
