using System.Collections;
using UnityEngine;

public class LightHandler : MonoBehaviour
{
    private Light l;
    private float lightTimer;
    public float MaxIntensity;
    private float lerp = 1;
    public float multi;

    private void Start()
    {
        l = GetComponent<Light>();
    }

    private void Update()
    {
        if (lerp < 1)
            lerp += Time.deltaTime * multi;

        l.intensity = Mathf.Lerp(MaxIntensity, 0, lerp);
    }

    public void LightOn()
    {
        lerp = 0;
    }
}