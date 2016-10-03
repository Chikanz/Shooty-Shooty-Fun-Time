using System;
using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public int id;
    public Transform hitPos;

    private BulletManager BM;
    private NetworkMan NM;
    private ShootyShooty SS;

    private float detectDistance;

    // Use this for initialization
    private void Start()
    {
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        SS = NM.SS;
        Destroy(gameObject, 5);
    }

    // Update is called once per frame
    private void Update()
    {
        if (transform == null) return;

        //MATH MAGIC
        //http://forum.unity3d.com/threads/how-do-i-detect-if-an-object-is-in-front-of-another-object.53188/
        Vector3 heading = transform.position - hitPos.position;
        float dot = Vector3.Dot(heading, hitPos.forward);
        //
        if (dot < 0) //If behind
        {
            BM.CreateNextHit(id);
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            if (SS.slowMoInUse)
                other.transform.GetComponent<Rigidbody>().AddForce(transform.forward * SS.ShootyBallForce);
            return;
            //other.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * SS.ShootyBallForce, other.contacts[0].point);
        }

        BM.CreateNextHit(id);
        Destroy(gameObject);
    }

    public void SetHitPos(Transform hp)
    {
        hitPos = hp;
    }
}