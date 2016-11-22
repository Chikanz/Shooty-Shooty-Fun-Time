using UnityEngine;
using System.Collections;

public class Shopper : MonoBehaviour
{
    enum eShopper
    {
        WANDERING,  //Looking for object
        LOCATED,    //Found object, moving towards
        HURT,       //Talk shit get hit
        STANDUP,    //On yer feet now
    }

    NavMeshAgent agent;
    GameObject foundObject;
    Rigidbody RB;
    private Vector3 ShopCenter = new Vector3(0,3.76f,-6.6f);

    eShopper state = eShopper.WANDERING;

    public int gubsCount;
    public float searchRadiusSize = 16;
    public float WanderDistance = 20;
    private Quaternion assRottaion;     //Rotation when knocked over on ass
    private Vector3 assPos;     //Rotation when knocked over on ass

    private float timeToStand;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!PhotonNetwork.isMasterClient)
        {
            agent.enabled = false;
            GetComponent<Shopper>().enabled = false;
        }

        RB = GetComponent<Rigidbody>();
        agent.destination = GetNewWander();
    }

    void Update()
    {
        if (state == eShopper.WANDERING)
        {
            //Get new wander
            if (agent.remainingDistance < 0.5f)
                agent.destination = GetNewWander();

            //Find new object
            Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadiusSize);
            //foreach (Collider c in colliders)
            float lowestDistance = 999;
            int LDindex = -1;

            //Make more efficient
            for (int i = 0; i < colliders.Length; i++)
            {
                var c = colliders[i];
                if (c.transform.tag == "Gubs")
                {
                    //See if the shopper can see the target
                    //https://docs.unity3d.com/Manual/DirectionDistanceFromOneObjectToAnother.html)
                    var heading = c.transform.position - transform.position;
                    var distance = heading.magnitude;
                    //var direction = heading / distance; // This is now the normalized direction.

                    if(distance < lowestDistance)
                    {
                        LDindex = i;
                        lowestDistance = distance;
                    } 
                }
            }

            if (LDindex != -1)
            {
                //Feed item to agent if found
                state = eShopper.LOCATED;
                agent.destination = colliders[LDindex].transform.position;
                foundObject = colliders[LDindex].gameObject;
            }
        }

        //Found an item in range
        if (state == eShopper.LOCATED)
        {
            //Object was taken by someone else
            if (!foundObject.activeSelf)
                state = eShopper.WANDERING;

            //Update position
            if (foundObject.transform.position != agent.destination)
                agent.destination = foundObject.transform.position;
        }

        if (state == eShopper.HURT)
        {
            //Use time to make sure it's not triggered instantly
            
            if (RB.velocity.magnitude < 0.5f)
            {
                timeToStand += Time.deltaTime;

                if (timeToStand > 2)
                {
                    state = eShopper.STANDUP;
                    assRottaion = transform.rotation;
                    assPos = transform.position;
                    timeToStand = 0;
                }
            }
        }

        if (state == eShopper.STANDUP)
        {
            timeToStand += (Time.deltaTime * 0.5f);
            var standpos = new Vector3(assPos.x, assPos.y + 1.5f, assPos.z);
            transform.rotation = Quaternion.Lerp(assRottaion, Quaternion.identity, timeToStand);
            transform.position = Vector3.Lerp(assPos, standpos, timeToStand);

            if (timeToStand >= 1)
            {
                GetComponent<MeshCollider>().enabled = true;
                HurtMode(false);
                timeToStand = 0;

                //Check if on mesh
                if (!agent.isOnNavMesh)
                {
                    RB.useGravity = true;
                    GetComponent<MeshCollider>().enabled = false;
                }
                else
                {
                    state = eShopper.WANDERING;
                }

                ////Check if on nav mesh
                //NavMeshHit hit;
                //if(NavMesh.SamplePosition(transform.position, out hit, 1f, 1))
                //{
                //    HurtMode(false);
                //    timeToStand = 0;
                //    state = eShopper.WANDERING;
                //}
                //else //Kill self if not on mesh
                //{
                //    GetComponent<MeshCollider>().enabled = false;
                //    HurtMode(true);
                //}

            }
        }
    }

    Vector3 GetNewWander()
    {
        //Searched for get random nav mesh point on google
        Vector3 randomDir = ShopCenter + (Random.insideUnitSphere * WanderDistance);
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDir, out hit, WanderDistance, 1);
        return hit.position;
    }

    void HurtMode(bool b)
    {
        agent.enabled = !b;
        RB.useGravity = b;
        RB.isKinematic = !b;
    }

    void shoot(Vector3 facing, int force)
    {
        HurtMode(true);
        state = eShopper.HURT;
        RB.AddForce(facing * force);
        timeToStand = 0;
    }

    [PunRPC]
    void Shoot(Vector3 facing)
    {
        if(PhotonNetwork.isMasterClient)
            shoot(facing,1000);
    }

    [PunRPC]
    void Punch()
    {
        if (PhotonNetwork.isMasterClient)
            shoot(transform.up, 1000);
    }

    [PunRPC]
    void RagDoll()
    {
        if (PhotonNetwork.isMasterClient)
            shoot(Vector3.zero,0);
    }
}
