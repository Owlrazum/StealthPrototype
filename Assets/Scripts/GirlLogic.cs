using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class GirlLogic : MonoBehaviour
{
    /// <summary>
    /// This MonoBehavior contains the following Coroutines:
    /// Patrolling, Wait, Chasing and PlayerVisible
    /// </summary>

    #region Animations
    Animator animator;
    public enum AnimationState
    {
        Idle,
        Sleeping,
        Walking,
        LookingAround,
        Searching,
        Chasing,
        Capturing
    }
    private AnimationState animationStateData;
    private AnimationState animationState
    {
        get { return animationStateData; }
        set
        {
            if (animationStateData != value)
            {
                animationStateData = value;
                switch (animationStateData)
                {
                    case AnimationState.Idle:
                        animator.CrossFade("Base Layer.Girl_Idle", 0.1f);
                        //Debug.Log("StateChanged WAlk to idle ANIMAITON");
                        break;
                    case AnimationState.Sleeping:
                        animator.CrossFade("Base Layer.Sleeping", 0.1f);
                        break;
                    case AnimationState.Walking:
                        animator.CrossFade("Base Layer.Walking", 0.1f);
                        break;
                    case AnimationState.LookingAround:
                        animator.CrossFade("Base Layer.LookingAround", 0.1f);
                        agent.updateRotation = false;
                        float waitUntilLookAround = animator.GetCurrentAnimatorStateInfo(0).length;
                        //Debug.Log("CHHHHHH " + animator.GetCurrentAnimatorStateInfo(0).length);
                        IsWaitCoroutineRunning = (true, 3);
                        fieldOfView.Activate();
                        //animator.CrossFade("Base Layer.Girl_Idle", 0.1f);
                        break;
                    case AnimationState.Chasing:
                        IsPatrolCoroutineRunning = false;
                        IsPlayerChasingCoroutineRunning = true;
                        animator.CrossFade("Base Layer.Chasing", 0.1f);
                        break;
                    case AnimationState.Searching:
                        // TODO
                        animationState = AnimationState.Idle;
                        break;
                    case AnimationState.Capturing:
                        animator.CrossFade("Base Layer.Girl_Idle", 0.1f);
                        transform.position += new Vector3(0, 2, 0);
                        transform.rotation = Quaternion.Euler(180, 0, 0);
                        break;
                }
            }
        }
    }
    #endregion

    #region SerializedFields
    [SerializeField]
    private bool debug1;
    [SerializeField]
    private bool debug2;
    [SerializeField]
    private bool debug3;

    [SerializeField]
    private float speed = 2;
    [SerializeField]
    private bool shouldLookAround;
    [SerializeField]
    private float lookingAroundTime = 1;
    [SerializeField]
    private bool isIdle;
    [SerializeField]
    private Bed bed;
    private bool isSleeping;


    [SerializeField]
    private PointOfPatrol pointOfPatrol;
    public PointOfPatrol PointOfPatrol { get { return pointOfPatrol; } set { pointOfPatrol = value; } }

    [SerializeField]
    private PlayerLogic player;
    public PlayerLogic Player { get { return player; } set { player = value; } }
    #endregion

    public bool IsSleeping()
    {
        return isSleeping;
    }

    private float viewDistance;
    [SerializeField]
    private FieldOfView fieldOfView;

    private NavMeshAgent agent;
    private void Awake()
    {
        isSleeping = bed != null;

    }

    /* To Do
     * Player Spotted coroutine and prevCoroutines
     * -----------------------------------------------
     * 
     * 
     * 
     * 
     * 
     * */

    private void Start()
    {
        EventSystem.Instance.OnStopGame += ProcessStopGame;
        EventSystem.Instance.OnContinueGame += ProcessContinueGame;


        EventSystem.Instance.OnVictoryObjectPickedUp += TreasurePickedUp;        
        EventSystem.Instance.OnPlayerCaptured += PlayerCaptured;
        EventSystem.Instance.OnPlayerSpotted += PlayerSpotted;


        // No use Of StartCoroutine manually, properties XRunning should handle it'

        PrevCoroutinesData.Add("patrol", IsPatrolCoroutineRunning);
        PrevCoroutinesData.Add("wait", IsWaitCoroutineRunning.Item1);
        PrevCoroutinesData.Add("visible", shouldCheckPlayerVisibility);
        PrevCoroutinesData.Add("chase", IsPlayerChasingCoroutineRunning);

        viewDistance = transform.GetChild(1).GetComponent<FieldOfView>().ViewDistance;

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();


        if (isSleeping)
        {
            animationState = AnimationState.Sleeping;
            agent.enabled = false;
            transform.rotation = Quaternion.Euler(bed.getRotationForSleeping());
            transform.position = bed.getPositionForSleeping();
            //Debug.Log("Is it Ok?");
        }
        else if (!isIdle)
        {
            IsPatrolCoroutineRunning = (pointOfPatrol != null) ? true : false;
            if (pointOfPatrol.shouldWaitHere)
            {
                IsWaitCoroutineRunning = (true, pointOfPatrol.Seconds);
            }
        }
        //Debug.Log("Initialize listen");

    }

    #region PatrolCoroutine
    private IEnumerator patrolCoroutine;
    private bool isPatrolCoroutineRunning = false;
    private bool IsPatrolCoroutineRunning
    {
        get { return isPatrolCoroutineRunning; }
        set
        {
            if (value)
            {
                if (!isPatrolCoroutineRunning)
                {
                    isPatrolCoroutineRunning = true;
                    patrolCoroutine = PatrolCoroutine();
                    StartCoroutine(patrolCoroutine);
                }
            }
            else
            {
                if (isPatrolCoroutineRunning)
                {
                    isPatrolCoroutineRunning = false;
                    StopCoroutine(patrolCoroutine);
                }
            }
        }
    }
    IEnumerator PatrolCoroutine()
    {
        while (pointOfPatrol.NextPoint != null)
        {
            if (!IsWaitCoroutineRunning.Item1)
            {
                agent.SetDestination(pointOfPatrol.Position);
                if (animationState != AnimationState.Walking)
                {
                    animationState = AnimationState.Walking;
                }
                while (agent.pathPending)
                {
                    yield return null;
                }
                float condition = Vector3.Distance(transform.position, agent.destination);
                while (Vector3.Distance(transform.position, agent.destination) > 0.1f)
                {
                    yield return null;
                }
                if (shouldLookAround)
                {
                    animationState = AnimationState.LookingAround;
                } else
                {
                    IsWaitCoroutineRunning = (true, pointOfPatrol.Seconds);
                }
            }
            if (pointOfPatrol.shouldWaitHere)
            {
                animationState = AnimationState.Idle;
            }
            Func<bool> shouldWait = new Func<bool>(() => { return IsWaitCoroutineRunning.Item1; });
            yield return new WaitWhile(shouldWait);
            if (shouldLookAround)
            {
                agent.updateRotation = true;
                shouldLookAround = false;
            }
            if (pointOfPatrol.shouldWaitHere)
            {
                animationState = AnimationState.Walking;
            }
            pointOfPatrol = pointOfPatrol.NextPoint;
        }
        IsPatrolCoroutineRunning = false;
    }
    #endregion

    #region WaitCoroutine
    private IEnumerator waitCoroutine;
    private bool isWaitCoroutineRunning = false;
    private float secondsRemaining = 0;
    private (bool, float) IsWaitCoroutineRunning
    {
        get { return (isWaitCoroutineRunning, secondsRemaining); }
        set
        {
            if (value.Item1)
            {
                if (!isWaitCoroutineRunning)
                {
                    if (value.Item2 > 0)
                    {
                        secondsRemaining = value.Item2;
                    } else if (value.Item2 == 0)
                    {
                        return;
                    }
                    isWaitCoroutineRunning = true;
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                    waitCoroutine = WaitCoroutine();
                    StartCoroutine(waitCoroutine);
                }
            }
            else
            {
                if (isWaitCoroutineRunning)
                {
                    isWaitCoroutineRunning = false;
                    agent.updateRotation = true;
                    agent.updatePosition = true;
                    if (value.Item2 >= 0)
                    {
                        secondsRemaining = value.Item2;
                    }
                    StopCoroutine(waitCoroutine);
                }
            }
        }
    }
    IEnumerator WaitCoroutine()
    {

        while (secondsRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            secondsRemaining--;
        }
        IsWaitCoroutineRunning = (false, 0);
    }
    #endregion

    private Vector3 lastKnownPosPlayer;

    #region PlayerVisibleCoroutine
    bool shouldCheckPlayerVisibility = false; 
    public void ShouldCheckPlayerVisibility(bool arg)
    {
        shouldCheckPlayerVisibility = arg;
        IsPlayerVisibleCoroutineRunning = arg;
        //Debug.Log("started player visible");
    }

    private IEnumerator playerVisibleCoroutine;
    private bool isPlayerVisibleCoroutineRunning = false;
    private bool IsPlayerVisibleCoroutineRunning
    {
        get { return isPlayerVisibleCoroutineRunning; }
        set
        {
            if (value)
            {
                if (!isPlayerVisibleCoroutineRunning)
                {
                    isPlayerVisibleCoroutineRunning = true;
                    playerVisibleCoroutine = PlayerVisibleCoroutine();
                    StartCoroutine(playerVisibleCoroutine);
                }
            }
            else
            {
                if (isPlayerVisibleCoroutineRunning)
                {
                    isPlayerVisibleCoroutineRunning = false;
                    StopCoroutine(playerVisibleCoroutine);
                }
            }
        }
    }
    IEnumerator PlayerVisibleCoroutine()
    {
        while (true)
        {
            if (shouldCheckPlayerVisibility)
            {
                //Debug.Log("Watching for player" + isPlayerVisibleCoroutineRunning);
                Ray ray = new Ray(transform.position, player.Position - transform.position);
                RaycastHit rayHit;
                if (Physics.Raycast(ray, out rayHit, viewDistance))
                {
                    if (rayHit.collider.GetComponent<PlayerLogic>() != null)
                    {
                        animationState = AnimationState.Chasing;
                        lastKnownPosPlayer = player.Position;
                        EventSystem.Instance.PlayerSpotted();
                    }
                }
            }
            yield return null;
        }
    }
    #endregion

    #region PlayerChasingCoroutine
    private IEnumerator playerChasingCoroutine;
    private bool isPlayerChasingCoroutineRunning = false;
    private bool IsPlayerChasingCoroutineRunning
    {
        get { return isPlayerChasingCoroutineRunning; }
        set
        {
            if (value)
            {
                if (!isPlayerChasingCoroutineRunning)
                {
                    playerChasingCoroutine = PlayerChasingCoroutine();
                    isPlayerChasingCoroutineRunning = true;
                    StartCoroutine(playerChasingCoroutine);
                }
            }
            else
            {
                if (isPlayerChasingCoroutineRunning)
                {
                    isPlayerChasingCoroutineRunning = false;
                    StopCoroutine(playerChasingCoroutine);
                }
            }
        }
    }
    IEnumerator PlayerChasingCoroutine()
    {
        while (true)
        {
            Vector3 dif = transform.position - lastKnownPosPlayer;
            if (Math.Abs(dif.x) <= 0.1 && Math.Abs(dif.z) <= 0.1)
            {
                //1Debug.LogWarning("Achieved lastKnownPosition");
                animationState = AnimationState.LookingAround;
                IsPlayerChasingCoroutineRunning = false;
            }
            agent.SetDestination(lastKnownPosPlayer);
            //1Debug.Log("New Destination");
            yield return null;
        }
    }
    #endregion

    private void WakeUp()
    {
        if (isSleeping)
        {
            Vector3 pos = bed.GetPositionForAwake();
            pos.y = 0;
            transform.position = pos;
            agent.enabled = false;
            agent.enabled = true;
            isSleeping = false;
        }
    }

    #region PlayerSpottedCoroutine
    private void PlayerSpotted()
    {
        WakeUp();

        IsPatrolCoroutineRunning = false;
        IsWaitCoroutineRunning = (false, 0);
        IsPlayerVisibleCoroutineRunning = false;
        IsPlayerSpottedCoroutineRunning = true;
        IsPlayerChasingCoroutineRunning = true;

        agent.speed *= 2.5f;
        animationState = AnimationState.Chasing;
    }

    private IEnumerator playerSpottedCoroutine;
    private bool isPlayerSpottedCoroutineRunning = false;
    private bool IsPlayerSpottedCoroutineRunning
    {
        get { return isPlayerSpottedCoroutineRunning; }
        set
        {
            if (value)
            {
                if (!isPlayerSpottedCoroutineRunning)
                {
                    playerSpottedCoroutine = PlayerSpottedCoroutine();
                    isPlayerSpottedCoroutineRunning = true;
                    StartCoroutine(playerSpottedCoroutine);
                }
            }
            else
            {
                if (isPlayerSpottedCoroutineRunning)
                {
                    isPlayerSpottedCoroutineRunning = false;
                    StopCoroutine(playerSpottedCoroutine);
                }
            }
        }
    }

    public bool IsIdle()
    {
        return isIdle;
    }

    IEnumerator PlayerSpottedCoroutine()
    {
        while (true)
        {
            lastKnownPosPlayer = player.Position;
            yield return null;
        }
    }
    #endregion

    
    private void TreasurePickedUp()
    {
        if (isSleeping)
        {
            WakeUp();
        }
        IsPatrolCoroutineRunning = true;
    }

    private void PlayerCaptured()
    {
        //Debug.Log("Some girl captured him(");
        IsPlayerChasingCoroutineRunning = false;
        agent.enabled = false;
        animationState = AnimationState.Capturing;
    }

    #region ProcessingStopContinue
    Dictionary<string, bool> PrevCoroutinesData = new Dictionary<string, bool>(4);
    private void ProcessStopGame()
    {
        agent.isStopped = true;
        animator.speed = 0;

        PrevCoroutinesData["patrol"] = IsPatrolCoroutineRunning;
        IsPatrolCoroutineRunning = false;
        PrevCoroutinesData["wait"] = IsWaitCoroutineRunning.Item1;
        IsWaitCoroutineRunning = (false, -1);
        PrevCoroutinesData["patrol"] = shouldCheckPlayerVisibility;
        shouldCheckPlayerVisibility = false;
        PrevCoroutinesData["chase"] = IsPlayerChasingCoroutineRunning;
        IsPlayerChasingCoroutineRunning = false;
    }
    private void ProcessContinueGame()
    {
        agent.isStopped = false;
        animator.speed = 1;

        IsPatrolCoroutineRunning = PrevCoroutinesData["patrol"];
        IsWaitCoroutineRunning = (PrevCoroutinesData["wait"], -1);
        shouldCheckPlayerVisibility = PrevCoroutinesData["visible"];
        IsPlayerChasingCoroutineRunning = PrevCoroutinesData["chase"];
    }
    #endregion

    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(lastKnownPosPlayer, new Vector3(2, 2, 2));
    }
}

