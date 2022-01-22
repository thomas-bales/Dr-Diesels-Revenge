using UnityEngine;
using UnityEngine.Tilemaps;

public class BulletController : MonoBehaviour
{
    public float speed = 1f;
    public int bulletRicochetAmt = 1;
    public int damagePower = 1;

    public int currentRicochetAmt;
    public bool isMissile = false;

    public ParticleSystem smoke;
    ParticleSystem thisSmoke;
    private void OnEnable()
    {
        EventManager.OnLoadLevel += resetBullets;

        currentRicochetAmt = bulletRicochetAmt;
        thisSmoke = Instantiate(smoke, transform);

        EventManager.PlaySound(isMissile ? "Missile Shot" : "Bullet Shot");
    }

    private void OnDisable()
    {
        EventManager.OnLoadLevel += resetBullets;
    }

    void Update()
    {
        moveBullet();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Destructible Terrain" && isMissile )
        {
            EventManager.PlaySound("Wall Destroyed");
            Vector2 hitPosition = new Vector2(collision.GetContact(0).point.x, collision.GetContact(0).point.y);
            collision.gameObject.GetComponent<Tilemap>().SetTile(collision.gameObject.GetComponent<Tilemap>().WorldToCell(hitPosition), null);
            destroyBullet();
        }
        else if (collision.gameObject.CompareTag("Wall") && currentRicochetAmt > 0)
        {
            transform.up = Vector2.Reflect(transform.up, collision.contacts[0].normal);
            EventManager.PlaySound("Ricochet");
            currentRicochetAmt--;
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.PlayerTakeDamage(damagePower);
            destroyBullet();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EventManager.EnemyTakeDamage(damagePower, collision.gameObject.GetComponent<BaseEnemyStats>());
            destroyBullet();
        }
        else if (collision.gameObject.CompareTag("Bullet"))
            destroyBullet();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && currentRicochetAmt > 0)
        {
            transform.up = Vector2.Reflect(transform.up, collision.contacts[0].normal);
            currentRicochetAmt--;
        }
        destroyBullet();
    }

    void moveBullet()
    {
        transform.Translate(transform.up * speed * Time.deltaTime, Space.World);

        if (transform.position.x < -102 || transform.position.y > 65 || transform.position.x > 102 || transform.position.y < -65)
            destroyBullet();
    }

    void destroyBullet()
    {
        thisSmoke.transform.parent = null;
        var main = thisSmoke.main;
        main.loop = false;
        thisSmoke.GetComponent<ParticleController>().DestroyParticles(5f);

        EventManager.PlaySound("Bullet Destroyed");
        Pooler.Despawn(gameObject);
    }

    void resetBullets()
    {
        //Pooler.Despawn(gameObject);
    }
}
