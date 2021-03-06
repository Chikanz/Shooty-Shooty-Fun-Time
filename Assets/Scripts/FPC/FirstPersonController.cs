using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour
{
    public float testVel;

    public static float m_GravityMultiplier = 2;

    [SerializeField]
    public bool m_IsWalking;

    [SerializeField]
    private float m_WalkSpeed;

    [SerializeField]
    private float m_RunSpeed;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_RunstepLenghten;

    [SerializeField]
    private float m_JumpSpeed;

    [SerializeField]
    private float m_StickToGroundForce;

    [SerializeField]
    public MouseLook m_MouseLook;

    [SerializeField]
    private bool m_UseFovKick;

    [SerializeField]
    private FOVKick m_FovKick = new FOVKick();

    [SerializeField]
    private bool m_UseHeadBob;

    [SerializeField]
    private CurveControlledBob m_HeadBob = new CurveControlledBob();

    [SerializeField]
    private LerpControlledBob m_JumpBob = new LerpControlledBob();

    [SerializeField]
    private float m_StepInterval;

    [SerializeField]
    private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.

    [SerializeField]
    private AudioClip m_JumpSound;           // the sound played when character leaves the ground.

    [SerializeField]
    private AudioClip m_LandSound;           // the sound played when character touches back on ground.

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    public Vector2 m_Input;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    public bool m_Jumping;
    private AudioSource m_AudioSource;

    public Vector3 velocity;

    private float speed;

    public Animator anim;

    public Vector3 m_MoveDir = Vector3.zero;

    private float horizontal = 0f;
    private float vertical = 0f;

    public bool allowInput = true;

    public Vector3 slippyVel;
    public float friction = 0.5f;

    public Vector3 desiredMove;

    private NetworkMan NM;
    private ShootyShooty SS;
    private Rigidbody RB;

    private Vector3 blinkVel;
    private float blinkLerp = 99;
    private Vector3 blinkFrom;
    public int blinks;
    private float blinkTimer;
    public int maxBlinks = 3;

    public int blinkSpeed = 4;
    public float blinkDistance = 2;

    public float CamKick = 0f;
    public float Kick = 2f;
    public float KickDecay = 1;
    public float KickDecayStart = 1;

    public float bouncePadForce;
    public bool bounced = false;

    //Double jump stuff
    private bool doubleJump;
    private bool touchingDoubleJump;
    private bool canDoubleJump = true;
    public float dJumpBonus;
    public string lastTouched = "";
    public string prevLastTouched;

    public bool Mlook = true;
    public bool canMove = true;

    //Explosion Stuff
    public bool explosionMode;
    private Vector3 landed;
    public float stopMagnitude = 10;
    private float explosionTimer = 0;

    //Screen shake stuff
    public float shakeDuration = 0.5f;
    public float shakeSpeed = 1.0f;
    public float shakeMagnitude = 0.1f;

    Vector3 CamShake = Vector3.zero;


    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();
        m_MouseLook.Init(transform, m_Camera.transform);

        NM = GameObject.Find("NetworkManager").GetComponent<NetworkMan>();
        SS = GetComponentInChildren<ShootyShooty>();
        RB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump && allowInput && (m_CharacterController.isGrounded))
        {
            m_Jump = Input.GetButtonDown("Jump");
        }

        //Double jump
        if (touchingDoubleJump && canDoubleJump && Input.GetButtonDown("Jump"))
        {
            doubleJump = true;
            canDoubleJump = false;
        }

        //Grounded stuff
        if (!m_PreviouslyGrounded && (m_CharacterController.isGrounded))
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }

        //Something to do with falling?
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        if (lastTouched != prevLastTouched)
            canDoubleJump = true;

        prevLastTouched = lastTouched;

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        //Explodey things
        //Explosion timer is the minumum time before stop magnitude inscreases,
        //which stops the player when their magnitude is less than it.
        if (explosionMode && explosionTimer > 0)
        {
            explosionTimer -= Time.deltaTime;
        }
        else if (explosionMode && explosionTimer <= 0)
        {
            stopMagnitude += Time.deltaTime * 4f;
        }

        //Stop Explosion mode
        if (RB.velocity.magnitude < stopMagnitude && explosionMode)
        {
            SetExplosionMode(false);
            Debug.Log("Nah");
        }
    }

    private void FixedUpdate()
    {
        GetInput(out speed);
        // always move along the camera forward as it is the direction that it being aimed at
        desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        //Walking animation
        if (m_Input.y != 0 || m_Input.x != 0)
            anim.SetBool("IsWalking", true);
        else
            anim.SetBool("IsWalking", false);

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        //Makes cancelling directions possible
        //desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        if (canMove)
        {
            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;

            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    if (bounced)
                    {
                        m_MoveDir.y = -velocity.y * bouncePadForce;
                        bounced = false;
                    }
                    else
                        m_MoveDir.y = m_JumpSpeed;

                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else if (doubleJump)
            {
                m_MoveDir.y = m_JumpSpeed * dJumpBonus;
                doubleJump = false;

                PlayJumpSound();
                m_Jump = false;
                m_Jumping = true;
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
        }

        Vector3 outVel = Vector3.zero;
        if (NM.drunk)
        {
            slippyVel = Vector3.Lerp(slippyVel, m_MoveDir, 5 * friction * friction * friction * Time.deltaTime);
            outVel = new Vector3(slippyVel.x, m_MoveDir.y, slippyVel.z);
            m_HeadBob.HorizontalBobRange = 0.3f;
            velocity = slippyVel;
        }
        else
        {
            if (canMove)
            {
                velocity = m_MoveDir;
                m_HeadBob.HorizontalBobRange = 0.1f;
                outVel = m_MoveDir;
            }
        }

        if (blinkLerp <= 1)
            outVel = Vector3.zero;

        //BIG DADDY MOVE
        if (canMove)
            m_CollisionFlags = m_CharacterController.Move(outVel * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (NM.blink && blinks > 0)// && !(blinkLerp <= 1))
            {
                blinkFrom = transform.position;
                blinkVel = transform.position += new Vector3(m_MoveDir.x, 0, m_MoveDir.z) * blinkDistance;
                blinkLerp = 0;
                blinks -= 1;
                blinkTimer = 0;
            }
        }
        if (blinkLerp <= 1)
        {
            transform.position = Vector3.Lerp(blinkFrom, blinkVel, blinkLerp);
            blinkLerp += Time.deltaTime * blinkSpeed;
        }
        if (blinks < maxBlinks && blinkTimer > 3)
        {
            blinks += 1;
            blinkTimer = 0;
        }
        else
        {
            blinkTimer += Time.deltaTime;
        }

        //THE BIGGEST HACK IN THE HISTORY OF MANKIND
        if (landed != Vector3.zero)
        {
            transform.position = landed;
            landed = Vector3.zero;
        }
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void PlayJumpSound()
    {
        m_AudioSource.clip = m_JumpSound;
        m_AudioSource.Play();
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                         Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded)
        {
            return;
        }
        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCP;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                  (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCP = m_Camera.transform.localPosition;
            newCP.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCP = m_Camera.transform.localPosition;
            newCP.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }

        m_Camera.transform.localPosition = newCP;         //+ CamShake
    }

    //Zac's Kick back 
    public void KickCam()
    {
        CamKick += Kick;
    }

    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine("Shake");
    }

    IEnumerator Shake()
    {
        float elapsed = 0.0f;
        float randomStart = UnityEngine.Random.Range(-1000.0f, 1000.0f);
        Vector3 ogCamPos = m_Camera.transform.localPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float percentComplete = elapsed / shakeDuration;

            // We want to reduce the shake from full power to 0 starting half way through
            float damper = 1.0f - Mathf.Clamp(2.0f * percentComplete - 1.0f, 0.0f, 1.0f);

            // Calculate the noise parameter starting randomly and going as fast as speed allows
            float alpha = randomStart + shakeSpeed * percentComplete;

            // map noise to [-1, 1]
            //float x = Util.Noise.GetNoise(alpha, 0.0f, 0.0f) * 2.0f - 1.0f;
            //float y = Util.Noise.GetNoise(0.0f, alpha, 0.0f) * 2.0f - 1.0f;

            float x = Mathf.PerlinNoise(alpha, 0) * 2.0f - 1.0f;
            float y = Mathf.PerlinNoise(0, alpha) * 2.0f - 1.0f;

            x *= shakeMagnitude * damper;
            y *= shakeMagnitude * damper;

            m_Camera.transform.localPosition = new Vector3(x, y, ogCamPos.z); 
            //CamShake.x = x;
            //CamShake.y = y;

            yield return null;
        }

        //m_Camera.transform.localPosition = ogCamPos;
        CamShake = Vector3.zero;
    }


    private void GetInput(out float speed)
    {
        // Read input
        if (allowInput)
        {
            horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            vertical = CrossPlatformInputManager.GetAxis("Vertical");
        }

        anim.SetBool("IsRunning", !m_IsWalking);

        bool waswalking = m_IsWalking;
        m_IsWalking = true; //!Input.GetKey(KeyCode.LeftShift);

        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }

    private void RotateView()
    {
        if (Mlook)
            m_MouseLook.LookRotation(transform, m_Camera.transform);

        if (CamKick >= 0)
        {
            m_Camera.transform.rotation *= Quaternion.Euler(-CamKick, 0, 0);
            CamKick -= (Time.deltaTime * KickDecay);
            KickDecay *= 2;
        }
        else
            KickDecay = KickDecayStart;
    }

    public void ResetCamDirection()
    {
        m_MouseLook.Init(transform, m_Camera.transform);
    }

    public void OnTriggerEnter(Collider c)
    {
        if (c.tag == "CheckPoint" && GetComponent<PhotonView>().isMine)
        {
            NM.SetSpawn(c.transform);
        }

        if (c.tag == "NoFloor")
        {
            NM.FXList.text = "Lol";
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        lastTouched = hit.transform.name;

        if (hit.gameObject.layer == 11)
        {
            hit.transform.GetComponent<PhotonView>().RequestOwnership();
        }

        if (!bounced && hit.transform.tag == "BouncePad")
        {
            m_Jump = true;
            m_Jumping = true;
            bounced = true;
        }

        if (hit.transform.tag == "FinishLine" && !NM.roundEnded)
        {
            NM.GMRoundEnd(PhotonNetwork.isMasterClient);
        }

        touchingDoubleJump = (hit.transform.tag == "DoubleJump");

        if (body == null || body.isKinematic)
        {
            return;
        }

        body.AddForceAtPosition(m_CharacterController.velocity, hit.point, ForceMode.Impulse);
    }

    public void SetExplosionMode(bool b)
    {
        explosionMode = b;
        canMove = !b;
        GetComponent<Rigidbody>().isKinematic = !b;
        GetComponent<Rigidbody>().freezeRotation = b;
        GetComponent<Rigidbody>().useGravity = b;
        stopMagnitude = 0;

        if (!b)
        {
            landed = transform.position;
            m_MoveDir.y = 0;
        }
        else
        {
            explosionTimer = 0.1f;
        }
    }

    public void Explode()
    {
        //Can only explode self
        //can only explode while not exploded
        if (!GetComponent<PhotonView>().isMine || explosionMode) return;

        SetExplosionMode(true);
    }

    public void changeSensitivity(float x, float y)
    {
        m_MouseLook.XSensitivity = x;
        m_MouseLook.YSensitivity = y;
    }
}