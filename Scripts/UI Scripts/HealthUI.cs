using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public Vector2 offset = new Vector2(0f, 7.3f);
    public Transform body;
    public Transform healthBar;

    PlayerController thisPlayer;

    public float maxHealth;
    public float currentHealth;
    [HideInInspector] public float maxScale;
    float currentScale;

    [HideInInspector] public Color bGroundColor;
    [HideInInspector] public Color fGroundColor;

    private void Awake()
    {
        bGroundColor = transform.GetChild(0).GetComponent<SpriteRenderer>().color;
        fGroundColor = transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color;

        Debug.Log(bGroundColor);
        Debug.Log(fGroundColor);
    }
    private void Start()
    {
        thisPlayer = body.GetComponent<PlayerController>();
        maxHealth = thisPlayer.maxHealth;
        maxScale = healthBar.localScale.x;
    }
    void Update()
    {
        transform.position = body.position + (Vector3)offset;

        updateHealthDisplay();
    }

    void updateHealthDisplay()
    {
        currentHealth = thisPlayer.currentHealth;

        //calculate scale based on original scale and the ratio of current health to maxHealth
        if (currentHealth < 0) currentHealth = 0;

        currentScale = currentHealth / maxHealth * maxScale;
        healthBar.localScale = new Vector3(currentScale, healthBar.localScale.y, healthBar.localScale.z);
    }
}
