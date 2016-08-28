using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public int id;
    public Vector3 hitPos;

    private BulletManager BM;
    private float detectDistance;

    // Use this for initialization
    private void Start()
    {
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
        Destroy(gameObject, 5);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Vector3.Distance(transform.position, hitPos) <= detectDistance)
        {
            //if (BM == null)
            //    BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();

            BM.CreateNextHit(id);
            Destroy(gameObject);
        }

        detectDistance = Time.timeScale == 1 ? 2 : 0.4f; //enable more accuracy during slowmo for more believeable impacts
    }

    public void OnTriggerEnter(Collider other)
    {
    }
}