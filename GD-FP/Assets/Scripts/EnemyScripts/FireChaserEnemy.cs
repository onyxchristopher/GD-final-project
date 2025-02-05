using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The fire chaser has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to ATTACK.
In ATTACK, it moves towards the player at a constant rate.
It also fires at the player every [delay] seconds.
It moves back to IDLE, returning to its spawnpoint, when its trigger is exited by the player.
*/

public class FireChaserEnemy : Enemy {
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    [SerializeField] private float delay;
    [SerializeField] private Vector2 vel;
    private bool firedWithinDelay = false;

    // Awake encodes the enemy FSM
    void Awake() {
        Action chaserAttack = ChaseLoop;
        chaserAttack += FireLoop;
        chaserAttack += MoveConstantVelocity;
        enterStateLogic.Add(State.ATTACK, chaserAttack);

        Action chaserReturn = ReturnLoop;
        chaserReturn += MoveConstantVelocity;
        exitStateLogic.Add(State.ATTACK, chaserReturn);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.GetComponent<Damageable>().enemy = this;
        ReassignSpawn(transform.position);

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void MoveConstantVelocity() {
        rb.velocity = vel;
    }

    private void ChaseLoop() {
        if (state == State.IDLE) {
            return;
        }

        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Chase());
        }
    }

    private IEnumerator<float> _Chase() {
        Vector2 dirToPlayer = playerRB.position - rb.position;
        float angle = Vector2.SignedAngle(transform.right, dirToPlayer);
        rb.rotation += angle;
        yield return Timing.WaitForOneFrame;
        ChaseLoop();
    }

    private void FireLoop() {
        if (state == State.IDLE || firedWithinDelay) {
            return;
        }

        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Fire());
        }
    }

    private IEnumerator<float> _Fire() {
        firedWithinDelay = true;
        Vector2 dirToPlayer = playerRB.position - rb.position;
        Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        yield return Timing.WaitForSeconds(delay);
        firedWithinDelay = false;
        FireLoop();
    }

    private void ReturnLoop() {
        if (state != State.IDLE) {
            return;
        }

        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_Return());
        }
    }
    
    private IEnumerator<float> _Return() {
        Vector2 dirToSpawn = spawnpoint - rb.position;
        if (dirToSpawn.magnitude < 1) {
            rb.velocity = Vector2.zero;
            yield break;
        }
        yield return Timing.WaitForOneFrame;
        ReturnLoop();
    }
    
}
