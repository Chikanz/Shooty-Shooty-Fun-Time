using System.Collections;
using UnityEngine;

public class killparticle : MonoBehaviour
{
    private void Start()
    {
        transform.localScale = new Vector3(1, 1, 1);
        //Destroy(gameObject, GetComponent<ParticleSystem>().duration * 2);
    }
}