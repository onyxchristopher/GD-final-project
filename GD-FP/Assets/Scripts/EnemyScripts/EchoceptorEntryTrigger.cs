using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoceptorEntryTrigger : MonoBehaviour
{
    private EchoceptorBossEnemy boss;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            boss = transform.parent.GetChild(0).GetComponent<EchoceptorBossEnemy>();
            boss.Spawn();
            EventManager.EnterBossArea(5);
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            bc.size = bc.size + Vector2.one * 20;
        }
    }
}
