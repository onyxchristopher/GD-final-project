using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The abyssforge fume turret is a child of the boss enemy of the same name. Its
purpose is to fire at the player with multiple fume bombs.
*/

public class AbyssforgeFumeTurretEnemy : Enemy
{
    private Rigidbody2D playerRB;
    [SerializeField] private float timeToRecharge;
    [SerializeField] private float timeToDetonate;
    [SerializeField] private GameObject fumebomb;
    private bool attacking = false;

    
    // Awake encodes the enemy FSM
    void Awake() {
        Action fumeTurretAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, fumeTurretAttack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void AttackLoop() {
        if (state != State.ATTACK) {
            return;
        }
        ReassignSpawn(transform.position);
        if (gameObject != null && gameObject.activeInHierarchy) {
            if (!attacking) {
                Timing.RunCoroutine(_Attack().CancelWith(gameObject), "fumeling");
            }
        }
    }

    private IEnumerator<float> _Attack() {
        attacking = true;
        while (state == State.ATTACK) {
            Vector2 dirToPlayer = playerRB.position - spawnpoint;
            GameObject bomb = Instantiate(fumebomb, transform.position,
            Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
            bomb.GetComponent<Bomb>().Detonate(timeToDetonate);
            yield return Timing.WaitForSeconds(timeToRecharge);
        }
        attacking = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.ATTACK;
            StateTransition();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.IDLE;
            StateTransition();
        }
    }
}
