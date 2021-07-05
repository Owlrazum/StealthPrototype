using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class PlayerLogic : MonoBehaviour
{
    #region Animaions
    private Animator animator;
    public enum AnimationState
    {
        Idle,
        IdleSneak,
        Crouching,
        Running,
        Trapped,
        Hiding,
        JumpingOver
    };
    private AnimationState animationStateData;
    private AnimationState animationState { 
        get { return animationStateData; }
        set 
        {
            animationStateData = value;
            switch (animationStateData)
            {
                case AnimationState.Idle:
                    animator.CrossFade("Base Layer.Player_Idle", 0.1f);
                    break;
                case AnimationState.IdleSneak:
                    animator.CrossFade("Base Layer.Idle_Sneak", 0.1f);
                    break;
                case AnimationState.Crouching:
                    animator.CrossFade("Base Layer.Crouch", 0.1f);
                    //  animator.get (false);
                    break;
                case AnimationState.Running:
                    animator.CrossFade("Base Layer.Run", 0.1f);
                    break;
                case AnimationState.Trapped:
                    animator.CrossFade("Base Layer.Player_Idle", 0.1f);
                    break;
                case AnimationState.JumpingOver:
                    animator.CrossFade("Base Layer.JumpOver", 0.1f);
                    break;
            }
        } 
    }

    #endregion
    [SerializeField]
    private float editorSpeed = 3;
    [SerializeField]
    private float angularSpeed = 100;
    [SerializeField]
    private float avoidanceRadius = 0.3f;
    [SerializeField]
    private float cameraRotation = 0;

    public Vector3 Position { get { return transform.position; } private set { } }
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); 

        EventSystem.Instance.OnStopGame += ProcessStopGame;
        EventSystem.Instance.OnContinueGame += ProcessContinueGame;

        EventSystem.Instance.OnCameraRotationStarted += CameraRotationStarted;
        EventSystem.Instance.OnCameraRotationCompleted += CameraRotationCompleted;

        EventSystem.Instance.OnTrapEntered += EnteredTrap;
        EventSystem.Instance.OnShortcutEntered += EnteredShortcut;
        EventSystem.Instance.OnShortcutExitted += ExittedShortcut;

        EventSystem.Instance.OnEnteredVictoryZone += EnteredVictoryZone;
        EventSystem.Instance.OnPlayerSpotted += Spotted;
        EventSystem.Instance.OnPlayerCaptured += Captured;

        initialForward = transform.forward;
        IsMoveCoroutineRunning = true;
    }

    private void CameraRotationStarted(int rot)
    {
        cameraRotation += rot;
        IsMoveCoroutineRunning = false;
        animationState = isInSneakMode ? AnimationState.IdleSneak : AnimationState.Running;
    }
    private void CameraRotationCompleted()
    {
        IsMoveCoroutineRunning = true;
    }

    #region MoveProcessing
    bool isInSneakMode = true;

    private IEnumerator moveCoroutine;
    private bool isMoveCoroutineRunning = false;
    private bool IsMoveCoroutineRunning
    {
        get { return isMoveCoroutineRunning; }
        set
        {
            if (value)
            {
                if (!isMoveCoroutineRunning)
                {
                    isMoveCoroutineRunning = true;
                    moveCoroutine = MoveCoroutine();
                    StartCoroutine(moveCoroutine);
                }
            }
            else
            {
                if (isMoveCoroutineRunning)
                {
                    isMoveCoroutineRunning = false;
                    StopCoroutine(moveCoroutine);
                }
            }
        }
    }
    IEnumerator MoveCoroutine()
    {
        while (true)
        {
            float moveX, moveZ;
            (moveX, moveZ) = PlayerInput.current.GetMoveData();
            bool isMoving = Mathf.Abs(moveX) > 0.001 || Mathf.Abs(moveZ) > 0.001;
            Vector3 directionOfMove = Quaternion.Euler(0, cameraRotation, 0) * (new Vector3(moveX, 0, moveZ)).normalized;
            //Debug.Log("Movinf Coroutine" + directionOfMove);
            if (isMoving)
            {
                if (isInSneakMode)
                {
                    if (animationState != AnimationState.Crouching)
                    {
                        animationState = AnimationState.Crouching;
                    }
                    Move(directionOfMove, editorSpeed);
                }
                else
                {
                    if (animationState != AnimationState.Running)
                    {
                        animationState = AnimationState.Running;
                    }
                    Move(directionOfMove, editorSpeed * 1.5f);
                }
            }
            else
            {
                if (isInSneakMode)
                {
                    if (animationState != AnimationState.IdleSneak)
                    {
                        animationState = AnimationState.IdleSneak;
                    }
                }
                else if (animationState != AnimationState.Idle)
                {
                    animationState = AnimationState.Idle;
                }
            }
            yield return null;
        }

    }

    private Vector3 initialForward;
    private Vector3 rotationDirection;
    bool isRotating = false;
    private void Move(Vector3 direction, float speed)
    {
        Vector3 movement = direction * Time.deltaTime * speed;
        transform.Translate(movement, Space.World);
        //Debug.Log("Moved " + moveX + " " + moveZ);
        if (isRotating == false)
        {
            if (rotationDirection != direction)
            {
                isRotating = true;
                rotationDirection = direction;
                RotateTo(initialForward, rotationDirection, true);
            } 
        }
        else
        {
            RotateTo(initialForward, rotationDirection, true);
        }
    }
    void RotateTo(Vector3 from, Vector3 to, bool isGradual = false)
    {
        float maxDegreesDelta = angularSpeed * Time.deltaTime * 100000;
        float angle = Vector3.SignedAngle(from, to, transform.up);

        if (!isGradual)
        {
            Debug.Log(angle);
        }
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
        if (isGradual)
        {
            transform.rotation =
            //targetRotation;
                Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
        } else
        {
            transform.rotation *= targetRotation;
        }


        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1)
        {
            isRotating = false;
        }
    }
    #endregion

    #region TrapProcessing
    private void EnteredTrap(float secondsDelay)
    {
        IsTrappedCoroutineRunning = (true, secondsDelay);
    }
    private IEnumerator trappedCoroutine;
    private bool isTrappedCoroutineRunning = false;
    private float secondsLeft = 0;
    private (bool, float) IsTrappedCoroutineRunning
    {
        get { return (isTrappedCoroutineRunning, secondsLeft); }
        set
        {
            if (value.Item1)
            {
                if (!isTrappedCoroutineRunning)
                {
                    isTrappedCoroutineRunning = true;
                    if (value.Item2 >= 0)
                    {
                        secondsLeft = value.Item2;
                    }
                    trappedCoroutine = TrappedCoroutine();
                    StartCoroutine(trappedCoroutine);
                }
            }
            else
            {
                if (isTrappedCoroutineRunning)
                {
                    isTrappedCoroutineRunning = false;
                    if (value.Item2 >= 0)
                    {
                        secondsLeft = value.Item2;
                    }
                    StopCoroutine(trappedCoroutine);
                }
            }
        }
    }
    IEnumerator TrappedCoroutine()
    {
        if (animationState != AnimationState.Trapped)
        {
            animationState = AnimationState.Trapped;
        }
        while (secondsLeft > 0)
        {
            IsMoveCoroutineRunning = false;
            secondsLeft--;
            yield return new WaitForSeconds(1);
        }
        IsMoveCoroutineRunning = true;
        animationState = AnimationState.Idle;
        IsTrappedCoroutineRunning = (false, 0);
    }
    #endregion

    #region ShortcutProcessing


    private Vector3 shortcutTranslation;
    private float targetY = -1;
    private bool isTopAchieved = false;
    private void EnteredShortcut(Vector3 translation)
    {
        //Debug.Log("translation " + translation);
        //state = State.Shortcutting;

        shortcutTranslation = translation;
        animationState = AnimationState.JumpingOver;
        IsShortcutCoroutineRunning = true;
        IsMoveCoroutineRunning = false;
        agent.enabled = false;
        //Debug.Log("Shortcut pos " + targetPosition);
    }
    private void ExittedShortcut(float landY)
    {
        //Debug.Log();
        targetY = landY;
        isTopAchieved = true;
    }

    private IEnumerator shortcutCoroutine;
    private bool isShortcutCoroutineRunning = false;
    private bool IsShortcutCoroutineRunning
    {
        get { return isShortcutCoroutineRunning; }
        set
        {
            if (value)
            {
                if (!isShortcutCoroutineRunning)
                {
                    isShortcutCoroutineRunning = true;
                    shortcutCoroutine = ShortcutCoroutine();
                    StartCoroutine(shortcutCoroutine);
                }
            }
            else
            {
                if (isShortcutCoroutineRunning)
                {
                    isShortcutCoroutineRunning = false;
                    StopCoroutine(shortcutCoroutine);
                }
            }
        }
    }
    IEnumerator ShortcutCoroutine()
    {
        RotateTo(transform.forward, shortcutTranslation.normalized);

        float animationProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationProgress %= 1;
        Debug.Log(animationProgress);
        float distPassed = editorSpeed * Time.deltaTime;
        
        while (!isTopAchieved)
        {
            transform.Translate(shortcutTranslation.normalized * 0.7f * distPassed, Space.World);
            yield return null;
        }

        animationProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationProgress %= 1;
        Debug.Log(animationProgress);

        shortcutTranslation.y = 0;
        float distanceLeftRatio = (1 - animationProgress) / animationProgress;
        shortcutTranslation *= distanceLeftRatio;
        shortcutTranslation.y = targetY;

        while (animationProgress < 0.99)
        {
            animationProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            animationProgress %= 1;
            Debug.Log(animationProgress);
            transform.Translate(shortcutTranslation.normalized * 0.7f * distPassed, Space.World);
            yield return null;
        }
        animationState = AnimationState.Running;
        float timer = 0;
        EventSystem.Instance.PlayerRunnedAway();
        while (timer < 100)
        {
            timer += 0.001f;
            transform.Translate(shortcutTranslation.normalized * distPassed, Space.World);
            yield return null;
        }
    }
    #endregion

    private void EnteredVictoryZone()
    {
        IsMoveCoroutineRunning = false;
        animationState = AnimationState.JumpingOver;
        //IsMoveCoroutineRunning = false;
    }

    void Spotted ()
    {
        isInSneakMode = false;
    }

    void Captured()
    {
        //IsMoveCoroutineRunning = false;
    }
    private void ProcessStopGame()
    {
        IsMoveCoroutineRunning = false;
        IsTrappedCoroutineRunning = (false, -1); // -1 should mean do not change seconds left in the trap;
        animator.speed = 0;
    }
    private void ProcessContinueGame() 
    {
        IsMoveCoroutineRunning = true;
        IsTrappedCoroutineRunning = (true, -1);
        animator.speed = 1;
    }


    /* 
     Draft

NavMeshHit navHit;
        agent.FindClosestEdge(out navHit);
        Vector3 posOfClosestNavMeshBorder = navHit.position;
        Vector3 vectorToBorder = posOfClosestNavMeshBorder - transform.position;
        Vector2 normalAtBorder = new Vector2 (navHit.normal.normalized.x, navHit.normal.normalized.z);

        float borderX = vectorToBorder.x;
        float borderZ = vectorToBorder.z;

        string message = "Direction: " + dir;

        bool isCloseToTheBorder = Math.Abs(borderX) < agent.radius / 2 && Math.Abs(borderZ) < agent.radius / 2;
        message += "\nisCloseToTheBorder " + isCloseToTheBorder;
        if (isCloseToTheBorder)
        {
            float angle = Vector2.SignedAngle(normalAtBorder, dir);
            float testAngle = Vector2.Angle(normalAtBorder, dir);
            message += "\nNormal " + normalAtBorder;
            message += "\nAngle " + angle;
            message += "\nTestAngle " + testAngle;
            //Debug.Log(message);
            if (angle > 120)
            {
                return true;
                dir = new Vector2(normalAtBorder.x, -normalAtBorder.y);
                //Debug.LogWarning("Dir " + dir);
            } else if (angle < -120)
            {
                return true;
                dir = new Vector2(-normalAtBorder.x, normalAtBorder.y);
                //Debug.LogWarning("Dir " + dir);
            }
        }
        return false;     
     
     */
}
