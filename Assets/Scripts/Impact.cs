using System.Collections;
using UnityEngine;

public class Impact : MonoBehaviour
{
    public float radius = 5.0F;
    public float power = 10.0F;
    private NetworkMan NM;
    private Transform splodeySphere;
    public Color otherCol;

    private void Start()
    {
    }

    private void Update()
    {
    }

    [PunRPC]
    public void MakeActive()
    {
        Activate(true);
    }

    //This makes my head hurt
    public void Activate(bool rpc)
    {
        if (!rpc)
            GetComponent<PhotonView>().RPC("MakeActive", PhotonTargets.Others, null);

        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        //Splosions
        if (NM.explosions)
        {
            Destroy(gameObject, 1);
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
            foreach (Collider hit in colliders)
            {
                if (hit.transform.tag == "Player")
                {
                    hit.GetComponentInParent<FirstPersonController>().Explode();
                }

                Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(power, explosionPos, radius, 1);
            }
            splodeySphere = transform.GetChild(0);
            splodeySphere.gameObject.SetActive(true);
            splodeySphere.localScale = new Vector3(radius, radius, radius);
        }
        else
        {
            GetComponent<Renderer>().material.color = otherCol;
            GetComponent<ParticleSystemRenderer>().material.color = otherCol;

            GetComponent<ParticleSystem>().Play();
            if (!NM.godBullets)
                GetComponent<MeshRenderer>().enabled = true;
        }
    }
}