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
        if(canPunch && c.gameObject.tag == "Player" && !c.gameObject.GetComponent<PhotonView>().isMine)
        {
            c.gameObject.GetComponent<Initalize>().Punched();
            canPunch = false;
        }
        else if(c.gameObject.GetComponent<Rigidbody>())
        {
            c.gameObject.GetComponent<Rigidbody>().AddForceAtPosition
                (transform.up * 10000, c.contacts[0].point);
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
