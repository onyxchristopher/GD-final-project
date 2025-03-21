using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelDrop : MonoBehaviour
{
    [HideInInspector] public int fuel; // the amount of fuel inside the drop

    void Start() {
        Destroy(gameObject, 60);
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            other.gameObject.GetComponent<PlayerMovement>().SetFuel(fuel);
            EventManager.Pickup();
            Destroy(gameObject);
        }
    }
}
