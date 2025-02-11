using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidchargerEntryTrigger : MonoBehaviour
{
    private VoidchargerBossEnemy boss;
    private bool bossDefeated = false;

    void Start() {
        boss = transform.parent.GetChild(0).GetComponent<VoidchargerBossEnemy>();
        EventManager.onBossDefeat += CheckBossDefeat;
    }
    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player" && !bossDefeated) {
            boss.state = Enemy.State.ATTACK;
            boss.StateTransition();
            EventManager.EnterBossArea(boss.bossName);
            BoxCollider2D bc = gameObject.GetComponent<BoxCollider2D>();
            bc.size = bc.size + Vector2.one * 10;
        }
    }

    public void CheckBossDefeat(string bossName) {
        if (bossName == "Voidcharger") {
            bossDefeated = true;
        }
    }
    
}
