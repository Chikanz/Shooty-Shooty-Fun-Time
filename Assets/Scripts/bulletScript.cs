using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public int id;
    public Vector3 hitPos;

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
        if (Vector3.Distance(transform.position, hitPos) <= detectDistance)
        {
            BM.CreateNextHit(id);
            Destroy(gameObject);
        }

        detectDistance = Time.timeScale == 1 ? 2 : 0.4f; //enable more accuracy during slowmo for more believeable impacts
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            if (SS.slowMoInUse)
                other.transform.GetComponent<Rigidbody>().AddForce(transform.forward * SS.ShootyBallForce);
            //other.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * SS.ShootyBallForce, other.contacts[0].point);
        }
    }
}