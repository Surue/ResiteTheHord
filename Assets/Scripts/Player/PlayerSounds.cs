using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSounds : MonoBehaviour {
    
    [Header("Audio Source")]
    [SerializeField] AudioSource uniqueAudioSource;
    [SerializeField] List<AudioSource> fireAudioSource;

    [Header("AudioClip")]
    [SerializeField] List<AudioClip> spawnSounds;
    [SerializeField] List<AudioClip> fireSounds;
    [SerializeField] List<AudioClip> damageSounds;

    public void Spawn() {
        uniqueAudioSource.clip = spawnSounds[(int) (RandomSeed.GetValue() * spawnSounds.Count)];
        uniqueAudioSource.Play();
    }

    public void Fire() {
        foreach (AudioSource audioSource in fireAudioSource) {
            if (!audioSource.isPlaying) {
                audioSource.clip = fireSounds[(int)(RandomSeed.GetValue() * spawnSounds.Count)];
                audioSource.Play();
                return;
            }
        }
    }

    public void Damage() {
        uniqueAudioSource.clip = damageSounds[(int)(RandomSeed.GetValue() * spawnSounds.Count)];
        uniqueAudioSource.Play();
    }
}
