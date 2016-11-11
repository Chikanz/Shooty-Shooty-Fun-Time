using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StopSignMan : MonoBehaviour {

    public GameObject[] signs;
    private List<Vector3> startPos = new List<Vector3>();
    private List<Quaternion> startRot = new List<Quaternion>();
    List<int> list = new List<int>();

    void Start ()
    {
        for(int i = 0; i < signs.Length; i++)
        {
            startPos.Add(signs[i].transform.position);
            startRot.Add(signs[i].transform.rotation);
        }

        NetworkMan.RestartEvent += reset;
	}
		
	void Update ()
    {

	}

    public void reset()
    {
        for (int i = 0; i < signs.Length; i++)
        {
            signs[i].gameObject.SetActive(true);

            signs[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            signs[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            signs[i].transform.position = startPos[i];
            signs[i].transform.rotation = startRot[i];
            
        }
    }
}
