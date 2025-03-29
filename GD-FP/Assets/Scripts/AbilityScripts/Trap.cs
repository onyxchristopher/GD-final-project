using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Trap : MonoBehaviour
{
    private int damage;
    private float duration = 60;
    private float explosionRadius = 12;

    void Start() {
        Destroy(gameObject, duration);
    }

    public void SetDamage(int d) {
        damage = d;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Damageable")) {
            if (!other.isTrigger) {
                FindEnemiesInRadius();
                Destroy(gameObject);
            }
        }
    }

    private void FindEnemiesInRadius() {
        // create a list of colliders to receive results
        List<Collider2D> colliders = new List<Collider2D>();

        // define a contact filter to only search in the Enemy layer
        ContactFilter2D cf = new ContactFilter2D();
        cf.SetLayerMask(LayerMask.GetMask("Enemy"));

        // check which enemies are in the circle
        int len = Physics2D.OverlapCircle((Vector2) transform.position, explosionRadius, cf, colliders);

        // damage all collided enemies, provided they can be damaged
        for (int i = 0; i < len; i++) {
            if (colliders[i].CompareTag("Damageable")) {
                colliders[i].GetComponent<Damageable>().Damage(damage, false, "trap");
            }
        }
    }
}
