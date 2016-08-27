using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    //private List<GameObject> impactList = new List<GameObject>();
    private List<MeshRenderer> rendererList = new List<MeshRenderer>();

    private List<ParticleSystem> particleList = new List<ParticleSystem>();
    public GameObject LevelHit;

    //public Queue<GameObject> hitQueue = new Queue<GameObject>();
    private int maxImpacts = 100;

    // Use this for initialization
    private void Start()
    {
        //impactList.Add(c);

        //if (impactList.Count > maxImpacts)
        //{
        //    Destroy(impactList[0]);
        //    impactList.RemoveAt(0);
        //}
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void CreateNextHit(int id)
    {
        //GameObject hit = impactList[id];
        rendererList[id].enabled = true;
        particleList[id].Play();

        //float t2 = Time.realtimeSinceStartup;
        //Debug.Log("Create next hit: " + (t2 - t1).ToString());
    }

    public int NewHit(RaycastHit hit, GameObject other)
    {
        var c = Instantiate(LevelHit, hit.point, hit.transform.rotation) as GameObject;

        rendererList.Add(c.GetComponent<MeshRenderer>());
        particleList.Add(c.GetComponent<ParticleSystem>());

        var othercol = other.GetComponent<Renderer>().material.color * 0.5f;
        c.GetComponent<Renderer>().material.color = othercol;
        c.GetComponent<ParticleSystemRenderer>().material.color = othercol;

        return rendererList.Count - 1;
    }
}