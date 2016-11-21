using UnityEngine;
using System.Collections;

public class punch : MonoBehaviour {

    bool canPunch = true;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void OnCollisionEnter(Collision c)
    {
        if (canPunch)
        {
            //punchin players awww ye
            if (c.gameObject.tag == "Player" && !c.gameObject.GetComponent<PhotonView>().isMine)
            {
                c.gameObject.GetComponent<Initalize>().Punched();
                c.gameObject.GetComponent<PhotonView>().RPC("PlayHurt", PhotonTargets.Others, null);
            }
            //Shoppers
            if (c.gameObject.tag == "Shopper")
            {
                c.gameObject.GetComponent<PhotonView>().RPC("Punch", PhotonTargets.All, null);
            }

            //Everything else (no players allowed!)
            else if (c.gameObject.GetComponent<Rigidbody>() && c.gameObject.tag != "Player")
            {
                c.gameObject.GetComponent<Rigidbody>().AddForceAtPosition
                    (transform.up * 10000, c.contacts[0].point);
            }
            canPunch = false;
        }
    }

    private void EnablePunching()
    {
        GetComponent<BoxCollider>().enabled = true;
        canPunch = true;
    }

    private void DisablePunching()
    {
        GetComponent<BoxCollider>().enabled = false;
        canPunch = false;
    }

}
