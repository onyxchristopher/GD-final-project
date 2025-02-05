using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDrop : MonoBehaviour
{
    [HideInInspector] public int health; // the amount of fuel inside the drop

    void Start() {
        Destroy(gameObject, 20);
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            other.gameObject.GetComponent<PlayerCollision>().SetHealth(health);
            EventManager.Pickup();
            Destroy(gameObject);
        }
    }
}
