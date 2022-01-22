using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MobileEnemyAI : BaseEnemyStats
{
    public Transform altTarget;

    //Used in StrafeAroundTarget method for calculating  where to move the altTarget
    [Header("Strafe State Settings")]
    public float strafeRadius = 50f;
    public float strafeBuffer = 10f;
    public float stepSize = 1f;
    public float boxCastSize = 10f;

    bool isGoingClockwise = true;
    bool isPlayerInSight = false;

    bool isMoving = false;
    [HideInInspector] public bool isMovingWater = false;
    bool isPlayingAudio = false;

    public AudioSource moveAudio;
    public AudioSource waterAudio;

    AIDestinationSetter setter;
    AIPath pathAlgo;
    Vector3 direction;
    Vector2 tangentDirection;
    CircleCollider2D altTargetCollider;
    enum States
    {
        HOME,
        STRAFE,
        RETREAT
    }
    States currentState;
    States nextState;

    protected void OnEnable()
    {
        base.OnEnable();
    }

    protected void OnDisable()
    {
        base.OnDisable();
    }
    void Start()
    {
        base.Start();
        setter = GetComponent<AIDestinationSetter>();
        pathAlgo = GetComponent<AIPath>();
        currentState = States.HOME;
        nextState = States.HOME;
        switchTarget(player);

        if (canCrossWater) gameObject.layer = 6;

        
    }

    void Update()
    {
        direction = player.position - transform.position;
        if (isPaused) pathAlgo.maxSpeed = 0;
        else pathAlgo.maxSpeed = speed;

        if (pathAlgo.maxSpeed > 0) isMoving = true;

        
        if (isMovingWater && !isPlayingAudio)
        {
            waterAudio.Play();
            isPlayingAudio = true;
        }
        else if (isMoving && !isPlayingAudio)
        {
            moveAudio.Play();
            isPlayingAudio = true;
        }
        else if (!isMovingWater && !isMoving && isPlayingAudio)
        {
            waterAudio.Pause();
            moveAudio.Pause();
            isPlayingAudio = false;
        }

        //Checks if player is behind a wall or not
        isPlayerInSight = isTargetInSight(player);
        if (isPlayerInSight)
        {
            enemyFocus(gunRotateSpeed, player, gun);
            if (searchForRicochetShot(player, bullet.GetComponent<BulletController>().bulletRicochetAmt))
            {
                shootBullet();
            }
            //Checks if shooting a bullet might hit the player
            else if (isGunFacingTarget(player))
            {
                shootBullet();
            }
        }
        else
        {
            if (!isRandomlyRotating) StartCoroutine(randomlyRotateObject(gunRotateSpeed, gun, 1f));

            if (searchForRicochetShot(player, bullet.GetComponent<BulletController>().bulletRicochetAmt))
            {
                isPaused = true;
                if (isPaused) pathAlgo.maxSpeed = 0;
                shootBullet();
            }
        }

        base.Update();

        switch (nextState)
        {
            case States.HOME:
                homeTowardsTarget();
                break;
            case States.STRAFE:
                strafeAroundTarget();
                break;
            case States.RETREAT:
                retreatFromTarget();
                break;
        }
    }

    #region Movement States
    void homeTowardsTarget()
    {
        //initializes state
        if (switchState(States.HOME))
        {
            switchTarget(player);
            Debug.Log("Switched to Home State.");
        }

        //Checks if enemy should switch states
        if (direction.magnitude < strafeRadius - strafeBuffer)
        {
            nextState = States.RETREAT;
        }
        else if (direction.magnitude >= strafeRadius - strafeBuffer && direction.magnitude <= strafeRadius + strafeBuffer && isPlayerInSight)
        {
            nextState = States.STRAFE;
        }
    }

    void strafeAroundTarget()
    {
        //initializes state
        if (switchState(States.STRAFE))
        {
            switchTarget(altTarget);
            isGoingClockwise = Random.value > 0.5;
            Debug.Log("Switched to Strafe State.");
        }

        //Logic to move altTarget
            //Randomize direction enemy strafes in
        tangentDirection = isGoingClockwise ? new Vector2(-direction.y, direction.x).normalized : new Vector2(direction.y, -direction.x).normalized;

        tangentDirection *= stepSize;
        altTarget.position = (Vector2)(player.position + (-direction.normalized) * strafeRadius) + tangentDirection;

        //If enemy approaches wall, reverse direction enemy strafes in, wall detected with raycasting
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(boxCastSize, boxCastSize), 0, tangentDirection, tangentDirection.magnitude, LayerMask.GetMask("Obstacles"));
        if (hit.collider)
        {
            isGoingClockwise = !isGoingClockwise;
            Debug.Log("About to hit a wall!");
        }
            

        //Checks if enemy should switch states
        if (direction.magnitude > strafeRadius + strafeBuffer || !isPlayerInSight)
        {
            nextState = States.HOME;
        }
        else if (direction.magnitude < strafeRadius - strafeBuffer)
        {
            nextState = States.RETREAT;
        }
    }

    void retreatFromTarget()
    {
        //initializes state
        if (switchState(States.RETREAT))
        {
            switchTarget(altTarget);
            Debug.Log("Switched to Retreat State.");
        }

        //Logic to move altTarget
        altTarget.position = player.position + (-direction.normalized) * strafeRadius;

        //Checks if enemy should switch states
        if (direction.magnitude > strafeRadius + strafeBuffer)
        {
            nextState = States.HOME;
        }
        else if (direction.magnitude >= strafeRadius - strafeBuffer && direction.magnitude <= strafeRadius + strafeBuffer && isPlayerInSight)
        {
            nextState = States.STRAFE;
        }
    }

    #endregion 
    bool switchState(States newState)
    {
        if (newState != currentState)
        {
            currentState = newState;
            return true;
        }
        else return false;   
    }
    public override void gotHacked(BaseEnemyStats enemy)
    {
        pathAlgo.enabled = false;
        base.gotHacked(enemy);
    }
    void switchTarget(Transform target)
    {
        setter.target = target;
    }
}
