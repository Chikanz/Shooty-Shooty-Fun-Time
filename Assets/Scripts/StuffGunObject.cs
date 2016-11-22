using System.Collections;
using UnityEngine;

public class StuffGunObject : MonoBehaviour
{
    private NetworkMan NM;
    private int destroyed = 0;
    public int stuffGunMulti = 2;
    public float TTL = 0.5f; //Time till live

    private Rigidbody RB;
    public int SpazForce;
    public int SpazChance = 20;

    public AudioClip[] Clips;
    private AudioSource AS;
    private float playOnAwakeDelay;
    private bool playedOnAwake = false;

    public int lifetime = 10;
    private float timeAlive;
    private bool ded;

    private BulletManager BM;

    private void Start()
    {
        AS = GetComponent<AudioSource>();
        AS.clip = Clips[Random.Range(0, Clips.Length)];
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        NetworkMan.RestartEvent += Die;
        RB = GetComponent<Rigidbody>();
        playOnAwakeDelay = Random.Range(0, 0.1f);
        Destroy(gameObject, lifetime + 2);
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > lifetime && !ded)
        {
            GetComponent<BoxCollider>().enabled = false;
            AS.enabled = false;
            ded = true;
        }

        AS.pitch = Time.timeScale;

        if (playOnAwakeDelay > 0)
            playOnAwakeDelay -= Time.deltaTime;
        else if (!playedOnAwake)
        {
            AS.Play();
            playedOnAwake = true;
        }

        if (TTL >= 0)
            TTL -= Time.deltaTime;

        if (Random.Range(0, 60) == 7)
        {
            if (AS.time == 0)
            {
                AS.clip = Clips[Random.Range(0, Clips.Length)];
                AS.Play();
            }
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        NetworkMan.RestartEvent -= Die;
    }

    public void OnCollisionStay(Collision c)
    {
        if (c.gameObject.layer == 10)
        {
            Vector3 rd = new Vector3(Random.value, Random.value, Random.value);
            GetComponent<Rigidbody>().AddForce(rd);
        }
    }

    public void OnCollisionEnter(Collision c)
    {
        if(NM.explosions && c.gameObject.tag != "Stuff" && c.gameObject.tag != "Player")
        {
            int id = BM.NewHit(c.contacts[0].point,Quaternion.identity, c.gameObject);
            BM.CreateNextHit(id);
            Destroy(gameObject);
        }

        if (NM == null)
            NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();

        if (c.gameObject.layer == 10)
        {
            Vector3 rd = new Vector3(Random.value, Random.value, Random.value);
            GetComponent<Rigidbody>().AddForce(SpazForce * rd);
        }

        if (NM.GMFootBall)
        {
            if (c.gameObject.name == "Shooty Ball")
            {
                c.gameObject.GetComponent<Rigidbody>()
                    .AddForceAtPosition(NM.SS.ShootyBallForce * stuffGunMulti * transform.forward, c.contacts[0].point);
            }
        }

        if (c.gameObject.tag == "Player" && TTL < 0)
        {
            if (c.gameObject.GetComponent<Initalize>().photonView.isMine)
            {
                var UI = GameObject.Find("UI Manager").GetComponent<UIManager>();
                c.gameObject.GetComponent<PhotonView>().RPC("GotShot", PhotonTargets.All, 4, new Vector3(0, 0, 0));
                UI.Owch();
            }
        }

        if (NM.godBullets)
        {
            var i = NM.everything.IndexOf(c.gameObject);
            if (i != -1)
            {
                NM.pv.RPC("KillLevel", PhotonTargets.All, i);
                destroyed += 1;
                if (destroyed >= 3)
                    Destroy(gameObject);
            }
            else
                Debug.Log("Couldn't find that item!" + c.gameObject.ToString());
        }
    }
}