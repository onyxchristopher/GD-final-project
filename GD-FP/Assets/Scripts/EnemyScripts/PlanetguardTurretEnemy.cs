using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The planetguard turret is a child of the boss enemy of the same name. Its
purpose is to track the player and fire at the player. 
*/

public class PlanetguardTurretEnemy : Enemy {
    [SerializeField] private GameObject projectile;
    private Rigidbody2D playerRB;
    public Vector2 location;

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
        if (state != State.ATTACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_TurretFire());
        }
    }

    private IEnumerator<float> _TurretFire() {
        Vector2 dirToPlayer = playerRB.position - location;
        Instantiate(projectile, transform.position, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dirToPlayer)));
        yield return Timing.WaitForSeconds(0.5f);
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
            state = State.TRACK;
            StateTransition();
        }
    }
}
