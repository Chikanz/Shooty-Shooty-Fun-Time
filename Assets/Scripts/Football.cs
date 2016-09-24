using System.Collections;
using UnityEngine;

public class Football : MonoBehaviour
{
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

    private void OnTriggerEnter(Collider other)
    {
        if (!NM.roundEnded)
        {
            if (other.transform.name == "Red Goal")
            {
                NM.GMRoundEnd(false);
            }

            if (other.transform.name == "Blue Goal")
            {
                NM.GMRoundEnd(true);
            }
        }
    }
}