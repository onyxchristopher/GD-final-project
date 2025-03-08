using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour
{
    private GameObject player;
    private float degreesToRotate = 180;
    private float duration = 0.25f;
    private float angularSpeed;
    private int damage;

    void Start() {
        player = transform.parent.gameObject;
        angularSpeed = degreesToRotate / duration;
        Destroy(gameObject, duration);
    }

    void Update() {
        transform.RotateAround(player.transform.position, Vector3.forward, angularSpeed * Time.deltaTime);
    }

    public int GetDamage() {
        return damage;
    }

    public void SetDamage(int d) {
        damage = d;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Forcefield")) {
            if (!other.isTrigger) {
                Destroy(gameObject, duration / 5);
                EventManager.ForcefieldHit();
            }
        } else if (other.CompareTag("Damageable")) {
            if (!other.isTrigger) {
                other.gameObject.GetComponent<Damageable>().Damage(damage, false, "blade");
            }
        }
    }
}
