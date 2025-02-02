using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelDrop : MonoBehaviour
{
    [HideInInspector] public int fuel; // the amount of fuel inside the drop
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            other.gameObject.GetComponent<PlayerMovement>().SetFuel(fuel);
            EventManager.FuelPickup();
            Destroy(gameObject);
        }
    }

    
}
