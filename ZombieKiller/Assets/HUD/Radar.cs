using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarObject
{
    public Image icon { get; set; }
    public GameObject owner { get; set; }
}

public class Radar : MonoBehaviour
{
    public Transform playerPosition;
    public float mapScale = 0.0f;

    public static List<RadarObject> radarObjects = new List<RadarObject>();

    public static void RegisterRadarObject(GameObject o, Image i)
    {
        Image image = Instantiate(i);
        radarObjects.Add(new RadarObject() { owner = o, icon = image });
    }

    public static void RemoveRadarObject (GameObject o)
    {
        List<RadarObject> newList = new List<RadarObject>();
        for (int i = 0; i < radarObjects.Count; i++)
        {
            if (radarObjects[i].owner == o)
            {
                Destroy(radarObjects[i].icon);
                continue;
            }
            else
            {
                newList.Add(radarObjects[i]);
            }
        }
        radarObjects.RemoveRange(0, radarObjects.Count);
        radarObjects.AddRange(newList);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPosition == null) return;
        foreach (RadarObject ro in radarObjects)
        {
            Vector3 radarPos = ro.owner.transform.position - playerPosition.position;
            float distanceToObject = Vector3.Distance(playerPosition.position, ro.owner.transform.position) * mapScale;

            float deltay = Mathf.Atan2(radarPos.x, radarPos.z) * Mathf.Rad2Deg - 270 - playerPosition.eulerAngles.y;
            radarPos.x = distanceToObject * Mathf.Cos(deltay * Mathf.Deg2Rad) * -1;
            radarPos.z = distanceToObject * Mathf.Sin(deltay * Mathf.Deg2Rad);

            ro.icon.transform.SetParent(this.transform);
            RectTransform rt = this.GetComponent<RectTransform>();
            ro.icon.transform.position = new Vector3(radarPos.x + rt.pivot.x, radarPos.z + rt.pivot.y, 0) + this.transform.position;
        }
    }
}
