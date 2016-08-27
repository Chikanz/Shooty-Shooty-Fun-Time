using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private NetworkMan NM;
    private GameObject Player;
    private FirstPersonController FPC;
    private ShootyShooty SS;

    public Image Rslider;
    public Canvas SlowmoCanvas;

    public Canvas AmmoCountCanvas;
    public Text AmmoCount;
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
            AmmoCount.text = SS.inClip.ToString();

            SlowmoCanvas.enabled = NM.slowMo;
            Rslider.fillAmount = SS.Map(0.0f, SS.slowMoMax, 0.0f, 1.0f, SS.slowMoJuice);

            BlinkCanvas.enabled = NM.blink;
            if (NM.blink)
                Displayblinks();
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
        if (!canUpdate)
        {
            canUpdate = true;
            AmmoCountCanvas.enabled = true;
            Player = NM.player;
            FPC = Player.GetComponent<FirstPersonController>();
            SS = Player.GetComponentInChildren<ShootyShooty>();
        }
    }
}