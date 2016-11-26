using System;
using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public int id;
    public Transform hitPos; //Impact hit

    private BulletManager BM;
    private NetworkMan NM;
    private ShootyShooty SS;
    public bool impact = true;
    bool isRocket = false;

    public GameObject smokeParent;

    private float detectDistance;

    // Use this for initialization
    private void Start()
    {
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        SS = NM.SS;

        if (NM.explosions)
        {
            isRocket = true;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //MATH MAGIC
        //http://forum.unity3d.com/threads/how-do-i-detect-if-an-object-is-in-front-of-another-object.53188/

        Vector3 heading = transform.position - hitPos.position;
        float dot = Vector3.Dot(heading, hitPos.forward);
        //
        if (dot < 0)
        {
            if (impact)
                BM.CreateNextHit(id);
            Die();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        return;

        if (other.tag == "Bullet")
            return;

        if (BM == null)
            Start();

        //Special case for slow mo, don't use for anything else
        if (other.gameObject.layer == 11)
        {
            if (SS.slowMoInUse)
                other.transform.GetComponent<Rigidbody>().AddForce(transform.forward*SS.ShootyBallForce);
            return;
        }

        if (impact)
        {
            BM.CreateNextHit(id);
        }

        Die();
        //Destroy(gameObject);
        //Debug.Log("collided");
    }

    public void SetHitPos(Transform hp)
    {
        hitPos = hp;
    }

    public void Die()
    {
        //Destroy(gameObject);
        //return;

        if (NM == null)
            NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        if (!isRocket)
        {
            Destroy(gameObject);
        }
        else
        {
            var ps = GetComponentInChildren<ParticleSystem>();
            ps.Stop();
            Transform PE = ps.transform;
            Debug.Assert(PE != null);
            PE.parent = NM.transform;
            PE.localScale = new Vector3(1, 1, 1);
            Destroy(gameObject);
        }
    }
}