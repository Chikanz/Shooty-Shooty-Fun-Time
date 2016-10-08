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
    public bool impact = true;

    private float detectDistance;

    // Use this for initialization
    private void Start()
    {
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        SS = NM.SS;
    }

    // Update is called once per frame
    private void Update()
    {
        //MATH MAGIC
        //http://forum.unity3d.com/threads/how-do-i-detect-if-an-object-is-in-front-of-another-object.53188/
        if (impact)
        {
            Vector3 heading = transform.position - hitPos.position;
            float dot = Vector3.Dot(heading, hitPos.forward);
            //
            if (dot < 0) //If behind
            {
                //Debug.Log("dotted");
                BM.CreateNextHit(id);
                Destroy(gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bullet")
            return;

        if (BM == null)
            Start();

        //Special case for slow mo, don't use for anything else
        if (other.gameObject.layer == 11)
        {
            if (SS.slowMoInUse)
                other.transform.GetComponent<Rigidbody>().AddForce(transform.forward * SS.ShootyBallForce);
            return;
        }

        if (impact)
        {
            BM.CreateNextHit(id);
        }

        Destroy(gameObject);
        //Debug.Log("collided");
    }

    public void SetHitPos(Transform hp)
    {
        hitPos = hp;
    }
}