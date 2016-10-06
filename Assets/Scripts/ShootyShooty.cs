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

    public GameObject Bullet;

    public GameObject impactPrefab;

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
    private FirstPersonController FPC;

    public bool outtaBullets;

    private float lerp = 0;

    //public GameObject UI;

    private Image CH1;
    private Image CH2;

    private NetworkMan NM;
    private BulletManager BM;

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
    public int bulletSpeed = 1000;

    public float slowMoJuice;
    public float slowMoMax = 3;
    public bool slowMoInUse = false;

    public int ShootyBallForce = 500;
    public int StuffGunForce = 1000;

    private readonly string[] stuffs =
    {
        "Doggo",
        "cat",
    };

    private void Start()
    {
        BM = GameObject.Find("Bullet Manager").GetComponent<BulletManager>();
        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        Playerpv = player.GetComponent<PhotonView>();
        prevPos = transform.position;
        Gunlight = GetComponentInChildren<Light>();
        FPC = player.GetComponent<FirstPersonController>();

        var CH = GameObject.Find("UI Groups/Main UI/CrossHair");
        CH.GetComponent<Canvas>().enabled = true;

        CH1 = CH.transform.GetChild(0).GetComponent<Image>();
        CH2 = CH.transform.GetChild(2).GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !reloading && inClip < maxClip && FPC.m_IsWalking)
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
            && FPC.m_IsWalking
            && shootCoolDownTimer <= 0
            )
        {
            Playerpv.RPC("PewPew", PhotonTargets.Others, null);
            MakeCasing();
            if (!NM.stuffGun)
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

        //SlowMo Stuff
        if (NM.slowMo && !slowMoInUse && slowMoJuice < slowMoMax)
        {
            slowMoJuice += Time.deltaTime * 0.5f;
        }
        else if (slowMoInUse)
        {
            slowMoJuice -= Time.deltaTime * 2;
        }

        if (slowMoJuice <= 0)
        {
            slowMoJuice = 0;
            slowMoInUse = false;
            NM.pv.RPC(PhotonNetwork.isMasterClient ? "P1SlowMoSet" : "P2SlowMoSet",
                    PhotonTargets.All, false);
        }

        if (NM.slowMo && Input.GetKeyDown(KeyCode.LeftShift) && slowMoJuice > 0)
        {
            if (!slowMoInUse)
            {
                slowMoInUse = true;
                NM.pv.RPC(PhotonNetwork.isMasterClient ? "P1SlowMoSet" : "P2SlowMoSet",
                    PhotonTargets.All, true);
            }
            else
            {
                slowMoInUse = false;
                NM.pv.RPC(PhotonNetwork.isMasterClient ? "P1SlowMoSet" : "P2SlowMoSet",
                    PhotonTargets.All, false);
            }
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

            var bulletCount = 1;
            if (NM.shotGun)
                bulletCount = 4;

            for (int i = 0; i < bulletCount; i++)
            {
                RaycastHit hit;

                var spray = transform.forward;

                spray.x += Random.Range(-inaccuracy, inaccuracy);
                spray.y += Random.Range(-inaccuracy, inaccuracy);
                spray.z += Random.Range(-inaccuracy, inaccuracy);

                if (Physics.Raycast(transform.position, spray, out hit, 300f))
                {
                    //Bullet Instance
                    var bullet = Instantiate(
                        Bullet, Gunlight.transform.position,
                        Gunlight.transform.rotation * Quaternion.Euler(-90, 0, 0)) as GameObject;

                    var BS = bullet.GetComponent<bulletScript>();

                    //Link Bullet and impact
                    if (hit.transform.tag != "Player" &&
                        (hit.transform.gameObject.layer != 12 &&
                        hit.transform.gameObject.layer != 11 || NM.explosions))
                    {
                        BS.SetHitPos(hit.transform);
                        BS.id = BM.NewHit(hit, hit.transform.gameObject);
                    }
                    else
                        BS.impact = false;

                    // Gets a vector that points from the player's position to the target's.
                    //(from https://docs.unity3d.com/Manual/DirectionDistanceFromOneObjectToAnother.html)
                    var heading = hit.point - Gunlight.transform.position;
                    var distance = heading.magnitude;
                    var direction = heading / distance; // This is now the normalized direction.

                    bullet.GetComponent<Rigidbody>().AddForce(direction * bulletSpeed);

                    //ShootyBall
                    if (hit.transform.gameObject.layer == 11 && !slowMoInUse)
                    {
                        hit.transform.GetComponent<PhotonView>().RequestOwnership();
                        hit.transform.GetComponent<Rigidbody>()
                            .AddForceAtPosition(transform.forward * ShootyBallForce, hit.point);
                    }

                    //Get shot
                    if (hit.transform.tag == "Player" && hit.collider.isTrigger &&
                        !hit.transform.GetComponent<PhotonView>().isMine)
                    {
                        hit.transform.GetComponent<PhotonView>()
                            .RPC("GotShot", PhotonTargets.All,
                                hit.collider.name == "Head" ? bulletDamage * 2 : bulletDamage, hit.transform.position);

                        Hitmarker(hit.transform.name == "Head");
                        BloodParticles(hit);
                    }
                    else if (hit.transform.tag == "Enemy")
                    {
                        hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(transform.forward * 500, hit.point);
                        BloodParticles(hit);
                    }
                    else if (hit.transform.tag == "Casing")
                    {
                        hit.transform.GetComponent<Rigidbody>().AddForce(200 * transform.forward);
                    }
                    else if (NM.godBullets)
                    {
                        var index = NM.everything.IndexOf(hit.transform.gameObject);
                        if (index != -1)
                            NM.pv.RPC("KillLevel", PhotonTargets.All, index);
                        else
                            Debug.Log("Couldn't find that item!" + hit.transform.gameObject.ToString());
                    }
                }
            }
            FPC.KickCam();
            shootInnac += spamInnac;
        }
        else if (shooting && NM.stuffGun) //Stuff Gun
        {
            var spray = transform.forward;

            spray.x += Random.Range(-inaccuracy, inaccuracy);
            spray.y += Random.Range(-inaccuracy, inaccuracy);
            spray.z += Random.Range(-inaccuracy, inaccuracy);

            var animal = NM.playerNumber;
            string stuffString = stuffs[animal];
            shooting = false;

            int count = 1;
            if (NM.shotGun)
                count = 5;

            for (int i = 0; i < count; i++)
            {
                var pet = PhotonNetwork.Instantiate(stuffString, Gunlight.transform.position, Random.rotation, 0);
                pet.GetComponent<Rigidbody>().AddForce(spray * StuffGunForce);
                pet.GetComponent<Rigidbody>().AddTorque(transform.up * 100);
                BM.NewPet(pet);
            }
        }

        CalcInaccuracy();
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

    public float Map(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {
        float oldRange = (OldMax - OldMin);
        float newRange = (NewMax - NewMin);
        float newValue = (((OldValue - OldMin) * newRange) / oldRange) + NewMin;

        return (newValue);
    }

    private void CalcInaccuracy()
    {
        inac = false; //Prove me wrong approachu desune

        //Normal Case
        if (FPC.velocity.magnitude > 10.5f)
            inac = true;

        if (FPC.m_Jumping)
            inac = true;

        //Override movement
        if (NM.GMRace)
            inac = false;

        //Jump Case
        if (NM.JumpAcc)
        {
            inac = !FPC.m_Jumping;
        }

        //Global inaccuracy
        if (outtaBullets || coolDown || drunkinnac || NM.shotGun)
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

        CHPlacement = Map(0.0f, maxInnac, 1, 0, inaccuracy);
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