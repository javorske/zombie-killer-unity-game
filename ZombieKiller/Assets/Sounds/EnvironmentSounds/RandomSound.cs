using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    public AudioSource sound;
    public float randomMin;
    public float randomMax;
    void Start()
    {
        Invoke("PlaySound", Random.Range(randomMin, randomMax));
    }

    void PlaySound()
    {
        GameObject newSound = new GameObject();
        AudioSource newAS = newSound.AddComponent<AudioSource>();
        newAS.clip = sound.clip;
        newAS.Play();
        Destroy(newSound, sound.clip.length);
        Invoke("PlaySound", Random.Range(randomMin, randomMax));
    }
}
