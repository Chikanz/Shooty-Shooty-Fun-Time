using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StopSignMan : MonoBehaviour {

    GameObject Barriers;

    private List<GameObject> gubs = new List<GameObject>();
    private List<Vector3> startPos = new List<Vector3>();
    private List<Quaternion> startRot = new List<Quaternion>();
    List<int> list = new List<int>();
    private NetworkMan NM;

    void Start ()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.GetComponent<Rigidbody>())
            {
                gubs.Add(child.gameObject);
                startPos.Add(child.transform.position);
                startRot.Add(child.transform.rotation);
            }
        }

        NetworkMan.RestartEvent += reset;
	}
		
	void Update ()
    {

	}

    public void reset()
    {
        
            for (int i = 0; i < gubs.Count; i++)
        {
            //Turn on if not gubs or if black friday
            //Turn on any other object if not (eg stop signs)
            if (gubs[i].tag != "Gubs" ||
                PhotonNetwork.isMasterClient && gubs[i].tag == "Gubs" && NM.bFriday)
            {
                if (gubs[i].GetComponent<gravItem>())
                    gubs[i].GetComponent<gravItem>().NetworkEnable(true);
                else
                    gubs[i].gameObject.SetActive(true);
            }

            gubs[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            gubs[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            gubs[i].transform.position = startPos[i];
            gubs[i].transform.rotation = startRot[i];
        }
        
    }
}
