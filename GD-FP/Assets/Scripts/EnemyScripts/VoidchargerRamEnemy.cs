using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidchargerRamEnemy : MonoBehaviour
{
    [SerializeField] private int damage;
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            EventManager.PlayerDamage(damage);
        }
    }
}
