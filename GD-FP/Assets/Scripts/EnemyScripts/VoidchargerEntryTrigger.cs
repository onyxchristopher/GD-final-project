using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidchargerEntryTrigger : MonoBehaviour
{
    private VoidchargerBossEnemy boss;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            boss = transform.parent.GetChild(0).GetComponent<VoidchargerBossEnemy>();
            boss.state = Enemy.State.ATTACK;
            boss.StateTransition();
            EventManager.EnterBossArea(2);
            BoxCollider2D bc = gameObject.GetComponent<BoxCollider2D>();
            bc.size = bc.size + Vector2.one * 10;
        }
    }
}
