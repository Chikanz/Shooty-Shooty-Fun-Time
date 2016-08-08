using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private NetworkMan NM;
    private GameObject Player;
    private FirstPersonController FPC;

    public Canvas BlinkCanvas;
    public Image[] blinkImages;
    public Color blinkActiveColor;
    public Color blinkInactiveColor;

    private bool canUpdate;

    private void Start()
    {
        NetworkMan.RestartEvent += Init;
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
    }

    private void Update()
    {
        if (canUpdate)
        {
            if (NM.blink)
            {
                BlinkCanvas.enabled = true;
                Displayblinks();
            }
            else
            {
                BlinkCanvas.enabled = false;
            }
        }
    }

    private void Displayblinks()
    {
        //Wipe
        foreach (Image i in blinkImages)
        {
            i.color = blinkInactiveColor;
        }

        if (FPC.blinks > 0)
        {
            for (int i = 0; i < FPC.blinks; i++)
            {
                blinkImages[i].color = blinkActiveColor;
            }
        }
    }

    private void Init()
    {
        Player = NM.player;
        FPC = Player.GetComponent<FirstPersonController>();
        canUpdate = true;
    }
}