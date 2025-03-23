﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The Abyssal Forge has two states: IDLE and TRACK.
It starts in IDLE, and when its trigger is entered by the player, moves to TRACK.
*/

public class AbyssforgeBossEnemy : Enemy
{
    private GameController gControl;
    private Damageable dmg;
    [SerializeField] private GameObject triangleDeathParticles;
    [SerializeField] private GameObject finalDeathParticles;

    void Awake() {
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();
    }

    void Start() {
        dmg = gameObject.GetComponent<Damageable>();
    }

    public void Spawn(Vector3 pos) {
        spawnpoint = (Vector2) pos;
        if (gControl.rightCoreDefeated) {
            transform.GetChild(0).gameObject.SetActive(false);
            dmg.Damage(15, true);
        }
        if (gControl.topCoreDefeated) {
            transform.GetChild(1).gameObject.SetActive(false);
            dmg.Damage(15, true);
        }
        if (gControl.leftCoreDefeated) {
            transform.GetChild(2).gameObject.SetActive(false); 
            dmg.Damage(15, true);
        }
        if (gControl.bottomCoreDefeated) {
            transform.GetChild(3).gameObject.SetActive(false);
            dmg.Damage(15, true);
        }
    }

    public void CoreDefeated(Vector3 absolutePosition) {
        Vector2 position = (Vector2) absolutePosition - spawnpoint;
        if (position.x > 0 && position.y == 0) {
            transform.GetChild(0).gameObject.SetActive(false);
            gControl.rightCoreDefeated = true;
            Instantiate(triangleDeathParticles, transform.position + Vector3.right * 100, Quaternion.Euler(0, 0, -90));
        } else if (position.x == 0 && position.y > 0) {
            transform.GetChild(1).gameObject.SetActive(false);
            gControl.topCoreDefeated = true;
            Instantiate(triangleDeathParticles, transform.position + Vector3.up * 100, Quaternion.Euler(0, 0, 0));
        } else if (position.x < 0 && position.y == 0) {
            transform.GetChild(2).gameObject.SetActive(false);
            gControl.leftCoreDefeated = true;
            Instantiate(triangleDeathParticles, transform.position - Vector3.right * 100, Quaternion.Euler(0, 0, 90));
        } else if (position.x == 0 && position.y < 0) {
            transform.GetChild(3).gameObject.SetActive(false);
            gControl.bottomCoreDefeated = true;
            Instantiate(triangleDeathParticles, transform.position - Vector3.up * 100, Quaternion.Euler(0, 0, 180));
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.TRACK;
            StateTransition();
            EventManager.EnterBossArea(6);
            GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(40, 0, 1);
            gameObject.GetComponent<BoxCollider2D>().size += (Vector2.one * 25);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            state = State.IDLE;
            StateTransition();
            EventManager.ExitBossArea();
            GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>().ChangeSize(30, 1, 1);
            gameObject.GetComponent<BoxCollider2D>().size -= (Vector2.one * 25);
        }
    }
}
