using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    private BulletManager BM;
    public int id;

    // Use this for initialization
    private void Start()
    {
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
        Destroy(gameObject, 5);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "LevelHit")
        //{
        //    other.GetComponent<MeshRenderer>().enabled = true;
        //    other.GetComponent<ParticleSystem>().Play();
        //}

        if (BM == null)
            BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();

        if (other.tag != "Player")
        {
            BM.CreateNextHit(id);
            Destroy(gameObject);
        }
    }
}