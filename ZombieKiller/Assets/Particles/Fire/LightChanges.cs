using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightChanges : MonoBehaviour
{
    Light myLight;

    void Start()
    {
        myLight = GetComponent<Light>();
    }
    void Update()
    {
        int randomIntensity = Random.Range(5, 9);
        myLight.intensity = randomIntensity;
    }
}
