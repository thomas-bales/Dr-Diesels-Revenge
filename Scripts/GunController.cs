using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    public Transform player;
    public GameObject bullet;
    public int bulletAmt = 5;
    public float barrelLength = 1f;
    void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
            return; // No gamepad connected.

        transform.position = player.position;
        aimGun(gamepad);

        if (gamepad.rightShoulder.wasPressedThisFrame)
            shootBullet();
    }

    void aimGun(Gamepad gamepad)
    {
        Vector2 move = gamepad.rightStick.ReadValue();

        if (move != Vector2.zero)
        {
            transform.up = move.normalized;
        }
    }

    void shootBullet()
    {
        Pooler.Spawn(bullet, transform.position + transform.up * barrelLength, transform.rotation, bulletAmt);
    }
}
