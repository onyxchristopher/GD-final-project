using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidchargerBackEnemy : MonoBehaviour
{
    private Damageable bossHealth;
    
    void Start() {
        bossHealth = transform.parent.gameObject.GetComponent<Damageable>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Blade")) {
            bossHealth.Damage(other.GetComponent<Blade>().GetDamage());
        }
    }
}
