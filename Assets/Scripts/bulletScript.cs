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

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}