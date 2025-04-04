﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuskwardenEntryTrigger : MonoBehaviour
{
    private DuskwardenBossEnemy boss;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            boss = transform.parent.GetChild(0).GetComponent<DuskwardenBossEnemy>();
            boss.Spawn();
            EventManager.EnterBossArea(3);
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            bc.size = bc.size + Vector2.one * 65;
        }
    }
}
