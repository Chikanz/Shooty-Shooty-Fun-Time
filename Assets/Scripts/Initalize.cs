﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Initalize : Photon.MonoBehaviour
{
    private Image hurtImage2;
    public Color hurtImageColor;

    private Vector3 PlayerPos;
    private Quaternion PlayerRot;

    public int health = 4;

    private float endRoundTimer;
    private float smoothing = 15f;

    private NetworkMan NM;

    private Animator anim;

    public Transform FPcam;
    private Quaternion camRot;

    private LightHandler lh;

    private ParticleSystem PS;

    private Transform body;

    private bool running = false;
    private bool walking = false;
    private bool reloading = false;
    private bool outtabullets = false;

    private bool damaged;
    public Color flashColour = new Color(1f, 0f, 0f, 1f);

    private bool died;

    public struct SendData
    {
        public Vector3 pos; //(transform.position);
        public Quaternion Rot;//(transform.rotation);
        public Quaternion camRot;//(FPcam.rotation);
        public bool AnimRunning;//(anim.GetBool("IsRunning"))
        public bool AnimWalking;//(anim.GetBool("IsWalking"))
    }

    private SendData SD;

    private void Start()
    {
        PhotonNetwork.sendRate = 60;
        PhotonNetwork.sendRateOnSerialize = 60;

        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        anim = GetComponentsInChildren<Animator>()[1];
        FPcam = transform.Find("FirstPersonCharacter");
        lh = GetComponentInChildren<LightHandler>();
        PS = GetComponentInChildren<ParticleSystem>();

        foreach (Transform c in GetComponentsInChildren<Transform>())
        {
            if (c.name != "Body") continue;
            body = c;
            break;
        }

        if (photonView.isMine)
        {
            hurtImage2 = transform.Find("/UI Groups/Main UI/HurtImage").GetComponent<Image>();
            gameObject.layer = 2;

            GetComponent<Rigidbody>().useGravity = true;
            GetComponentInChildren<FirstPersonController>().enabled = true;

            GetComponent<CharacterController>().enabled = true;

            GetComponentInChildren<AudioListener>().enabled = true;
            GetComponentInChildren<FlareLayer>().enabled = true;
            //GetComponentInChildren<Skybox>().enabled = true;

            GetComponentInChildren<ShootyShooty>().enabled = true;
            GetComponentInChildren<GUILayer>().enabled = true;

            GetComponentInChildren<GunAnmiation>().enabled = true;

            transform.FindChild("Body").gameObject.layer = 2;

            foreach (Camera cam in GetComponentsInChildren<Camera>())
                cam.enabled = true;

            Transform Gun = transform.Find("FirstPersonCharacter/GunCamera/Gun");
            Transform particles = transform.Find("FirstPersonCharacter/GunCamera/Particles");

            foreach (Transform gunParts in Gun.GetComponentsInChildren<Transform>())
                gunParts.gameObject.layer = 8;

            particles.gameObject.layer = 8;
        }
        else
        {
            StartCoroutine("UpdateData");
        }
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.L) && photonView.isMine)
            Die();

        if (photonView.isMine)
        {
            running = anim.GetBool("IsRunning");
            walking = anim.GetBool("IsWalking");
            camRot = FPcam.rotation;

            if (died)
                endRoundTimer -= Time.deltaTime;

            //endRoundTimer = Mathf.Clamp(endRoundTimer, 0, 10);

            if (endRoundTimer <= 0 && died)
            {
                SetKill(false);
                GetComponentInChildren<Animator>().enabled = false;
                died = false;
                NM.pv.RPC("RestartRound", PhotonTargets.All, null);
            }
        }
    }

    private IEnumerator UpdateData()
    {
        while (PhotonNetwork.playerList.Length != 1)
        {
            transform.position = Vector3.Lerp(transform.position, SD.pos, Time.deltaTime * smoothing);
            transform.rotation = Quaternion.Lerp(transform.rotation, SD.Rot, Time.deltaTime * smoothing);
            FPcam.rotation = Quaternion.Lerp(FPcam.rotation, SD.camRot, Time.deltaTime * smoothing);

            anim.SetBool("IsRunning", SD.AnimRunning);
            anim.SetBool("IsWalking", SD.AnimWalking);

            //anim.SetBool("Reload", reloading);
            //anim.SetBool("outtaBullets", outtabullets);

            yield return null;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(camRot);
            stream.SendNext(running);
            stream.SendNext(walking);
        }
        else
        {
            SD.pos = (Vector3)stream.ReceiveNext();
            SD.Rot = (Quaternion)stream.ReceiveNext();
            SD.camRot = (Quaternion)stream.ReceiveNext();
            SD.AnimRunning = (bool)stream.ReceiveNext();
            SD.AnimWalking = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void GotShot(int damage, Vector3 forcepos)
    {
        health -= damage;

        if (photonView.isMine)
            hurtImage2.color = hurtImageColor;

        if (health <= 0 && photonView.isMine)
        {
            Die(forcepos);
        }
    }

    [PunRPC]
    public void PewPew()
    {
        PS.Play();
        lh.LightOn();
        GetComponentInChildren<ShootyShooty>().gunSound.Play();
    }

    [PunRPC]
    public void ReloadRPC()
    {
        anim.SetTrigger("Reload");
    }

    [PunRPC]
    public void outtaBulletsRPC()
    {
        anim.SetTrigger("outtaBullets");
    }

    public void Die(Vector3 forcepos)
    {
        if (died)
        {
            GetComponent<Rigidbody>().AddForceAtPosition(body.transform.forward * 500, forcepos);
            return;
        }
        DieInner();
    }

    public void Die()
    {
        GetComponent<Rigidbody>().AddForce(body.transform.forward * -700);
        DieInner();
    }

    private void DieInner()
    {
        if (died)
            return;

        if (!NM.roundEnded && photonView.isMine)
        {
            NM.shotcaller = true;
            NM.pv.RPC(NM.playerNumber == 0 ? "P2Up" : "P1Up", PhotonTargets.All, null);
            endRoundTimer = 5;
        }

        SetKill(true);
        died = true;
    }

    private void SetKill(bool d) //true when dead
    {
        //GetComponentInChildren<Animator>().SetBool("isDead", d);
        GetComponentInChildren<ShootyShooty>().shootingEnabled = !d;
        GetComponent<FirstPersonController>().enabled = !d;
        GetComponent<Rigidbody>().isKinematic = !d;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "OuttaBounds" && !died && photonView.isMine)
        {
            Die();
            NM.pv.RPC("SendChatMessage", PhotonTargets.All, PhotonNetwork.player.name + " fell off the map like an idiot.");
        }
    }
}