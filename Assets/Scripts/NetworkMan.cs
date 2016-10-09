using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CinematicEffects;
using Random = UnityEngine.Random;

public class NetworkMan : Photon.MonoBehaviour
{
    public Text WinText;
    public Text connectionText;

    [SerializeField]
    private Transform[] SpawnPoints;

    public GameObject[] Plants;

    private Transform spawn;
    private Transform OGSpawn;

    public Camera lobbyCam;

    public GameObject RaceFallBounds;

    public Slider sensSlider;
    public Slider fovSlider;

    public GameObject player;

    public int playerNumber;

    public int Score;

    //public Text ScoreText;

    public Animator CHAnim;
    public Canvas CHCanvas;

    public GameObject innerStuff;
    public GameObject shootyBallStuff;

    public GameObject ShootyBall;

    public Text ThingExplainer;
    public GameObject godModeStuff;
    public List<GameObject> everything;
    //private List<string> DestroyedObjs = new List<string>();

    public Canvas chat;
    public Text chatBox;
    public InputField chatMessage;
    private Queue messages = new Queue();
    private const int messageLimit = 10;

    public Color P1Col = new Color();
    public Color P2Col = new Color();

    public bool lowGrav = false;

    public string oldChatText = "";
    private bool ChatEnabled;

    public PhotonView pv;

    private FirstPersonController FPC;
    public ShootyShooty SS;
    private Initalize init;

    public InputField username;
    public Canvas usernamecanvas;

    private const string alphabet = "abcdefghijklmnopqrstuvwxyz";
    private const string keyboard = "qwertyuiopasdfghjklzxcvbnm";
    private int[] backwards = { 9, 18, 25 };
    private int[] forwards = { 0, 10, 19 };
    private float randomConsecMod;

    public Text FXList;
    private string FXString = "";
    private Text FXCountdown;
    private float newRoundTimer;
    public float endRoundTimer;
    private bool playerLocked;

    public bool roundEnded;
    //public bool shotcaller;

    public delegate void RoundEvent();

    public static RoundEvent RestartEvent;

    public float deagleInnac = 0.2f;
    private float normalInnac;
    private float normalInnacDecay;

    public int SBMaxForce = 2000;
    public int SBNormForce = 500;

    public int MaxStuffGunForce = 10000;

    private readonly char[] trimings = { '+', ' ' };

    public bool MDeath = true; //Move death - moves to spawn, doesn't restart

    private bool slowMoP1;
    private bool slowMoP2;
    private readonly float slowMoMulti = 0.4f;

    //FX
    private string[] FXText =
        {
            "The Moon",
            "Jump Gun",
            "God Bullets",
            "Shotgun",
            "Blink",
            "Pet cannon",
            "Slow Mo",
            "Mum's Spaghetti",
            "Rockets"
        };

    private string[] FXDesc =
    {
            "Low Gravity",
            "Only accurate while jumping",
            "Destroy everything",
            "Mo' bullets mo' money",
            "Press 'E' to tracer blink",
            "Deadly Doggos",
            "Press 'Shift' to slow mo. Effects other players",
            "YOU ONLY GOT ONE SHOT",
            "Michael Bay splosions "
        };

    private readonly string[] FXRPCs =
        {
            "LowGrav",
            "MakeJumpAcc",
            "GodBullets",
            "Shotty",
            "Blink",
            "StuffGun",
            "SlowMo",
            "MumsSpaghetti",
            "Rockets"
        };

    private readonly string[] GMText =
    {
        "Shooty Ball",
        "Race",
    };

    private readonly string[] GMDesc =
{
        "Soccer with guns",
        "Dying sends you back to a check point",
    };

    private readonly string[] GMRPCs =
   {
        "Football",
        "Race",
    };

    //FX Bools
    private const int GmCount = 2;

    public int gmSelect = -1;

    public bool lowgrav;
    public bool JumpAcc;
    public bool drunk;
    public bool godBullets;
    public bool blink;
    public bool stuffGun;
    public bool slowMo;
    public bool oneShot;
    public bool shotGun;
    public bool explosions;

    //Gamemode
    public bool GMFootBall;

    public bool GMRace;

    private void Start()
    {
        //Time.fixedDeltaTime = 0.01f;
        //PhotonNetwork.offlineMode = true;
        //PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("0.1");
        PhotonNetwork.sendRate = 60;
        PhotonNetwork.sendRateOnSerialize = 60;
        pv = GetComponent<PhotonView>();
        FXList = transform.Find("/UI Manager/Main UI/FX Text").GetComponent<Text>();
        FXCountdown = transform.Find("/UI Manager/Main UI/Round Countdown").GetComponent<Text>();

        foreach (Transform child in godModeStuff.GetComponentsInChildren<Transform>())
            everything.Add(child.gameObject);
    }

    private void Update()
    {
        connectionText.text = PhotonNetwork.connectionStateDetailed.ToString() + "   " + PhotonNetwork.GetPing();

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            if (chat.enabled)
            {
                //Close
                chatMessage.interactable = true;
                chat.enabled = false;

                FPC.allowInput = true;
                FPC.Mlook = true;
            }
            else
            {
                //Open
                FPC.allowInput = false;
                FPC.Mlook = false;

                chat.enabled = true;
                chatMessage.interactable = true;
                chatMessage.Select();
            }
        }

        //Filter backslashes
        if (chatMessage.text.Contains("\\"))
            chatMessage.text = chatMessage.text.Replace("\\", "");

        //Drunk Typos
        if (drunk && chatMessage.text.Length > oldChatText.Length && chatMessage.text.Length != 0)
        {
            string cm = chatMessage.text;
            char LL = cm[cm.Length - 1];                //Get last letter inputted

            if (keyboard.Contains(LL.ToString()) && Random.value > 0.4f)
            {
                cm = cm.Remove(cm.Length - 1);           //Get last outta here
                chatMessage.text = cm.Insert(cm.Length, GetTypo(LL).ToString());
                //randomConsecMod += 0.15f;
            }
            //else
            //randomConsecMod = 0.0f;

            oldChatText = chatMessage.text;
        }

        if (Input.GetKeyDown(KeyCode.Return) && ChatEnabled)
        {
            string toSend = chatMessage.text;
            string color;

            if (toSend == "")
                return;

            color = ColToHex(playerNumber == 0 ? P1Col : P2Col);

            toSend = toSend.Insert(0, "<color=" + color + ">");
            toSend = toSend.Insert(toSend.Length, "</color>");

            pv.RPC("SendChatMessage", PhotonTargets.All, toSend);
            chatMessage.text = "";
            chatMessage.Select();
        }

        //New Round Timer
        if (newRoundTimer > 0 && playerLocked)
        {
            newRoundTimer -= Time.deltaTime;
            FXCountdown.text = newRoundTimer.ToString("#");
        }
        else if (newRoundTimer <= 0.0f && playerLocked) //New Round Start
        {
            LockPlayer(false);
            FXString = "";
            FXList.text = "";
            FXCountdown.text = "";
        }

        //End Round Timer
        if (PhotonNetwork.inRoom)
        {
            if (roundEnded && PhotonNetwork.isMasterClient)
            {
                endRoundTimer -= Time.deltaTime;
            }
            endRoundTimer = Mathf.Clamp(endRoundTimer, 0, 10);

            if (endRoundTimer <= 0 && roundEnded && PhotonNetwork.isMasterClient)
            {
                roundEnded = false;
                pv.RPC("RestartRound", PhotonTargets.All, null);
            }
        }

        if (playerLocked)
        {
            player.transform.position = spawn.position;
        }
    }

    public void OnConnectedToMaster()
    {
        usernamecanvas.enabled = true;
    }

    private void OnJoinedRoom()
    {
        DecideSpawn();
        Spawn();
        pv.RPC("SendChatMessage", PhotonTargets.All, PhotonNetwork.player.name + " has joined");
        chat.enabled = false;
    }

    public void SetSpawn(Transform pos)
    {
        spawn = pos;
    }

    public void ResetSpawn()
    {
        spawn = OGSpawn;
    }

    private void DecideSpawn()
    {
        if (PhotonNetwork.playerList.Length == 1)
        {//First
            OGSpawn = SpawnPoints[0];
            spawn = OGSpawn;
            playerNumber = 0;
        }
        else
        {//Second
            OGSpawn = SpawnPoints[1];
            spawn = OGSpawn;
            playerNumber = 1;
        }
    }

    public void Spawn()
    {
        lobbyCam.gameObject.SetActive(false);
        player = PhotonNetwork.Instantiate("Player", spawn.position, spawn.rotation, 0);
        SS = player.GetComponentInChildren<ShootyShooty>();
        FPC = player.GetComponentInChildren<FirstPersonController>();
        init = player.GetComponentInChildren<Initalize>();
        normalInnac = SS.maxInnac;
        normalInnacDecay = SS.spamInnac;
        FXText[5] = playerNumber == 0 ? "Doggo Launcher" : "Kitty Cannon";

        if (RestartEvent != null)
            RestartEvent();
    }

    public void MoveToSpawn()
    {
        //player.GetComponent<Initalize>().FPcam.rotation = spawn.rotation;
        player.transform.position = spawn.position;
        player.transform.rotation = spawn.rotation;
        player.GetComponent<FirstPersonController>().ResetCamDirection();
    }

    //Modifier Switches
    [PunRPC]
    public void LowGrav(bool lg)
    {
        lowGrav = lg;
        float gravity = -9.8f;
        if (lg)
        {
            if (gmSelect == 0)
                gravity = -5f;
            else
                gravity = -3f;
        }

        Physics.gravity = new Vector3(0, gravity, 0);
    }

    [PunRPC]
    public void AAAMDRUUUUNK(bool d)
    {
        CHAnim.SetBool("Drunk", d);
        drunk = d;
        if (d)
        {
            SS.StartCoroutine("drunkInnaccuracy");
        }
        else
        {
            SS.drunkinnac = false;
            SS.StopCoroutine("drunkInnaccuracy");
        }
    }

    [PunRPC]
    public void MakeJumpAcc(bool j)
    {
        JumpAcc = j;
    }

    [PunRPC]
    public void GodBullets(bool b)
    {
        godBullets = b;
    }

    [PunRPC]
    public void Deagle(bool b)
    {
        if (b)
        {
            SS.maxInnac = deagleInnac;
            SS.spamInnac = 99;
            SS.bulletDamage = 2;
        }
        else
        {
            SS.maxInnac = normalInnac;
            SS.spamInnac = normalInnacDecay;
            SS.bulletDamage = 1;
        }
    }

    [PunRPC]
    public void NoScope(bool b)
    {
        CHCanvas.enabled = !b;
    }

    [PunRPC]
    public void MumsSpaghetti(bool b)
    {
        oneShot = b;
        SS.maxClip = b ? 1 : 10;
        SS.inClip = b ? 1 : 10;
        SS.bulletDamage = b ? 4 : 1;
        SS.ShootyBallForce = b ? SBMaxForce : SBNormForce;
        SS.StuffGunForce = b ? MaxStuffGunForce : 1000;
        SS.shootCoolDown = b ? 0 : 0.15f;
    }

    [PunRPC]
    public void Blink(bool b)
    {
        player.GetComponent<FirstPersonController>().blinks = 0;
        blink = b;
        //Ability = b ? Abilities.Blink : Abilities.Sprint;
    }

    [PunRPC]
    public void StuffGun(bool b)
    {
        stuffGun = b;
        PhotonNetwork.sendRate = b ? 30 : 60;
        PhotonNetwork.sendRateOnSerialize = b ? 30 : 60;
    }

    [PunRPC]
    public void SlowMo(bool b)
    {
        slowMo = b;
        player.GetComponentInChildren<ShootyShooty>().slowMoJuice = 0;
    }

    [PunRPC]
    public void Shotty(bool b)
    {
        shotGun = b;
        SS.maxClip = b ? 2 : 10;
        SS.maxClip = SS.inClip;
        SS.maxInnac = GMRace ? 0.05f : 0.13f; //If race allow long range shotty
        SS.shootCoolDown = b ? 0.5f : 0.15f;
    }

    [PunRPC]
    public void Rockets(bool b)
    {
        explosions = b;
        SS.bulletSpeed = b ? 5000 : 10000;
    }

    //Detailed modifier RPCs

    [PunRPC]
    public void SendChatMessage(string text)
    {
        messages.Enqueue(text);
        if (messages.Count > messageLimit)
            messages.Dequeue();

        chatBox.text = "";

        foreach (string m in messages)
        {
            chatBox.text += m + "\n";
        }

        chatBox.text = chatBox.text.TrimEnd('\n');

        if (!ChatEnabled)
        {
            chat.enabled = true;
        }
    }

    [PunRPC]
    public void RestartRound()
    {
        //if (ShootyBall.gameObject != null)
        //    PhotonNetwork.Destroy(ShootyBall);

        if (PhotonNetwork.isMasterClient)
        {
            ResetFX();
            ChooseFX();
            pv.RPC("FinishedCallingShots", PhotonTargets.Others, FXString, ThingExplainer.text);
        }

        roundEnded = false;
        init.SetKill(false);
        WinText.text = "";
        player.GetComponent<Initalize>().health = 4;
        player.GetComponentInChildren<ShootyShooty>().inClip = 10;
        player.GetComponentInChildren<ShootyShooty>().reloading = false;
        player.GetComponentInChildren<ShootyShooty>().outtaBullets = false;
        player.GetComponentInChildren<ShootyShooty>().anim.SetTrigger("ForceIdle");

        MoveToSpawn();

        LockPlayer(true);

        newRoundTimer = 3;

        FXList.text = FXString;

        if (RestartEvent != null)
            RestartEvent();
    }

    [PunRPC]
    public void P1Up()
    {
        roundEnded = true;
        endRoundTimer = 5;
        WinText.text = "<color=" + ColToHex(P1Col) + ">Red Wins!</color>";
        Score -= 1;
    }

    [PunRPC]
    public void P2Up()
    {
        roundEnded = true;
        endRoundTimer = 5;
        WinText.text = "<color=" + ColToHex(P2Col) + ">Blue Wins!</color>";
        Score += 1;
    }

    [PunRPC]
    public void P1SlowMoSet(bool on)
    {
        slowMoP1 = on;

        if (slowMoP1 || slowMoP2)
        {
            SetTimeScale(slowMoMulti);
        }
        else if (!slowMoP1 && !slowMoP2)
        {
            SetTimeScale(1);
        }
    }

    [PunRPC]
    public void P2SlowMoSet(bool on)
    {
        slowMoP2 = on;

        if (slowMoP1 || slowMoP2)
        {
            SetTimeScale(slowMoMulti);
        }
        else if (!slowMoP1 && !slowMoP2)
        {
            SetTimeScale(1);
        }
    }

    private void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
    }

    [PunRPC]
    public void KillLevel(int i)
    {
        everything[i].SetActive(false);
    }

    [PunRPC]
    public void RestoreLevel()
    {
        foreach (GameObject obj in everything)
        {
            try
            {
                obj.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.Log(obj);
            }
        }
    }

    //Game mode RPCs
    [PunRPC]
    public void Football(bool b)
    {
        GMFootBall = b;
        innerStuff.gameObject.SetActive(!b);
        shootyBallStuff.gameObject.SetActive(b);
        MDeath = b;
        player.GetComponentInChildren<FirstPersonController>().blinkSpeed = b ? 2 : 4; //Half blink speed when true
        SS.StuffGunForce = b ? MaxStuffGunForce : 1000;
        SS.shootCoolDown = b ? 0 : 0.15f;

        if (!PhotonNetwork.isMasterClient) return; //Master client only

        if (b)
            ShootyBall = PhotonNetwork.Instantiate("Shooty Ball", SpawnPoints[4].position, Quaternion.identity, 0);
        else
            PhotonNetwork.Destroy(ShootyBall);
    }

    [PunRPC]
    public void Race(bool b)
    {
        GMRace = b;
        if (b)
        {
            RaceSpawnReset();
            MoveToSpawn();
        }
        else
        {
            ResetSpawn();
            MoveToSpawn();
        }

        player.GetComponent<Initalize>().maxHealth = b ? 2 : 4;
        player.GetComponent<Initalize>().health = b ? 2 : 4;

        Plants[0].SetActive(b);
        Plants[1].SetActive(b);
        FPC.blinkDistance = b ? 0.7f : 2;
        FPC.maxBlinks = b ? 1 : 3;
        SS.StuffGunForce = b ? MaxStuffGunForce : 1000;
        MDeath = b;
        RaceFallBounds.SetActive(b);
    }

    //Other RPCs
    [PunRPC]
    public void FinishedCallingShots(string f, string thing)
    {
        FXString = f;
        ThingExplainer.text = thing;
        FXList.text = FXString;
    }

    public void connectToRoom()
    {
        RoomOptions ro = new RoomOptions() { IsVisible = true, MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom("ayy Lmao", ro, TypedLobby.Default);
        PhotonNetwork.player.name = username.text;
        usernamecanvas.enabled = false;
    }

    private void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        pv.RPC("SendChatMessage", PhotonTargets.All, otherPlayer.name + " has quit");
    }

    //THE DECIDER
    private void ChooseFX()
    {
        ThingExplainer.text = "";
        FXString = "";

        int numFX;
        float v = Random.value;

        if (v < 0.1f)
        {
            numFX = 0;
            FXString = "Just Normal\n";
        }
        if (v < 0.5)
            numFX = 1;
        else if (v < 0.7)
            numFX = 2;
        else
            numFX = 3;

        ThingExplainer.text += "<size=18>" + "Combat" + "</size> \n";
        ThingExplainer.text += "Shoot your friends in the face!\n\n";

        if (Random.value > 0.3f)
        {
            gmSelect = Random.Range(0, GmCount);
            SetGM(gmSelect, true);
            FXString += GMText[gmSelect] + "\n";
            ThingExplainer.text = "<size=18>" + GMText[gmSelect] + "</size> \n";
            ThingExplainer.text += GMDesc[gmSelect] + "\n\n";
        }

        //Exclusions
        int exclusions = 0;
        List<int> pastIndexes = new List<int>();
        if (gmSelect == 0)
            pastIndexes.Add(2); //Exclude godbullets

        if (gmSelect == 1)
        {
            pastIndexes.Add(2); //Exclude godbullets
            pastIndexes.Add(3); //Exclude OneShot
        }

        //Disable rockets for now
        pastIndexes.Add(8);
        exclusions = pastIndexes.Count;

        while (pastIndexes.Count - exclusions <= numFX - 1)
        {
            int FXindex = Random.Range(0, FXText.Count());
            if (pastIndexes.Contains(FXindex))
                continue;

            pastIndexes.Add(FXindex);
            flipFX(FXindex, true);
            FXString += FXText[FXindex] + " + ";

            ThingExplainer.text += "<size=18>" + FXText[FXindex] + "</size> \n";
            ThingExplainer.text += FXDesc[FXindex] + "\n";
        }
        //pastIndexes.Clear();
        FXString = FXString.TrimEnd(trimings);
    }

    private void flipFX(int i, bool flip)
    //I find it hilarious how this wouldn't work in any other situation
    {
        pv.RPC(FXRPCs[i], PhotonTargets.All, flip);
    }

    private void ResetFX()
    {
        if (gmSelect != -1)
        {
            SetGM(gmSelect, false);
            gmSelect = -1;
        }

        if (godBullets)
            pv.RPC("RestoreLevel", PhotonTargets.All, null);

        for (int i = 0; i < FXText.Count(); i++)
        {
            flipFX(i, false);
        }
    }

    private void SetGM(int i, bool b)
    {
        pv.RPC(GMRPCs[i], PhotonTargets.All, b);
    }

    public void LockPlayer(bool l)
    {
        playerLocked = l;
        player.GetComponent<FirstPersonController>().allowInput = !l;
        player.GetComponentInChildren<ShootyShooty>().shootingEnabled = !l;
    }

    public void GMRoundEnd(bool p1Won)
    {
        if (p1Won)
            pv.RPC("P1Up", PhotonTargets.All, null);
        else
            pv.RPC("P2Up", PhotonTargets.All, null);

        MDeath = false;
    }

    //UI functions

    public void RaceSpawnReset()
    {
        spawn = PhotonNetwork.isMasterClient ? SpawnPoints[5] : SpawnPoints[6];
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene("scene1");
    }

    public void ToggeAO(bool b)
    {
        player.GetComponentInChildren<AmbientOcclusion>().enabled = b;
    }

    public void sensitivity()
    {
        float s = sensSlider.value;
        player.GetComponent<FirstPersonController>().changeSensitivity(s, s);
    }

    public void Getouttahere()
    {
        Application.Quit();
    }

    public void kys()
    {
        player.GetComponent<Initalize>().Die();
    }

    public int getPlayerHealth()
    {
        return player.GetComponent<Initalize>().health;
    }

    public void ToggleMdeath(bool b)
    {
        MDeath = b;
    }

    public void setFOV()
    {
        player.GetComponentInChildren<Camera>().fieldOfView = fovSlider.value;
    }

    //Depricated functions
    private string InsertRandomLetter(string s)
    {
        int index = Random.Range(0, s.Length);
        int charIndex = Random.Range(0, alphabet.Length);

        return s.Insert(index, alphabet[charIndex].ToString());
    }

    private string RemoveRandomLetter(string s)
    {
        int index = Random.Range(0, s.Length);

        return s.Remove(index);
    }

    private char GetTypo(char c)
    {
        int index = keyboard.IndexOf(c);
        int mod = -1;
        if (!forwards.Contains(index) && !backwards.Contains(index)) //Normal case
        {
            float modr = Random.value;
            if (modr > 0.5)
                mod = 1;
        }
        else if (forwards.Contains(index)) //Has to go forward
            mod = 1;
        else if (backwards.Contains(index)) //Has to go backwards
            mod = -1;

        return keyboard[index + mod];
    }

    private string ColToHex(Color col)
    {
        int r = (int)Mathf.Floor(col.r * 255);
        int g = (int)Mathf.Floor(col.g * 255);
        int b = (int)Mathf.Floor(col.b * 255);
        return "#" + r.ToString("X") + g.ToString("X") + b.ToString("X");
    }
}