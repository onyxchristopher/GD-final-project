using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurkfangEntryTrigger : MonoBehaviour
{
    private MurkfangBossEnemy boss;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            boss = transform.parent.GetChild(0).GetComponent<MurkfangBossEnemy>();
            boss.state = Enemy.State.ATTACK;
            boss.StateTransition();
            EventManager.EnterBossArea(4);
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            bc.size = bc.size + Vector2.one * 20;
        }
    }
}
