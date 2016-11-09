using UnityEngine;
using System.Collections;

public class gravItem : MonoBehaviour {


    Rigidbody RB;
    BoxCollider BC;
    // Use this for initialization
    void Start ()
    {
        BC = GetComponent<BoxCollider>();        
        RB = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update ()
    {

	}

    public void OnTriggerStay(Collider c)
    {
        if(c.tag == "Player")
        {
            Vector3 pPos = c.transform.position;

            // Get a direction to the player
            //(from https://docs.unity3d.com/Manual/DirectionDistanceFromOneObjectToAnother.html)
            var heading = pPos - transform.position;
            var distance = heading.magnitude;
            var direction = heading / distance; // This is now the normalized direction.

            RB.AddForce(direction * (GetComponent<SphereCollider>().radius - distance) * 1.5f);
        }
    }

    public void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.tag == "Player")
            Destroy(gameObject);
    }
}
