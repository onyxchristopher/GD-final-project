using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // the projectile's rigidbody
    private Rigidbody2D rb;
    // the projectile's speed
    [SerializeField] private float speed = 15;
    // the projectile's damage to the player
    [SerializeField] private int damage = 4;
    // how much faster the projectile is when reflected
    [SerializeField] private float reflectScaling = 2;

    private Rigidbody2D playerRB;
    
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        EventManager.ProjectileFire();
        rb.velocity = speed * transform.right;
        Destroy(gameObject, 5);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            EventManager.PlayerDamage(damage);
            Destroy(gameObject);
        } else if (other.CompareTag("Shield")) {
            playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
            Vector2 diffVector = (rb.position - playerRB.position).normalized;
            rb.velocity = reflectScaling * speed * diffVector;
            gameObject.layer = LayerMask.NameToLayer("Attack");
            EventManager.ShieldReflect();
            GetComponent<SpriteRenderer>().color = Color.cyan;
        } else if (!other.isTrigger && other.CompareTag("Damageable")) {
            other.gameObject.GetComponent<Damageable>().Damage(Mathf.RoundToInt(reflectScaling * damage));
            Destroy(gameObject);
        } else if (!other.isTrigger) {
            Destroy(gameObject);
        }
    }
}
