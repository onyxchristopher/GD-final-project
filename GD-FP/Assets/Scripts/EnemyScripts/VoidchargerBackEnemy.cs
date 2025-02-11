using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidchargerBackEnemy : MonoBehaviour
{
    private Damageable bossHealth;
    // Start is called before the first frame update
    void Start()
    {
        bossHealth = transform.parent.gameObject.GetComponent<Damageable>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Blade") {
            bossHealth.Damage(other.GetComponent<Blade>().GetDamage());
        }
    }
}
