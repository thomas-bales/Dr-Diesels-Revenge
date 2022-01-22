using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthUI : MonoBehaviour
{
    public Vector2 offset;
    public Transform body;
    public Transform healthBar;
    public Transform staminaBar;

    BaseEnemyStats thisEnemy;
    
    float maxHealth;
    float currentHealth;
    float currentStamina = 0;
    [HideInInspector] public float maxScale;
    float currentScale;

    private void OnEnable()
    {
        EventManager.OnAttemptHack += updateStaminaDisplay;
    }

    private void OnDisable()
    {
        EventManager.OnAttemptHack -= updateStaminaDisplay;

        staminaBar.localScale = new Vector3(0, staminaBar.localScale.y, staminaBar.localScale.z);
    }

    private void Start()
    {
        thisEnemy = body.GetComponent<BaseEnemyStats>();
        maxHealth = thisEnemy.maxHealth;
        maxScale = healthBar.localScale.x;

    }
    void Update()
    {
        transform.position = body.position + (Vector3)offset;

        updateHealthDisplay();
    }

    void updateHealthDisplay()
    {
        currentHealth = thisEnemy.currentHealth;

        //calculate scale based on original scale and the ratio of current health to maxHealth
        if (currentHealth < 0) currentHealth = 0;
        currentScale = currentHealth / maxHealth * maxScale;
        
        healthBar.localScale = new Vector3(currentScale, healthBar.localScale.y, healthBar.localScale.z);

    }

    void updateStaminaDisplay(BaseEnemyStats enemy)
    {
        if (enemy == thisEnemy && currentStamina != currentHealth)
        {
            currentStamina++;
            if (currentStamina > currentHealth) currentStamina = currentHealth;
            currentScale = currentStamina / maxHealth * maxScale;

            staminaBar.localScale = new Vector3(currentScale, staminaBar.localScale.y, staminaBar.localScale.z);

            if (currentStamina == currentHealth)
            {
                tankGotHacked(thisEnemy);
            }
            else EventManager.PlaySound("Attempt Hack");
        }
    }

    void tankGotHacked(BaseEnemyStats enemy)
    {
        EventManager.SuccessHack(enemy);
    }
}
