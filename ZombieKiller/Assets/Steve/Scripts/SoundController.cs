using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource shot;
    public AudioSource realodingSound;
    public void Reload()
    {
        realodingSound.Play();
    }
    public void Fire()
    {
        shot.Play();
    }
    public void CanShoot()
    {
        GameStats.canShoot = true;
    }
    public void CanReload()
    {
        GameStats.canReload = true;
    }
}
