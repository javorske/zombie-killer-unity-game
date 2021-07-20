using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomZombieGrowling : MonoBehaviour
{
    public AudioSource sound;
    public float randomMin;
    public float randomMax;


    void Start()
    {
        Invoke("GrowlSound", Random.Range(randomMin, randomMax));
    }
    void GrowlSound()
    {
        if (!sound.isPlaying)
        {
            sound.Play();
        }
        Invoke("GrowlSound", Random.Range(randomMin, randomMax));
    }

}
