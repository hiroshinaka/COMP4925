using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set; }

    private Animator anim;
    private bool dead;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    public Transform spawnPoint;



    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }
    public void TakeDamage(float _damage)
    {
        if (dead) return;
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);
        if (currentHealth > 0)
        {
            anim.SetTrigger("hurt");
        }
        else
        {
            // Death
            dead = true;
            anim.SetTrigger("die");

            if (playerMovement != null)
                playerMovement.enabled = false;


        }
    }
    public void OnDeathAnimationComplete()
    {
        Die();
    }
    public void Die()
    {
        // Teleport to spawn
        if (spawnPoint != null)
            transform.position = spawnPoint.position;

        // Reset velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Reset health and state
        currentHealth = startingHealth;
        dead = false;

        if (playerMovement != null)
            playerMovement.enabled = true;
        anim.Play("Idle");  

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(1);
            Debug.Log("Current Health: " + currentHealth);
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
    public void Kill()
    {
        // Take all remaining health as damage
        TakeDamage(currentHealth);
    }
}
