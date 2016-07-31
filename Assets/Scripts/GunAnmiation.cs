using System.Collections;
using UnityEngine;

public class GunAnmiation : MonoBehaviour
{
    public GameObject player;

    public Animator anim;

    private void Start()
    {
    }

    private void ReloadTrigger()
    {
        player.GetComponentInChildren<ShootyShooty>().reloading = false;
        player.GetComponentInChildren<ShootyShooty>().outtaBullets = false;
    }

    private void repeatRun()
    {
        anim.Play("runrunrun", -1, 0.24f);
    }

    private void coolStart()
    {
        player.GetComponentInChildren<ShootyShooty>().coolDown = true;
    }

    private void coolEnd()
    {
        player.GetComponentInChildren<ShootyShooty>().coolDown = false;
    }
}