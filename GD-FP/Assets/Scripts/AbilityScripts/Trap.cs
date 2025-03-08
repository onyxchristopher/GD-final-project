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
        List<Collider2D> colliders = new List<Collider2D>();
        ContactFilter2D cf = new ContactFilter2D();
        cf.SetLayerMask(LayerMask.GetMask("Enemy"));
        int len = Physics2D.OverlapCircle((Vector2) transform.position, explosionRadius, cf, colliders);
        for (int i = 0; i < len; i++) {
            if (colliders[i].CompareTag("Damageable")) {
                colliders[i].GetComponent<Damageable>().Damage(damage, false, "trap");
            }
        }
    }
}
