using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Image hurtImage;
    public Image Rslider;
    public Canvas SlowmoCanvas;

    public Canvas AmmoCountCanvas;
    public Text AmmoCount;
    public Canvas BlinkCanvas;
    public Image[] blinkImages;
    public Color blinkActiveColor;
    public Color blinkInactiveColor;

    public GameObject EscapeMenu;
    public Image[] ScoreImages;

    public Text healthText;

    public bool excOn = false;

    private bool owies;
    private NetworkMan NM;
    private GameObject Player;
    private FirstPersonController FPC;
    private ShootyShooty SS;

    public GameObject GubsCanvas;
    public Text GubsCount;

    private bool canUpdate;

    private void Start()
    {
        NetworkMan.RestartEvent += Init;
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EscMenu();
        }

        UpdateScoreImages(NM.Score);

        if (canUpdate)
        {
            AmmoCount.text = SS.inClip.ToString();

            SlowmoCanvas.enabled = NM.slowMo;
            Rslider.fillAmount = SS.Map(0.0f, SS.slowMoMax, 0.0f, 1.0f, SS.slowMoJuice);

            BlinkCanvas.enabled = NM.blink;

            if (NM.blink)
            {
                Displayblinks();
            }

            if(NM.bFriday)
            {
                GubsCount.text = Player.GetComponent<Initalize>().GubsCount.ToString();
            }

            healthText.text = NM.getPlayerHealth() > 0 ? NM.getPlayerHealth().ToString() : "0";
        }

        if (owies)
        {
            hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, hurtImage.color.a - Time.deltaTime * 2);
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

    public void UpdateScoreImages(int score)
    {
        //Clear Score Images
        for (int i = 0; i < ScoreImages.Count(); i++)
        {
            ScoreImages[i].enabled = false;
        }

        //Translate score to array
        if (score == 0)
            return;
        if (score > 0)
            score += 3;
        if (score < 0)
            score += 4;

        //Turn on score
        for (int j = 0; j < ScoreImages.Count(); j++)
        {
            if (j > 3 && j <= score)
            {
                ScoreImages[j].enabled = true;
            }

            if (j < 4 && j >= score)
                ScoreImages[j].enabled = true;
        }
    }

    public void EscMenu()
    {
        excOn = !excOn;
        //Clean this up pls
        EscapeMenu.SetActive(excOn);
        NM.player.GetComponent<FirstPersonController>().m_MouseLook.SetCursorLock(!excOn);
        NM.player.GetComponent<FirstPersonController>().enabled = !excOn;
        NM.player.GetComponent<FirstPersonController>().m_MouseLook.m_cursorIsLocked = !excOn;
        NM.player.GetComponentInChildren<ShootyShooty>().enabled = !excOn;
    }

    public void Owch()
    {
        hurtImage.color = Color.red;
        owies = true;
    }
}