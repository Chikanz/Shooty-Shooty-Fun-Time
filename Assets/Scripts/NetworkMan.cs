using System;
using System.Collections;
using System.Collections.Generic;
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

    public Transform spawn;

    private GameObject player;

    public int playerNumber;

    public Vector2 Score = new Vector2(0, 0);

    public Text ScoreText;

    public Animator CHAnim;
    public Canvas CHCanvas;

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
    private ShootyShooty ss;
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

    private readonly char[] trimings = { '+', ' ' };

    //FX
    private const int FxCount = 7;

    public bool lowgrav;
    public bool JumpAcc;
    public bool drunk;
    public bool godBullets;
    public bool blink = true;

    // Use this for initialization
    private void Start()
    {
        //PhotonNetwork.offlineMode = true;
        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("0.1");
        pv = GetComponent<PhotonView>();
        FXList = transform.Find("/UI Groups/Main UI/FX Text").GetComponent<Text>();
        FXCountdown = transform.Find("/UI Groups/Main UI/Round Countdown").GetComponent<Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        ScoreText.text = Score[0] + " - " + Score[1];

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

            if (playerNumber == 0)
                color = ColToHex(P1Col);
            else
                color = ColToHex(P2Col);

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
        else if (newRoundTimer <= 0.0f && playerLocked)
        {
            LockPlayer(false);
            FXString = "";
            FXList.text = "";
            FXCountdown.text = "";
            shotcaller = false;
            roundEnded = false;
        }

        //End Round Timer
        if (PhotonNetwork.inRoom)
        {
            if (init.died)
                endRoundTimer -= Time.deltaTime;
            endRoundTimer = Mathf.Clamp(endRoundTimer, 0, 10);

            if (endRoundTimer <= 0 && init.died && shotcaller)
            {
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
        SetSpawn();
        Spawn();
        pv.RPC("SendChatMessage", PhotonTargets.All, PhotonNetwork.player.name + " has joined");
        chat.enabled = false;
    }

    private void SetSpawn()
    {
        if (PhotonNetwork.playerList.Length == 1)
        {//First
            spawn = SpawnPoints[0];
            playerNumber = 0;
        }
        else
        {//Second
            spawn = SpawnPoints[1];
            playerNumber = 1;
        }
    }

    public void Spawn()
    {
        player = PhotonNetwork.Instantiate("Player", spawn.position, spawn.rotation, 0);
        ss = player.GetComponentInChildren<ShootyShooty>();
        init = player.GetComponentInChildren<Initalize>();
        normalInnac = ss.maxInnac;
        normalInnacDecay = ss.spamInnac;
    }

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

        player.GetComponent<Initalize>().FPcam.rotation = spawn.rotation;
        player.transform.position = spawn.position;
        player.transform.rotation = spawn.rotation;

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
        Score[0] += 1;
    }

    [PunRPC]
    public void P2Up()
    {
        roundEnded = true;
        WinText.text = "<color=" + ColToHex(P2Col) + ">Blue Wins!</color>";
        Score[1] += 1;
    }

    [PunRPC]
    public void LowGrav(bool lg)
    {
        lowgrav = lg;
        float gravity = -9.8f;
        if (lg)
            gravity = -3f;

        Physics.gravity = new Vector3(0, gravity, 0);
    }

    [PunRPC]
    public void AAAMDRUUUUNK(bool d)
    {
        CHAnim.SetBool("Drunk", d);
        drunk = d;
        if (d)
        {
            ss.StartCoroutine("drunkInnaccuracy");
        }
        else
        {
            ss.drunkinnac = false;
            ss.StopCoroutine("drunkInnaccuracy");
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
            ss.maxInnac = deagleInnac;
            ss.spamInnac = 99;
            ss.bulletDamage = 2;
        }
        else
        {
            ss.maxInnac = normalInnac;
            ss.spamInnac = normalInnacDecay;
            ss.bulletDamage = 1;
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
        ss.maxClip = b ? 1 : 10;
        ss.inClip = b ? 1 : 10;
        ss.bulletDamage = b ? 4 : 1;
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

    [PunRPC]
    public void FinishedCallingShots(string f)
    {
        FXString = f;
        FXList.text = FXString;
    }

    private string ColToHex(Color col)
    {
        int r = (int)Mathf.Floor(col.r * 255);
        int g = (int)Mathf.Floor(col.g * 255);
        int b = (int)Mathf.Floor(col.b * 255);
        return "#" + r.ToString("X") + g.ToString("X") + b.ToString("X");
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

        List<int> pastIndexes = new List<int>();

        while (pastIndexes.Count != numFX)
        {
            int FXindex = Random.Range(0, FxCount);
            if (pastIndexes.Contains(FXindex))
                continue;

            pastIndexes.Add(FXindex);
            flipFX(FXindex, true);
            FXString += GetFXText(FXindex) + " + ";
        }

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
                pv.RPC("AAAMDRUUUUNK", PhotonTargets.All, flip);
                break;

            case 3:
                pv.RPC("GodBullets", PhotonTargets.All, flip);
                break;

            case 4:
                pv.RPC("Deagle", PhotonTargets.All, flip);
                break;

            case 5:
                pv.RPC("NoScope", PhotonTargets.All, flip);
                break;

            case 6:
                pv.RPC("MumsSpaghetti", PhotonTargets.All, flip);
                break;

            default:
                Debug.Log("FXFlip Out of bounds");
                break;
        }
    }

    private void ResetFX()
    {
        if (godBullets)
            pv.RPC("RestoreLevel", PhotonTargets.All, null);

        for (int i = 0; i < FxCount; i++)
        {
            flipFX(i, false);
        }
    }

    private string GetFXText(int index)
    {
        switch (index)
        {
            case 0:
                return "The Moon";
                break;

            case 1:
                return "Jump gun"; //Weird gun?
                break;

            case 2:
                return "Alcohol";
                break;

            case 3:
                return "God Bullets";
                break;

            case 4:
                return "Deagle";
                break;

            case 5:
                return "No Scope";
                break;

            case 6:
                return "Mum's Spaghetti";
                break;

            default:
                Debug.Log("FXString Out of bounds");
                return "";
                break;
        }
    }

    public void LockPlayer(bool l)
    {
        playerLocked = l;
        player.GetComponent<FirstPersonController>().allowInput = !l;
        player.GetComponentInChildren<ShootyShooty>().shootingEnabled = !l;
    }
}