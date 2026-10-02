using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Damage Protection")]
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float invulnerabilityTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)  
    {
        if (invulnerabilityTimer > 0f) return;

        Debug.Log("Player took " + damage + " damage!");
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        invulnerabilityTimer = invulnerabilityTime;

        if (currentHealth == 0) OnDie();
    }

    private void OnDie()
    {
        playerInput.ToggleScreenLock();

        sceneManager.Instance.LoadGameOver();
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
