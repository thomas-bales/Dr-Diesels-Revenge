using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 3;
    public float speed = 1f;
    public float personSpeed = 1f;
    public float dashSpeed = 10f;
    public float dashTime = 1f;
    public float dashBufferTime = 0.1f;
    public float damageBufferTime = 0.1f;

    [SerializeField] Animator playerAnim;

    public GameObject gun;
    public GameObject grave;
    public Rigidbody2D rb;
    public GameObject healthBar;
    public AudioSource moveAudio;
    public AudioSource waterAudio;

    private Sprite tankSprite;
    private SpriteRenderer spriteRenderer;

    private Vector2 move;
    private const string tank = "Tank";
    private const string person = "Person";
    private string state = tank;
    private bool isDashing = false;
    private bool isRechargingDash = false;
    private bool hasCollidedwEnemy = false;
    private bool isImmune = false;

    private bool isMoving = false;
     public bool isMovingWater = false;
    private bool isPlayingAudio = false;

    [HideInInspector] public int currentHealth;
     public bool canCrossWater;

    private void OnEnable()
    {
        EventManager.OnPlayerTakeDamage += takeDamage;
        EventManager.OnSuccessHack += switchToHackedTank;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerTakeDamage -= takeDamage;
        EventManager.OnSuccessHack -= switchToHackedTank;
    }

    private void Start()
    {
        spriteRenderer = transform.GetComponent<SpriteRenderer>();
        tankSprite = spriteRenderer.sprite;
        currentHealth = maxHealth;

        EventManager.PlayerUpdate(this);
        rb.velocity = Vector3.zero;

        if (canCrossWater) gameObject.layer = 6;
    }
    void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return; // No gamepad connected.

        if (rb.velocity != Vector2.zero && !isDashing) rb.velocity = Vector2.zero;

        //Movement
        if (state == tank)
        {
            movePlayerTank(gamepad);
            
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
        }
        else if (state == person && !isDashing)
        {
            waterAudio.Pause();
            moveAudio.Pause();
            isPlayingAudio = false;
            movePlayerPerson(gamepad);
        }

        //Stop rotation when in person mode
        if (state == person)
            transform.up = new Vector2(0, 1);

        //For testing only!!!
        if (gamepad.rightTrigger.wasPressedThisFrame && state == tank)
            destroyTank();
        else if (gamepad.rightTrigger.wasPressedThisFrame && state == person)
            switchToTank();

        //Dash Mechanic
        if (gamepad.rightShoulder.wasPressedThisFrame && state == person && !isRechargingDash)
        {
            StartCoroutine(dashPerson());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Checks if dash collided w/ enemy
        if (isDashing && !hasCollidedwEnemy && collision.gameObject.CompareTag("Enemy"))
        {
            attemptHackTank(collision.gameObject.GetComponent<BaseEnemyStats>());
            
        }
    }

    void movePlayerTank(Gamepad gamepad)
    {
        move = gamepad.leftStick.ReadValue();

        if (move != Vector2.zero)
        {
            playerAnim.SetBool("isMoving", true);
            isMoving = true;

            transform.up = move.normalized;
            transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
        }
        else
        {
            playerAnim.SetBool("isMoving", false);
            isMoving = false;
        }
            
    }

    void movePlayerPerson(Gamepad gamepad)
    {
        move = gamepad.leftStick.ReadValue();

        if (move != Vector2.zero)
        {
            playerAnim.SetFloat("Horizontal", move.x);
            playerAnim.SetFloat("Vertical", move.y);

            transform.Translate(move.normalized * personSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            playerAnim.SetFloat("Horizontal", 0);
            playerAnim.SetFloat("Vertical", 0);
        }
    }

    IEnumerator dashPerson()
    {
        isDashing = true;
        EventManager.PlaySound("Dash");
        isRechargingDash = true;
        rb.freezeRotation = true;
        rb.velocity = move.normalized * dashSpeed;
        playerAnim.SetBool("isDash", true);

        yield return new WaitForSeconds(dashTime);

        rb.velocity = Vector2.zero;
        rb.freezeRotation = false;
        isDashing = false;
        hasCollidedwEnemy = false;
        playerAnim.SetBool("isDash", false);

        yield return new WaitForSeconds(dashBufferTime);

        isRechargingDash = false;
    }

    void destroyTank()
    {
     
        //Switch animations
        playerAnim.SetBool("isTankDestroyed", true);
        isPlayingAudio = false;

        //Disable Gun
        gun.SetActive(false);

        Transform graveMarker = Instantiate(grave).transform;
        graveMarker.position = transform.position;

        transform.up = Vector2.up;

        healthBar.SetActive(false);
        state = person;
        EventManager.PlaySound("Tank Destroyed");
        EventManager.DestroyPlayerTank();
    }

    void attemptHackTank(BaseEnemyStats enemy)
    {
        EventManager.AttemptHack(enemy);
    }

    //For testing purposes only!!!
    void switchToTank()
    {
        //Enable Gun
        gun.SetActive(true);

        //Switch back to Player Sprite
        spriteRenderer.sprite = tankSprite;

        state = tank;
    }

    void takeDamage(int damage)
    {
        if (!isImmune)
        {
            StartCoroutine(takeDamageBuffer(damage));
            
        }
            
    }

    IEnumerator takeDamageBuffer(int damage)
    {
        isImmune = true;
        currentHealth -= damage;
        if (currentHealth <= 0 && state == tank)
        {
            currentHealth = 1;
            destroyTank();
        }
        else if (currentHealth <= 0 && state == person)
        {
            currentHealth = 0;
            playerDeath();
        }
        else EventManager.PlaySound("Player Take Damage");

        yield return new WaitForSeconds(damageBufferTime);

        isImmune = false;
    }

    void switchToHackedTank(BaseEnemyStats enemy)
    {
        transform.parent.gameObject.SetActive(false);
    }

    void playerDeath()
    {

    }
}
