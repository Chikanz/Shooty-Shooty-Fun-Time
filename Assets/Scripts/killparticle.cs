using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class killparticle : MonoBehaviour
{
    private void Start()
    {
        NetworkMan.RestartEvent += Die;
        transform.localScale = new Vector3(1, 1, 1);
        Destroy(gameObject, GetComponent<ParticleSystem>().startLifetime + GetComponent<ParticleSystem>().duration + 2);
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        NetworkMan.RestartEvent -= Die;
    }
}