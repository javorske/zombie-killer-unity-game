using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour
{
    public int timeTillSinking = 10;
    float destroyHeight;

    void Start()
    {
        if (this.gameObject.tag == "Ragdoll")
        {
            Invoke("StartSink", 5);
        }
    }

    public void StartSink()
    {
        destroyHeight = Terrain.activeTerrain.SampleHeight(this.transform.position) - 5;
        Collider[] colList = this.transform.GetComponentsInChildren<Collider>();
        foreach (Collider c in colList)
        {
            Destroy(c);
        }
        InvokeRepeating("SinkIntoGround", timeTillSinking, 0.1f);
    }

    public void SinkIntoGround()
    {
        if (this.gameObject.tag == "Ragdoll")
        {
            this.transform.Translate(0, -0.5f, 0);
        }
        else
            this.transform.Translate(0, -0.001f, 0);
        if (this.transform.position.y < destroyHeight)
        {
            Destroy(this.gameObject);
        }
    }
}
