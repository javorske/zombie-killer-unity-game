﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawn : MonoBehaviour
{
    public GameObject zombiePrefab;
    public int number;
    public float spawnRadius;
    public bool spawnOnStart = true;
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnAll();
        }
    }
    void SpawnAll()
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 randomPoint = this.transform.position + Random.insideUnitSphere * spawnRadius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
            {
                Instantiate(zombiePrefab, hit.position, Quaternion.identity);
            }
            else
                i--;
        }

        StartCoroutine(SelfDestruct());
    }
    private void OnTriggerEnter(Collider collider)
    {
        if (!spawnOnStart && collider.gameObject.tag == "Player")
        {
            SpawnAll();
        }
    }
    IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }
}
