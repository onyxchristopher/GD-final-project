using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private int damage;

    public void SetDamage(int d) {
        damage = d;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Damageable") {
            if (!other.isTrigger) {
                other.gameObject.GetComponent<Damageable>().Damage(damage);
                Destroy(gameObject);
            }
        }
    }
}
