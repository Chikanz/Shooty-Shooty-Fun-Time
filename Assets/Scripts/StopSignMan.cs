using UnityEngine;
using System.Collections;

public class StopSignMan : MonoBehaviour {

    public GameObject[] signs;
    public Transform[] startPos;

	void Start ()
    {
        for(int i = 0; i < signs.GetLength(1); i++)
        {
            startPos[i] = signs[i].transform;
        }

        NetworkMan.RestartEvent += reset;
	}
		
	void Update ()
    {

	}

    public void reset()
    {
        for (int i = 0; i < signs.GetLength(1); i++)
        {
            signs[i].transform.position = startPos[i].position;
            signs[i].transform.rotation = startPos[i].rotation;
        }
    }
}
