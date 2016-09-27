using System.Collections;
using UnityEngine;

public class Impact : MonoBehaviour
{
    public float radius = 5.0F;
    public float power = 10.0F;
    private NetworkMan NM;
    private Transform splodeySphere;

    private void Start()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        if (NM.explosions)
        {
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
                    rb.AddExplosionForce(power, explosionPos, radius, 0);
            }
            splodeySphere = transform.GetChild(0);
            splodeySphere.gameObject.SetActive(true);
            splodeySphere.localScale = new Vector3(radius, radius, radius);
        }
        else
        {
            GetComponent<ParticleSystem>().Play();
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}