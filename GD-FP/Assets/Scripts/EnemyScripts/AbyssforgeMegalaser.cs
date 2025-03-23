using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbyssforgeMegalaser : MonoBehaviour
{
    [SerializeField] private int damage;
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            EventManager.PlayerDamage(damage);
        }
    }
}
