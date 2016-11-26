using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private List<GameObject> hitList = new List<GameObject>();
    private List<GameObject> PetList = new List<GameObject>();

    public int PetMax;
    public int maxImpacts = 20;

    private NetworkMan NM;

    private void Start()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        NetworkMan.RestartEvent += die;
    }

    private void Update()
    {
    }

    //Clears on level restart
    public void die()
    {
        foreach (GameObject t in hitList)
        {
            Destroy(t);
        }
        hitList.Clear();
    }

    public void CreateNextHit(int id)
    {
        if (hitList[id] != null)
            hitList[id].GetComponent<Impact>().Activate(false);
    }

    //pos and rot used to instance
    //Gameobject used to get hit colour
    public int NewHit(Vector3 pos, Quaternion rot, GameObject other)
    {
        var c = PhotonNetwork.Instantiate("LevelHit", pos, rot, 0);
        hitList.Add(c);

        c.transform.parent = NM.transform;
        //c.transform.parent = other.transform; //Make child

        if (other.GetComponent<Renderer>() != null)
        {
            c.GetComponent<Impact>().otherCol = other.GetComponent<Renderer>().material.color*0.5f;
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

    //Doggos and cates
    public void NewPet(GameObject pet)
    {
        PetList.Add(pet);
        if (PetList.Count > PetMax)
        {
            Destroy(PetList[0]);
            PetList.RemoveAt(0);
        }
    }

    public Transform GetBulletReference(int i)
    {
        return hitList[i].transform;
    }
}