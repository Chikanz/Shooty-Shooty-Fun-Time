using System.Collections;
using UnityEngine;

public class locky : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}