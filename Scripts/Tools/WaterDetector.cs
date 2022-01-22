using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDetector : MonoBehaviour
{
    PlayerController player;
    MobileEnemyAI enemy;
    void Start()
    {
        transform.parent.GetChild(0).TryGetComponent(out MobileEnemyAI enemy);
        player = transform.parent.GetChild(0).GetComponent<PlayerController>();

        if (enemy) Debug.Log(enemy.gameObject.name);
        Debug.Log(player.gameObject.name);
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Entered Something");
        if (collision.gameObject.layer == 4)
        {
            if (enemy) enemy.isMovingWater = true;
            player.isMovingWater = true;

            Debug.Log("Entered Water!");
        }

        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exited Something!");
        if (collision.gameObject.layer == 4)
        {
            if (enemy) enemy.isMovingWater = true;
            player.isMovingWater = true;

            Debug.Log("Exited Water!");
        }
    }
}
