using System.Collections;
using UnityEngine;

public class Parkour : MonoBehaviour
{
    private FirstPersonController FPC;

    // Use this for initialization
    private void Start()
    {
        FPC = GetComponentInParent<FirstPersonController>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.transform.tag == "Level")
        {
            Debug.Log("heyyy Lmao");
        }
    }
}