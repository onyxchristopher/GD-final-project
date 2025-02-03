using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The planetguard turret is a child of the boss enemy of the same name. Its
purpose is to fire at the player. 
*/

public class PlanetguardTurretEnemy : Enemy {
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    [HideInInspector] public Vector2 location;
    [SerializeField] private float delay;
    private bool firedWithinDelay = false;

    // Awake encodes the enemy FSM
    void Awake() {
        Action turretAttack = AttackLoop;
        enterStateLogic.Add(State.ATTACK, turretAttack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        location = new Vector2(transform.position.x, transform.position.y);
    }

    private void AttackLoop() {
        if (state != State.ATTACK || firedWithinDelay) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_TurretFire());
        }
    }

    private IEnumerator<float> _TurretFire() {
        firedWithinDelay = true;
        Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
        Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        yield return Timing.WaitForSeconds(delay);
        firedWithinDelay = false;
        AttackLoop();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            state = State.ATTACK;
            StateTransition();
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            state = State.IDLE;
            StateTransition();
        }
    }
}
