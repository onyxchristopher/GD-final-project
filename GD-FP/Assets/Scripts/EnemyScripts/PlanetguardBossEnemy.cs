using System;
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

public class PlanetguardBossEnemy : Enemy {
    [SerializeField] private Vector3 rotationVector;
    private Rigidbody2D playerRB;
    private Vector2 spawnpoint;
    private GameObject turret1;
    private GameObject turret2;
    private PlanetguardTurretEnemy turretControl1;
    private PlanetguardTurretEnemy turretControl2;
    private Damageable damageable;
    [SerializeField] private string bossName;

    // Awake encodes the enemy FSM
    void Awake() {
        Action planetguardTrack = TrackLoop;
        enterStateLogic.Add(State.TRACK, planetguardTrack);
    }

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        spawnpoint = new Vector2(transform.position.x, transform.position.y);
        turret1 = transform.GetChild(1).gameObject;
        turret2 = transform.GetChild(2).gameObject;
        turretControl1 = turret1.GetComponent<PlanetguardTurretEnemy>();
        turretControl2 = turret2.GetComponent<PlanetguardTurretEnemy>();
        gameObject.GetComponent<Damageable>().enemy = this;
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
        Vector2 dirToPlayer = playerRB.position - spawnpoint;

        // rotate the closest turret towards the player
        float angle = Vector2.SignedAngle(transform.up, dirToPlayer);
        if ((angle > 0 && angle <= 90) || (angle > -180 && angle <= -90)) {
            transform.Rotate(rotationVector);
        } else {
            transform.Rotate(-rotationVector);
        }
        turretControl1.location = (Vector2) turret1.transform.position;
        turretControl2.location = (Vector2) turret2.transform.position;

        yield return Timing.WaitForOneFrame;
        TrackLoop();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Player") {
            state = State.TRACK;
            StateTransition();
            EventManager.EnterBossArea(bossName);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.tag == "Player") {
            state = State.IDLE;
            StateTransition();
            EventManager.ExitBossArea();
        }
    }

    public override void EnemyDeath() {
        Destroy(gameObject);
    }
}
