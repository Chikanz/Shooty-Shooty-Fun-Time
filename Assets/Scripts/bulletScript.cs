using System.Collections;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        Destroy(gameObject, 5);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LevelHit")
        {
            other.GetComponent<MeshRenderer>().enabled = true;
            other.GetComponent<ParticleSystem>().Play();
        }
        Destroy(gameObject);
    }
}