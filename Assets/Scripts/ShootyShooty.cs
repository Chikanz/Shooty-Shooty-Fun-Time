using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ShootyShooty : NetworkBehaviour
{
    public ParticleSystem muzzleFlash;

    public Animator anim;

    public GameObject pokeBall;

    public GameObject impactPrefab;
    public GameObject levelImpact;

    public AudioSource gunSound;
    public GameObject casing;
    public Transform casingSpawn;

    public bool coolDown;

    public int maxClip = 10;
    public int inClip = 10;

    //public float sprayfactor = 0.02f;
    public float inaccuracy;

    private float CHPlacement;

    public float moveInac;
    public float shootInnac;
    public float maxInnac = 0.02f;
    public float inacDecayRate = 0.05f;
    public float spamInnac = 0.01f;

    private GameObject[] impacts;
    private int currentImpact = 0;
    private int maxImpacts = 20;
    public bool reloading = false;

    public int maxCasings = 20;
    private List<GameObject> Casings = new List<GameObject>();

    public int maxDecals = 20;
    private List<GameObject> impactList = new List<GameObject>();
    public Light Gunlight;

    public bool shooting = false;
    private float lightTimer;

    private Vector3 accPos = new Vector3(-18, 0, 0);
    private Vector3 inaccPos = new Vector3(-35, 0, 0);

    private Vector3 prevPos;

    public GameObject player;
    private FirstPersonController fpc;

    public bool outtaBullets;

    private float lerp = 0;

    //public GameObject UI;

    private Image CH1;
    private Image CH2;

    private NetworkMan NM;

    public float shootCoolDown = 0.3f;

    [SerializeField]
    private float shootCoolDownTimer = 0;

    private int damage = 1;

    public LightHandler lh;

    private PhotonView Playerpv;

    public bool drunkinnac = false;
    public float drunkInnacChance = 0.6f;

    private float PokeThrust;
    public float pokeTMultiplier;
    private bool inac;
    public bool shootingEnabled = true;

    public int bulletDamage = 1;

    private string[] stuffs =
    {
        "arrow",
        "BEVS",
        "MrShooty",
        "pokeball"
    };

    private void Start()
    {
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        Playerpv = player.GetComponent<PhotonView>();
        prevPos = transform.position;
        Gunlight = GetComponentInChildren<Light>();
        fpc = player.GetComponent<FirstPersonController>();

        //Makey UI
        //var UIinst = Instantiate(UI) as GameObject;
        //Image[] images = UIinst.GetComponentsInChildren<Image>();

        var CH = GameObject.Find("UI Groups/Main UI/CrossHair");
        CH.GetComponent<Canvas>().enabled = true;

        CH1 = CH.transform.GetChild(0).GetComponent<Image>();
        CH2 = CH.transform.GetChild(2).GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !reloading && inClip < maxClip && fpc.m_IsWalking)
        {
            anim.SetTrigger("Reload");
            inClip = maxClip;
            reloading = true;
            Playerpv.RPC("ReloadRPC", PhotonTargets.All, null);
        }

        if (Input.GetButtonDown("Fire1")
            && shootingEnabled
            && !reloading
            && inClip > 0
            && fpc.m_IsWalking
            && shootCoolDownTimer <= 0
            )
        {
            Playerpv.RPC("PewPew", PhotonTargets.Others, null);
            MakeCasing();
            gunSound.Play();
            muzzleFlash.Play();

            anim.SetTrigger("Shooting");

            lh.LightOn();
            shooting = true;

            shootCoolDownTimer = shootCoolDown;

            inClip -= 1;

            if (inClip <= 0)
            {
                anim.SetTrigger("outtaBullets");
                //Playerpv.RPC("outtaBulletsRPC", PhotonTargets.All, null);
                outtaBullets = true;
            }
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            PokeThrust += Time.deltaTime * pokeTMultiplier;
        }

        if (Input.GetKeyUp(KeyCode.Mouse1) && PokeThrust != 0)
        {
            var pb = Instantiate(pokeBall, transform.position, Quaternion.identity) as GameObject;
            pb.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
            PokeThrust = 0;
        }

        if (shootCoolDownTimer > 0.0f)
        {
            shootCoolDownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (shooting && !NM.stuffGun)
        {
            shooting = false;

            RaycastHit hit;
            var spray = transform.forward;

            spray.x += Random.Range(-inaccuracy, inaccuracy);
            spray.y += Random.Range(-inaccuracy, inaccuracy);
            spray.z += Random.Range(-inaccuracy, inaccuracy);

            if (Physics.Raycast(transform.position, spray, out hit, 300f))
            {
                if (hit.transform.tag == "Player" && hit.collider.isTrigger && !hit.transform.GetComponent<PhotonView>().isMine)
                {
                    hit.transform.GetComponent<PhotonView>()
                        .RPC("GotShot", PhotonTargets.All, hit.collider.name == "Head" ? bulletDamage * 2 : bulletDamage, hit.transform.position);

                    Hitmarker(hit.transform.name == "Head");
                    BloodParticles(hit);
                }
                else if (hit.transform.tag == "Enemy")
                {
                    hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * 500, hit.point);
                    BloodParticles(hit);
                }
                else if (NM.godBullets)
                {
                    var i = NM.everything.IndexOf(hit.transform.gameObject);
                    if (i != -1)
                        NM.pv.RPC("KillLevel", PhotonTargets.All, i);
                    else
                        Debug.Log("Couldn't find that item!" + hit.transform.gameObject.ToString());
                }

                if (hit.transform.gameObject.layer == 10)
                {
                    LevelParticles(hit);
                }
            }

            shootInnac += spamInnac;
        }
        else if (shooting && NM.stuffGun)
        {
            var spray = transform.forward;

            spray.x += Random.Range(-inaccuracy, inaccuracy);
            spray.y += Random.Range(-inaccuracy, inaccuracy);
            spray.z += Random.Range(-inaccuracy, inaccuracy);

            var rnd = Random.Range(0, stuffs.Length);
            string stuffString = stuffs[rnd];
            shooting = false;
            var stuff = PhotonNetwork.Instantiate(stuffString, Gunlight.transform.position, Random.rotation, 0);
            stuff.GetComponent<Rigidbody>().AddForce(spray * 1000);
            stuff.GetComponent<Rigidbody>().AddTorque(transform.up * 100);
        }

        CalcInaccuracy();
    }

    private void LevelParticles(RaycastHit hit)
    {
        var c = Instantiate(levelImpact, hit.point, hit.transform.rotation) as GameObject;
        impactList.Add(c);

        if (impactList.Count > maxImpacts)
        {
            Destroy(impactList[0]);
            impactList.RemoveAt(0);
        }
    }

    private void BloodParticles(RaycastHit hit)
    {
        var blood = Instantiate(impactPrefab, hit.point, Quaternion.identity) as GameObject; //hit.point
        blood.transform.localScale = new Vector3(1, 1, 1);
        impactList.Add(blood);

        blood.transform.parent = hit.transform.gameObject.transform;
        blood.GetComponent<ParticleSystem>().Play();

        if (impactList.Count > maxImpacts)
        {
            Destroy(impactList[0]);
            impactList.RemoveAt(0);
        }
    }

    private void MakeCasing()
    {
        var c = Instantiate(casing, casingSpawn.position, Quaternion.identity) as GameObject;
        Casings.Add(c);

        Vector3 force = transform.right * 200;
        force.x += Random.Range(-50, 50);
        c.GetComponent<Rigidbody>().AddForce(force);

        if (Casings.Count <= maxCasings) return;
        Destroy(Casings[0]);
        Casings.RemoveAt(0);
    }

    private IEnumerator drunkInnaccuracy()
    {
        while (true)
        {
            float i = Random.value;
            if (i > drunkInnacChance)
                drunkinnac = false;
            else
                drunkinnac = true;

            float t = Random.Range(0.2F, 0.6f);

            yield return new WaitForSeconds(t);
        }
    }

    public void UpdateCrosshair(float l)
    {
        CH1.transform.localPosition = Vector3.Lerp(inaccPos, accPos, l);
        CH2.transform.localPosition = Vector3.Lerp(-inaccPos, -accPos, l);
    }

    public float map(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {
        float oldRange = (OldMax - OldMin);
        float newRange = (NewMax - NewMin);
        float newValue = (((OldValue - OldMin) * newRange) / oldRange) + NewMin;

        return (newValue);
    }

    private void CalcInaccuracy()
    {
        inac = false; //Prove me wrong approachu desune

        //Jump Case
        if (NM.JumpAcc)
        {
            inac = !fpc.m_Jumping;
            fpc.velocity.y = 1;
        }
        else if (fpc.m_Jumping)
            inac = true;

        //Normal Case
        if (fpc.velocity.magnitude > 10.5f)
            inac = true;

        //Global inaccuracy
        if (outtaBullets || coolDown || drunkinnac)
            inac = true;

        //Apply accuracy
        if (inac)
            moveInac += (maxInnac / 4);
        else
            moveInac -= (maxInnac / 4);

        //Recoil Cooldown
        if (shootInnac > 0.0f)
        {
            shootInnac -= (inacDecayRate * Time.deltaTime);
        }

        CHPlacement = map(0.0f, maxInnac, 1, 0, inaccuracy);
        UpdateCrosshair(CHPlacement);

        inaccuracy = moveInac + shootInnac;

        inaccuracy = Mathf.Clamp(inaccuracy, 0, maxInnac);
        shootInnac = Mathf.Clamp(shootInnac, 0, maxInnac);
        moveInac = Mathf.Clamp(moveInac, 0, maxInnac);
    }

    public void Hitmarker(bool headshot)
    {
        //xX_SWAG YOLO_HITMARKER_SOUND.MP4_Xx\\
        if (headshot)
            GetComponent<AudioSource>().pitch = 1.5f;
        GetComponent<AudioSource>().Play();
        GetComponent<AudioSource>().pitch = 1f;
    }
}