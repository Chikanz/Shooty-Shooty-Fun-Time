using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private List<GameObject> hitList = new List<GameObject>();
    private List<GameObject> PetList = new List<GameObject>();

    public int PetMax;
    public int maxImpacts = 2;

    public GameObject LevelHit;
    private NetworkMan NM;

    // Use this for initialization
    private void Start()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void CreateNextHit(int id)
    {
        if (hitList[id] != null)
            hitList[id].GetComponent<Impact>().Activate(false);
    }

    public int NewHit(RaycastHit hit, GameObject other)
    {
        var c = PhotonNetwork.Instantiate("LevelHit", hit.point, hit.transform.rotation, 0);
        hitList.Add(c);

        c.transform.parent = NM.transform; //Make child

        if (other.GetComponent<Renderer>() != null)
        {
            c.GetComponent<Impact>().otherCol = other.GetComponent<Renderer>().material.color * 0.5f;
        }

        c.transform.name = "Impact id" + Random.Range(0, 999);

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