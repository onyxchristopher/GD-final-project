using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The planetguard has three states: IDLE, TRACK, and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, moves to TRACK.
In TRACK, it rotates so its turret is facing the player. When the player is
in front of the turret, it moves to ATTACK. It moves back to TRACK when the player
gets out of the line of fire, and back to IDLE if the player moves out of range.

The planetguard is invulnerable except for four cracks in its structure. These can
be damaged by the player, and the planetguard takes 1/4 of its health as damage
when this happens. Any crack becomes invulnerable when damaged.
*/

public class PlanetguardBossEnemy : Enemy {
    private GameController gameController;
    [SerializeField] private float rotationSpeed;
    private Rigidbody2D playerRB;
    private Vector2 spawnpoint;
    private GameObject turret;
    private PlanetguardTurretEnemy turretControl;
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
        turret = transform.GetChild(1).gameObject;
        turretControl = turret.GetComponent<PlanetguardTurretEnemy>();
        gameObject.GetComponent<Damageable>().enemy = this;
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
    }

    private void TrackLoop() {
        if (state != State.TRACK) {
            return;
        }
        Timing.RunCoroutine(_RotatePlanetguard(), Segment.FixedUpdate);
    }

    private IEnumerator<float> _RotatePlanetguard() {
        Vector2 dirToPlayer = playerRB.position - spawnpoint;
        if (Vector2.SignedAngle(-transform.up, dirToPlayer) < 0) {
            transform.Rotate(new Vector3(0, 0, -rotationSpeed));
        } else {
            transform.Rotate(new Vector3(0, 0, rotationSpeed));
        }
        turretControl.location = new Vector2(turret.transform.position.x, turret.transform.position.y);
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
