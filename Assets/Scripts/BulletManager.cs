using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    //private List<MeshRenderer> rendererList = new List<MeshRenderer>();
    //private List<ParticleSystem> particleList = new List<ParticleSystem>();
    private List<GameObject> hitList = new List<GameObject>();

    private List<GameObject> PetList = new List<GameObject>();

    public int PetMax;
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
        hitList[id].gameObject.SetActive(true);
    }

    public int NewHit(RaycastHit hit, GameObject other)
    {
        var c = Instantiate(LevelHit, hit.point, hit.transform.rotation) as GameObject;

        hitList.Add(c);

        if (other.GetComponent<Renderer>())
        {
            var othercol = other.GetComponent<Renderer>().material.color * 0.5f;
            c.GetComponent<Renderer>().material.color = othercol;
            c.GetComponent<ParticleSystemRenderer>().material.color = othercol;
        }

        //if (rendererList.Count > maxImpacts) //Cap
        //{
        //    Destroy(rendererList[0]);
        //    rendererList.RemoveAt(0);
        //    particleList.RemoveAt(0);
        //}

        return hitList.Count - 1;
    }

    public void NewPet(GameObject pet)
    {
        PetList.Add(pet);
        if (PetList.Count > PetMax)
        {
            Destroy(PetList[0]);
            PetList.RemoveAt(0);
        }
    }
}