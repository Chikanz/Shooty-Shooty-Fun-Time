using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;

public class NetworkMan : Photon.MonoBehaviour
{
    public Text WinText;

    public Text connectionText;

    [SerializeField]
    private Transform[] SpawnPoints;

    private Transform spawn;
    private Transform OGSpawn;

    public Camera lobbyCam;

    public GameObject RaceFallBounds;

    public GameObject player;

    public int playerNumber;

    public int Score;

    //public Text ScoreText;

    public Animator CHAnim;
    public Canvas CHCanvas;

    public GameObject innerStuff;
    public GameObject shootyBallStuff;

    private GameObject ShootyBall;

    public List<GameObject> everything;
    //private List<string> DestroyedObjs = new List<string>();

    public Canvas chat;
    public Text chatBox;
    public InputField chatMessage;
    private Queue messages = new Queue();
    private const int messageLimit = 10;
    private bool ChatEnabled = false;

    public Color P1Col = new Color();
    public Color P2Col = new Color();

    public bool lowGrav = false;

    public string oldChatText = "";

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

    private Text FXList;
    private string FXString = "";
    private Text FXCountdown;
    private float newRoundTimer;
    public float endRoundTimer;
    private bool playerLocked;

    public bool roundEnded;
    public bool shotcaller;

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

    public bool slowMoP1;
    public bool slowMoP2;

    //FX
    private readonly string[] FXText =
        {
            "The Moon",
            "Jump Gun",
            "God Bullets",
            "Mum's Spaghetti",
            "Blink",
            "Stuff Gun",
            "Slow Mo",
            "Shotgun"
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

    //Gamemode
    public bool GMFootBall;

    public bool GMRace;

    // Use this for initialization
    private void Start()
    {
        //Time.fixedDeltaTime = 0.01f;
        //PhotonNetwork.offlineMode = true;
        //PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("0.1");
        pv = GetComponent<PhotonView>();
        FXList = transform.Find("/UI Groups/Main UI/FX Text").GetComponent<Text>();
        FXCountdown = transform.Find("/UI Groups/Main UI/Round Countdown").GetComponent<Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        //ScoreText.text = Score[0] + " - " + Score[1];

        connectionText.text = PhotonNetwork.connectionStateDetailed.ToString() + "   " + PhotonNetwork.GetPing();

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            if (ChatEnabled)
            {
                chatMessage.interactable = true;
                ChatEnabled = false;
                chat.enabled = false;
                player.GetComponent<FirstPersonController>().allowInput = true;
            }
            else
            {
                player.GetComponent<FirstPersonController>().allowInput = false;
                ChatEnabled = true;
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

            if (gmSelect == 0 && shotcaller)
            {
                ShootyBall.GetComponent<Rigidbody>().useGravity = true;
            }

            shotcaller = false;
            //roundEnded = false;
        }

        //End Round Timer
        if (PhotonNetwork.inRoom)
        {
            //if (init.died)
            if (roundEnded)
                endRoundTimer -= Time.deltaTime;
            endRoundTimer = Mathf.Clamp(endRoundTimer, 0, 10);

            if (endRoundTimer <= 0 && roundEnded && shotcaller)
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

        if (shotcaller)
        {
            ResetFX();
            ChooseFX();
            pv.RPC("FinishedCallingShots", PhotonTargets.Others, FXString);
        }
        FXList.text = FXString;

        if (RestartEvent != null)
            RestartEvent();
    }

    [PunRPC]
    public void P1Up()
    {
        roundEnded = true;
        WinText.text = "<color=" + ColToHex(P1Col) + ">Red Wins!</color>";
        Score -= 1;
    }

    [PunRPC]
    public void P2Up()
    {
        roundEnded = true;
        WinText.text = "<color=" + ColToHex(P2Col) + ">Blue Wins!</color>";
        Score += 1;
    }

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
        if (stuffGun)
            SS.StuffGunForce = b ? MaxStuffGunForce : 1000;
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
    }

    //Detailed modifier RPCs
    [PunRPC]
    public void SlowMoSet(float scale)
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
            obj.SetActive(true);
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
        //SS.StuffGunForce = b ? MaxStuffGunForce : 1000;

        if (!shotcaller) return; //Shot Callers only

        if (b)
            ShootyBall = PhotonNetwork.Instantiate("Shooty Ball", SpawnPoints[4].position, Quaternion.identity, 0);
        else
            PhotonNetwork.Destroy(ShootyBall.gameObject);
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
        FPC.blinkDistance = b ? 0.7f : 2;
        //FPC.blinkDistance = 1.2f;
        SS.StuffGunForce = b ? MaxStuffGunForce : 1000;
        MDeath = b;
        RaceFallBounds.SetActive(b);
    }

    //Other RPCs
    [PunRPC]
    public void FinishedCallingShots(string f)
    {
        FXString = f;
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

    private void ChooseFX()
    {
        int numFX;
        float v = Random.value;

        if (v < 0.1f)
        {
            numFX = 0;
            FXString = "Just Normal";
        }
        else if (v < 0.5)
            numFX = 1;
        else if (v < 0.7)
            numFX = 2;
        else
            numFX = 3;

        if (Random.value > 0.0f)
        {
            gmSelect = Random.Range(1, GmCount);
            SetGameMode(gmSelect, true);
            FXString += GetGmString(gmSelect) + "\n";
        }

        List<int> pastIndexes = new List<int>();
        if (gmSelect == 0)
            pastIndexes.Add(2); //Exclude godbullets

        if (gmSelect == 1)
        {
            pastIndexes.Add(2); //Exclude godbullets
            pastIndexes.Add(3); //Exclude OneShot
        }
        while (pastIndexes.Count <= numFX)
        {
            int FXindex = Random.Range(0, FXText.Count());
            if (pastIndexes.Contains(FXindex))
                continue;

            pastIndexes.Add(FXindex);
            flipFX(FXindex, true);
            FXString += FXText[FXindex] + " + ";
        }
        pastIndexes.Clear();
        FXString = FXString.TrimEnd(trimings);
    }

    private void flipFX(int index, bool flip)
    {
        switch (index)
        {
            case 0:
                pv.RPC("LowGrav", PhotonTargets.All, flip);
                break;

            case 1:
                pv.RPC("MakeJumpAcc", PhotonTargets.All, flip);
                break;

            case 2:
                pv.RPC("GodBullets", PhotonTargets.All, flip);
                break;

            case 3:
                pv.RPC("MumsSpaghetti", PhotonTargets.All, flip);
                break;

            case 4:
                pv.RPC("Blink", PhotonTargets.All, flip);
                break;

            case 5:
                pv.RPC("StuffGun", PhotonTargets.All, flip);
                break;

            case 6:
                pv.RPC("SlowMo", PhotonTargets.All, flip);
                break;

            case 7:
                pv.RPC("Shotty", PhotonTargets.All, flip);
                break;

            default:
                Debug.Log("FXFlip Out of bounds");
                break;

                //case 3:
                //    pv.RPC("Deagle", PhotonTargets.All, flip);
                //    break;
                //case 5:
                //    pv.RPC("NoScope", PhotonTargets.All, flip);
                //    break;

                //case 2:
                //    pv.RPC("AAAMDRUUUUNK", PhotonTargets.All, flip);
                //    break;
        }
    }

    private void ResetFX()
    {
        if (gmSelect != -1)
        {
            SetGameMode(gmSelect, false);
            gmSelect = -1;
        }

        if (godBullets)
            pv.RPC("RestoreLevel", PhotonTargets.All, null);

        for (int i = 0; i < FXText.Count(); i++)
        {
            flipFX(i, false);
        }
    }

    private void SetGameMode(int i, bool b)
    {
        switch (i)
        {
            case 0:
                pv.RPC("Football", PhotonTargets.All, b);
                break;

            case 1:
                pv.RPC("Race", PhotonTargets.All, b);
                break;
        }
    }

    private string GetGmString(int i)
    {
        switch (i)
        {
            case 0:
                return "Shooty Ball";
                break;

            case 1:
                return "Race";
                break;
        }
        return "Uh oh";
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

        shotcaller = true;
        roundEnded = true;
        endRoundTimer = 5;
        MDeath = false;
    }

    public void RaceSpawnReset()
    {
        spawn = PhotonNetwork.isMasterClient ? SpawnPoints[5] : SpawnPoints[6];
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