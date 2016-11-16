using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Initalize : Photon.MonoBehaviour
{
    public Material[] Mats;

    private Vector3 PlayerPos;
    private Quaternion PlayerRot;

    public int health = 4;
    public int maxHealth = 4;
    public int GubsCount;

    //private float endRoundTimer;
    private float smoothing = 15f;

    private NetworkMan NM;

    private Animator anim;

    public Transform FPcam;
    private Quaternion camRot;

    private LightHandler lh;

    private ParticleSystem PS;

    private Transform body;

    private UIManager UIMan;

    private bool running = false;
    private bool walking = false;
    private bool reloading = false;
    private bool outtabullets = false;

    private bool damaged;
    public Color flashColour = new Color(1f, 0f, 0f, 1f);

    public bool died;

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
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        UIMan = GameObject.Find("UI Manager").GetComponent<UIManager>();

        anim = GetComponentInChildren<Animator>();
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
            transform.Find("Head").gameObject.layer = 2;

            GetComponent<Rigidbody>().useGravity = true;
            GetComponentInChildren<FirstPersonController>().enabled = true;

            GetComponent<CharacterController>().enabled = true;

            GetComponentInChildren<AudioListener>().enabled = true;
            GetComponentInChildren<FlareLayer>().enabled = true;
            //GetComponentInChildren<Skybox>().enabled = true;

            GetComponentInChildren<ShootyShooty>().enabled = true;
            GetComponentInChildren<GUILayer>().enabled = true;

            GetComponentInChildren<GunAnmiation>().enabled = true;

            //transform.FindChild("Body").gameObject.layer = 2;

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
            var mat = NM.playerNumber == 0 ? 1 : 0;
            transform.FindChild("Body").GetComponent<MeshRenderer>().material = Mats[mat];
            StartCoroutine("UpdateData");
        }
    }

    public void Update()
    {
        if (photonView.isMine)
        {
            running = anim.GetBool("IsRunning");
            walking = anim.GetBool("IsWalking");
            camRot = FPcam.rotation;
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
            stream.SendNext(GubsCount);
        }
        else
        {
            SD.pos = (Vector3)stream.ReceiveNext();
            SD.Rot = (Quaternion)stream.ReceiveNext();
            SD.camRot = (Quaternion)stream.ReceiveNext();
            SD.AnimRunning = (bool)stream.ReceiveNext();
            SD.AnimWalking = (bool)stream.ReceiveNext();
            GubsCount = (int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void GotShot(int damage, Vector3 forcepos)
    {
        health -= damage;

        if (health <= 0 && photonView.isMine)
        {
            if (NM.oneShot && NM.GMRace)
                NM.RaceSpawnReset();

            Die(forcepos);
        }
    }

    [PunRPC]
    public void PlayHurt()
    {
        UIMan.Owch();
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

    [PunRPC]
    public void GetGubs(int count)
    {
        GubsCount += count;
    }

    [PunRPC]
    public void HalfGubs()
    {
        if (PhotonNetwork.isMasterClient)
        {
            for (int i = 0; i < GubsCount/2; i++)
                PhotonNetwork.Instantiate("franku", transform.position + (new Vector3(Random.value, 0, Random.value) * 5)
                    , Random.rotation, 0);
        }

        GubsCount /= 2;
    }

    //gubs rpc caller
    public void getGubsHandler()
    {
            Debug.Log("You've Got Gubs!");
            GetComponent<PhotonView>().RPC("GetGubs", PhotonTargets.All, 1);
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
        DieInner();
        if (!NM.MDeath)
            GetComponent<Rigidbody>().AddForce(body.transform.forward * 700);
    }

    private void DieInner()
    {
        if (died)
            return;

        if (NM.MDeath)
        {
            if(NM.bFriday)
            {
                if (GetComponent<PhotonView>().isMine)
                    GetComponent<PhotonView>().RPC("HalfGubs", PhotonTargets.All, null);
            }

            health = maxHealth;
            NM.MoveToSpawn();
            return;
        }

        if (!NM.roundEnded)
        {
            NM.pv.RPC(NM.playerNumber == 0 ? "P2Up" : "P1Up", PhotonTargets.All, null);
        }

        SetKill(true);
        died = true;
    }

    public void SetKill(bool d) //true when dead
    {
        //GetComponentInChildren<Animator>().SetBool("isDead", d);
        died = d;
        GetComponentInChildren<ShootyShooty>().shootingEnabled = !d;
        GetComponent<FirstPersonController>().enabled = !d;
        GetComponent<Rigidbody>().isKinematic = !d;
        GetComponentInChildren<Animator>().enabled = !d;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "OuttaBounds" && !died && photonView.isMine)
        {
            photonView.RPC("GotShot", PhotonTargets.All, 999, new Vector3(Random.value, Random.value, Random.value) * 1000);
            UIMan.Owch();
            //if(NM.gmSelect != 1)
            //    NM.pv.RPC("SendChatMessage", PhotonTargets.All, PhotonNetwork.player.name + " fell off the map like an idiot.");
        }
    }
}