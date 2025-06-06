﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The planetguard has three states: IDLE, TRACK, and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to TRACK.
In TRACK, it rotates so one of its turrets is facing the player. When the player is
in front of the turret, it moves to ATTACK. It moves back to TRACK when the player
gets out of the line of fire, and back to IDLE if the player moves out of range.

The planetguard is invulnerable except for four cracks in its structure. These can
be damaged by the player, and the planetguard takes 1/4 of its health as damage
when this happens. Any crack becomes invulnerable when damaged.
*/

public class PlanetguardBossEnemy : Enemy
{
    [SerializeField] private Vector3 rotationVector;
    private Rigidbody2D playerRB;
    private Damageable damageable;
    [SerializeField] private string bossName;
    [SerializeField] private GameObject deathParticles;

    // Awake encodes the enemy FSM
    void Awake() {
        Action planetguardTrack = TrackLoop;
        enterStateLogic.Add(State.TRACK, planetguardTrack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        gameObject.GetComponent<Damageable>().enemy = this;

        EventManager.onPlayerDeath += ResetToIdle;
    }

    private void TrackLoop() {
        if (state != State.TRACK) {
            return;
        }
        if (gameObject != null && gameObject.activeInHierarchy) {
            Timing.RunCoroutine(_RotatePlanetguard(), Segment.FixedUpdate);
        }
    }

    private IEnumerator<float> _RotatePlanetguard() {
        while (state != State.IDLE) {
            Vector2 dirToPlayer = playerRB.position - (Vector2) transform.position;
            float angle = Vector2.SignedAngle(dirToPlayer, playerRB.velocity);
            if (angle > 0) {
                transform.Rotate(rotationVector);
            } else {
                transform.Rotate(-rotationVector);
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<PlayerCollision>().HullCollision();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.TRACK;
            StateTransition();
            EventManager.EnterBossArea(1);
            gameObject.GetComponent<CircleCollider2D>().radius += 20;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.IDLE;
            StateTransition();
            EventManager.ExitBossArea();
            gameObject.GetComponent<CircleCollider2D>().radius -= 20;
        }
    }

    public override void EnemyDeath() {
        EventManager.onPlayerDeath -= ResetToIdle;
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        EventManager.BossDefeat(1);
        if (drop) {
            GameObject artifact = Instantiate(drop, transform.position + Vector3.up * 18, Quaternion.identity);
            artifact.GetComponent<Artifact>().setId(10);
        }
        gameObject.SetActive(false);
    }
}
