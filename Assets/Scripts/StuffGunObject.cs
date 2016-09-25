using System.Collections;
using UnityEngine;

public class StuffGunObject : MonoBehaviour
{
    private NetworkMan NM;
    private int destroyed = 0;
    public int stuffGunMulti = 2;
    public float TTL = 0.5f; //Time till live

    private void Start()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        NetworkMan.RestartEvent += Die;
    }

    private void Update()
    {
        if (TTL >= 0)
            TTL -= Time.deltaTime;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        NetworkMan.RestartEvent -= Die;
    }

    public void OnCollisionEnter(Collision c)
    {
        if (NM == null)
            NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        if (NM.GMFootBall)
        {
            if (c.gameObject.name == "Shooty Ball")
            {
                c.gameObject.GetComponent<Rigidbody>()
                    .AddForceAtPosition(NM.SS.ShootyBallForce * stuffGunMulti * transform.forward, c.contacts[0].point);
            }
        }

        if (c.gameObject.tag == "Player" && TTL < 0)
        {
            if (c.gameObject.GetComponent<Initalize>().photonView.isMine)
            {
                c.gameObject.GetComponent<Initalize>().Die();
                return;
            }
        }

        if (NM.godBullets)
        {
            var i = NM.everything.IndexOf(c.gameObject);
            if (i != -1)
            {
                NM.pv.RPC("KillLevel", PhotonTargets.All, i);
                destroyed += 1;
                if (destroyed >= 3)
                    Destroy(gameObject);
            }
            else
                Debug.Log("Couldn't find that item!" + c.gameObject.ToString());
        }
    }
}