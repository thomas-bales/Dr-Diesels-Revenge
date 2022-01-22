using System.Collections;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine;

public class BaseEnemyStats : MonoBehaviour
{
    public int maxHealth = 3;
    public int strength = 1;
    public float speed = 1;
    public int bulletSpeed = 1;
    public float bulletCooldownTime = 2f;
    public float shootPauseLength = 0.2f;
    public float gunRotateSpeed = 20f;
    public float barrelLength = 5f;

    //Used in the raycast of the isTargetInSight method;
    public float tankRadius = 5f;

    public bool canCrossWater;

    public Transform healthBar;
    public GameObject bullet;
    public GameObject grave;
    public Transform glowLight;

    protected bool isPaused = false;
    protected bool isRandomlyRotating = false;
    bool isCoolingDown = false;
    bool isDead = false;
     public int currentHealth;

    protected Transform player;
    protected Transform gun;

    protected void OnEnable()
    {
        EventManager.OnEnemyTakeDamage += takeDamage;
        EventManager.OnSuccessHack += gotHacked;
        EventManager.OnPlayerUpdate += setPlayerTarget;
    }

    protected void OnDisable()
    {
        EventManager.OnEnemyTakeDamage -= takeDamage;
        EventManager.OnSuccessHack -= gotHacked;
        EventManager.OnPlayerUpdate -= setPlayerTarget;
    }
    protected void Start()
    {
        //Grabs a reference to the "player body" object w/out having to look for it in the inspector.
        //In order for inherited classes to do this, call the "base.Start()" method within the new class's own "Start()" method.
        //"player" is now protected, since we no longer need to see it in the inspector, but derived classes should inherit it.
        //player = FindObjectOfType<PlayerController>().transform;

        //Because tanks are made up of two seperate game objects, but we use one script to control them (attached to the "Body" part) we need a reference to the gun
        //This grabs a reference to the respective gun of each tank, which you can use to rotate it when necessary
        gun = transform.parent.GetChild(1);

        player = GameObject.Find("Player").transform.GetChild(0);

        currentHealth = maxHealth;
    }
    protected void Update()
    {
        //keeps gun attached to the body
        gun.position = transform.position;

        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0f;

        //Checks if enemy died
        if (currentHealth <= 0 && !isDead)
        {
            currentHealth = 0;
            isDead = true;
            enemyDeath();
        }
            
    }

    //I modified this script to be more customizable. When you call this function, the first parameter will be the target you want to point to.
    //Normally, this would just be "player". The second parameter will be the object you want to rotate to face the target.
    //This is useful if you want to rotate the gun, but not the body.
    //rotate speed is in degrees/second
    //Add this method to your update function if you want to use it
    protected void enemyFocus(float rotateSpeed, Transform target, Transform objectThatWillRotate)
    {
        // Cancels the randomly rotating method, if active
        isRandomlyRotating = false;

        // Determine which direction to rotate towards
        Vector3 targetDirection = (target.position - objectThatWillRotate.position).normalized;

        // The step size is equal to speed times frame time.
        var step = rotateSpeed * Time.deltaTime;

        // Rotate our transform a step closer to the target's.
        float angleToMove = Vector3.SignedAngle(objectThatWillRotate.up, targetDirection, objectThatWillRotate.forward);

        Quaternion targetRotation = Quaternion.Euler(0, 0, objectThatWillRotate.rotation.eulerAngles.z + angleToMove);

        objectThatWillRotate.rotation = Quaternion.RotateTowards(objectThatWillRotate.rotation, targetRotation, step);
    }

    //This method causes the object to randomly rotate.
    //Use the StartCoroutine() method to call this function.
    //If calling w/in the Update() method, don't call this function unless isRandomlyRotating is equal to false. 
    protected IEnumerator randomlyRotateObject(float rotateSpeed, Transform objectThatWillRotate, float minArcLength, float maxArcLength = 10f, float buffer = 0.5f)
    {
        isRandomlyRotating = true;

        // Determine which direction to rotate towards
        Vector2 targetDirection = new Vector2(objectThatWillRotate.up.x, objectThatWillRotate.up.y);
        Vector2 tangentDirection = (Random.value > 0.5) ? new Vector2(-targetDirection.y, targetDirection.x).normalized : new Vector2(targetDirection.y, -targetDirection.x).normalized;
        targetDirection = (targetDirection + tangentDirection * Random.Range(minArcLength, maxArcLength)).normalized;

        while (((Vector2)objectThatWillRotate.up.normalized - targetDirection).magnitude > 0.5)
        {
            if (isRandomlyRotating == false)
            {
                isPaused = false;
                break;
            }

            // The step size is equal to speed times frame time.
            var step = rotateSpeed * Time.deltaTime;

            // Rotate our transform a step closer to the target's.
            float angleToMove = Vector3.SignedAngle(objectThatWillRotate.up, targetDirection, objectThatWillRotate.forward);

            Quaternion targetRotation = Quaternion.Euler(0, 0, objectThatWillRotate.rotation.eulerAngles.z + angleToMove);

            objectThatWillRotate.rotation = Quaternion.RotateTowards(objectThatWillRotate.rotation, targetRotation, step);

            if (isPaused && !isCoolingDown)
            {
                yield return new WaitForSeconds(shootPauseLength);
                isPaused = false;
            } 
            else yield return null;
        }

        isRandomlyRotating = false;
    }

    //This script returns true if there is a clear unobstructed path between the enemy and the target.
    //Useful for finding out if the target is behind a wall or another tank
    protected bool isTargetInSight(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + direction * tankRadius, direction, Mathf.Infinity, ~LayerMask.GetMask("Water"));

        Debug.Log(hit.transform.gameObject.name);

        if (hit.transform == target)
        {
            return true;
        } 
        else return false;
    }

    //Returns true if gun is facing target.
    //Useful deciding if the gun should shoot or not.
    protected bool isGunFacingTarget(Transform target)
    {
        Vector2 direction = gun.up.normalized;
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + direction * tankRadius, direction, Mathf.Infinity, ~LayerMask.GetMask("Water"));

        if (hit.transform == target)
        {
            return true;
        }
        else return false;
    }

    //Returns true if the gun has a ricochet shot in the direction it's facing that will hit the target.
    //numOfRicochest should be the number of ricochets the enemy's bullet has.
    //NOTE: this method should be put in the Update function, but a Coroutine may have better performance.
    protected bool searchForRicochetShot(Transform target, int numOfRicochets = 1)
    {
        Vector2 rayStart = transform.position + gun.transform.up * tankRadius;

        RaycastHit2D hit = Physics2D.Raycast(rayStart, gun.transform.up, Mathf.Infinity, ~LayerMask.GetMask("Water"));
        Debug.DrawLine(rayStart, hit.point, Color.white);

        if (hit.transform.gameObject.name == target.name)
        {
            return true;
        }

        for (int i = 0; i < numOfRicochets; i++)
        {
            if (hit.transform.gameObject.name != target.gameObject.name)
            {
                Vector2 reflectVector = Vector2.Reflect(hit.point - rayStart, hit.normal);
                RaycastHit2D oldHit = hit;

                hit = Physics2D.Raycast(oldHit.point, reflectVector, Mathf.Infinity, ~LayerMask.GetMask("Water"));
                Debug.DrawLine(oldHit.point, hit.point, Color.white);
            }

            if (hit.transform.gameObject.name == target.name)
            {
                return true;
            }
        }

        if (hit.transform.gameObject.name == target.name)
        {
            return true;
        }
        else return false;
    }

    // Method for shooting a bullet, functionality not yet implemented
    // See gunController script for example implementation
    protected void shootBullet()
    {
        if (!isCoolingDown)
        {
            Pooler.Spawn(bullet, transform.position + gun.up * barrelLength, gun.rotation);
            StartCoroutine(bulletCooldown());
        }
    }

    IEnumerator bulletCooldown()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(bulletCooldownTime);
        isPaused = false;
        isCoolingDown = false;
    }

    // Damage function
    public void takeDamage(int damage, BaseEnemyStats enemy)
    {
        if (enemy == this)
        {
            currentHealth -= damage;
            EventManager.PlaySound("Enemy Take Damage");
        }   
    }

    void setPlayerTarget(PlayerController playerTarget)
    {
        player = playerTarget.transform;
    }

    //Activated when tank is successfully hacked. Causes player to control enemy tank.
    public virtual void gotHacked(BaseEnemyStats enemy)
    {
        if (enemy == this)
        {
            EventManager.PlaySound("Success Hack");

            GetComponent<PlayerController>().enabled = true;
            gun.GetComponent<GunController>().enabled = true;
            healthBar.GetComponent<HealthUI>().enabled = true;

            Color newColor;
            if (ColorUtility.TryParseHtmlString("#00F1F8", out newColor))
                glowLight.GetComponent<Light2D>().color = newColor;
            if (canCrossWater) GetComponent<PlayerController>().canCrossWater = true;
          
            StartCoroutine(gotHackedCoroutine());
        }
    }

    IEnumerator gotHackedCoroutine()
    {
        yield return new WaitForSeconds(0.05f);
        GetComponent<PlayerController>().maxHealth = maxHealth;
        GetComponent<PlayerController>().currentHealth = currentHealth;
        healthBar.GetComponent<HealthUI>().maxScale = healthBar.GetComponent<EnemyHealthUI>().maxScale;
        gameObject.tag = "Player";

        healthBar.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.235f, 0.314f, 0.679f, 1.000f);
        healthBar.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1.000f, 0.971f, 0.042f, 1.000f);

        EventManager.DestroyEnemyTank();

        healthBar.GetComponent<EnemyHealthUI>().enabled = false;
        this.enabled = false;
    }

    public void enemyDeath()
    {
        EventManager.PlaySound("Tank Destroyed");
        EventManager.DestroyEnemyTank();

        Transform graveMarker = Instantiate(grave).transform;
        graveMarker.position = transform.position;

        transform.parent.gameObject.SetActive(false);
    }
}
