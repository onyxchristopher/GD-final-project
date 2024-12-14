using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private float speed = 15;
    private int damage = 4;
    
    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start() {
        rb.velocity = speed * transform.right;
        Destroy(gameObject, 5);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            EventManager.PlayerDamage(damage);
            Destroy(gameObject);
        }
        if (!other.isTrigger || other.tag == "Blade") {
            Destroy(gameObject);
        }
    }
}
