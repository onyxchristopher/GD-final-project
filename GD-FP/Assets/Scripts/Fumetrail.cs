using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fumetrail : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            other.gameObject.GetComponent<PlayerCollision>().Damage(9);
        }
    }
}
