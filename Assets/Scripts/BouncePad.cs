using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;

public class BouncePad : MonoBehaviour
{
    private bool triggered = false;
    private float lerpPos = 0;
    private Vector3 startpos;
    private Vector3 endPos;

    public float lerpSpeed;
    public float distance;

    // Use this for initialization
    private void Start()
    {
        startpos = transform.position;
        endPos = startpos;
        endPos.y += distance;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}