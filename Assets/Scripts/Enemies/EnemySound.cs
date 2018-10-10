using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySound : MonoBehaviour {

    [Header("Audio Source")]
    [SerializeField] AudioSource uniqueAudioSource;
    [SerializeField] List<AudioSource> attackAudioSource;


    [Header("AudioClip")]
    [SerializeField] List<AudioClip> spawnSounds;
    [SerializeField] List<AudioClip> attackSounds;
    [SerializeField] List<AudioClip> deathSounds;

    public void Spawn() {
        uniqueAudioSource.clip = spawnSounds[(int)(RandomSeed.GetValue() * spawnSounds.Count)];
        uniqueAudioSource.Play();
    }

    public void Attack() {
        foreach(AudioSource audioSource in attackAudioSource) {
            if(!audioSource.isPlaying) {
                audioSource.clip = attackSounds[(int)(RandomSeed.GetValue() * attackSounds.Count)];
                audioSource.Play();
                return;
            }
        }
    }

    public void Death() {
        uniqueAudioSource.clip = deathSounds[(int)(RandomSeed.GetValue() *deathSounds.Count)];
        uniqueAudioSource.Play();
    }
}
