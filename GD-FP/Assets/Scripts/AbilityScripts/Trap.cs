using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private int damage;
    private float duration = 20;

    void Start() {
        Destroy(gameObject, duration);
    }

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
