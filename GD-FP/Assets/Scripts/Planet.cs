﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Planet : MonoBehaviour
{
    private Rigidbody2D playerRB;
    private Vector2 dist;
    [SerializeField] private float gravityScale;
    private float gravitationalConstant = 50;
    private bool suspendGravity = false;
    private GameController gControl;

    void Start() {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        EventManager.onPlayerDeath += SuspendGravityTimer;
    }

    // OnTriggerStay2D applies gravity to the player while they are within the planet's trigger
    // It specifies the Law of Gravity: F = G * m1 * m2 / r^2, where G = gravitationalConstant,
    // m1 = player mass (which is 1), m2 = gravityScale, r = distance to planet center
    void OnTriggerStay2D(Collider2D other) {
        if (!other.CompareTag("Player") || suspendGravity) {
            return;
        }
        dist = (Vector2) transform.position - playerRB.position; // distance to planet center
        if (dist.magnitude > 80) {
            return;
        }
        // the law of gravity, in code
        playerRB.AddForce(dist * (gravitationalConstant * gravityScale / dist.sqrMagnitude));
    }

    public void SuspendGravityTimer() {
        Timing.RunCoroutine(_DelaySuspendGravity());
    }

    private IEnumerator<float> _DelaySuspendGravity() {
        yield return Timing.WaitForSeconds(gControl.timeToMove);
        suspendGravity = true;
        yield return Timing.WaitForSeconds(gControl.timeToRespawn);
        suspendGravity = false;
    }
}
