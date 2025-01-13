using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour {
    private GameObject player;
    private int damage;

    void Start() {
        player = transform.parent.gameObject;
        Destroy(gameObject, 0.5f);
    }

    void FixedUpdate() {
        transform.RotateAround(player.transform.position, Vector3.forward, 14.4f);
    }

    public void SetDamage(int d) {
        damage = d;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Damageable") {
            if (!other.isTrigger) {
                other.gameObject.GetComponent<Damageable>().Damage(damage);
            }
        }
    }
}
