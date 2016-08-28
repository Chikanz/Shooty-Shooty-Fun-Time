using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private List<MeshRenderer> rendererList = new List<MeshRenderer>();
    private List<ParticleSystem> particleList = new List<ParticleSystem>();
    public int maxImpacts = 2;

    public GameObject LevelHit;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void CreateNextHit(int id)
    {
        rendererList[id].enabled = true;
        particleList[id].Play();
    }

    public int NewHit(RaycastHit hit, GameObject other)
    {
        var c = Instantiate(LevelHit, hit.point, hit.transform.rotation) as GameObject;

        rendererList.Add(c.GetComponent<MeshRenderer>());
        particleList.Add(c.GetComponent<ParticleSystem>());

        var othercol = other.GetComponent<Renderer>().material.color * 0.5f;
        c.GetComponent<Renderer>().material.color = othercol;
        c.GetComponent<ParticleSystemRenderer>().material.color = othercol;

        if (rendererList.Count > maxImpacts) //Cap
        {
            Destroy(rendererList[0]);
            rendererList.RemoveAt(0);
            particleList.RemoveAt(0);
        }

        return rendererList.Count - 1;
    }
}