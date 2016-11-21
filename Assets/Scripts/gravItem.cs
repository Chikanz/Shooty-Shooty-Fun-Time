using UnityEngine;
using System.Collections;

public class gravItem : MonoBehaviour {
    Rigidbody RB;
    SphereCollider SC;

    bool inGrav = false;
    bool collided = false;
    float TTL = 1;

    void Start ()
    {
        RB = GetComponent<Rigidbody>();
        SC = GetComponentInChildren<SphereCollider>();
    }
	
	void Update ()
    {
        if (TTL > 0)
            TTL -= Time.deltaTime;
	}

    [PunRPC]
    void enable(bool b)
    {
        if (b)
            collided = false;

        gameObject.SetActive(b);
    }

    public void NetworkEnable(bool enabled)
    {
        GetComponent<PhotonView>().RPC("enable", PhotonTargets.All, enabled);
    }

    //All gub physics is handled on master client, hence ismine
    //Possible disadvantage is physics is based on where the master thinks the client is, not where it actually is
    //p2p sucks
    public void OnTriggerStay(Collider c)
    {
        if(TTL < 0 && 
            c.tag == "Player" || c.tag == "Shopper" &&
            GetComponent<PhotonView>().isMine)
        {
            Vector3 pPos = c.transform.position;

            // Get a direction to the player
            //(from https://docs.unity3d.com/Manual/DirectionDistanceFromOneObjectToAnother.html)
            var heading = pPos - transform.position;
            var distance = heading.magnitude;
            var direction = heading / distance; // This is now the normalized direction.

            float forceMulti = (SC.radius - distance) * 1.5f;

            if(forceMulti > 0)                          //Make sure force doesn't go backwards
                RB.AddForce(direction * forceMulti);    //if collider scale is less than real world, shit fucks up
        }
    }

    public void OnCollisionEnter(Collision c)
    {
        if (TTL < 0 && GetComponent<PhotonView>().isMine && !collided)
        {
            if (c.gameObject.tag == "Player")
            {
                c.gameObject.GetComponent<Initalize>().getGubsHandler();
                collided = true;
                NetworkEnable(false);
            }
            else if(c.gameObject.tag == "Shopper")
            {
                c.gameObject.GetComponent<Shopper>().gubsCount += 1;
                collided = true;
                NetworkEnable(false);
            }
        }
    }
}
