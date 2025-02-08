using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

/*
The voidcharger has two states: IDLE and ATTACK.
It starts in IDLE, and when its trigger is entered by the player, enacts a forcefield around its domain.
In ATTACK, it briefly telegraphs its movement and then charges at the player. If it hits the player,
it damages them and knocks them back. If it hits a forcefield wall, it bounces off with an approximation of
an elastic collision. It moves back to IDLE and removes the forcefield if the player dies.

The voidcharger is vulnerable on its sides and back.
*/

public class VoidchargerBossEnemy : Enemy
{
    [SerializeField] private Vector3 rotationVector;
    private Rigidbody2D playerRB;
    private Rigidbody2D rb;
    private Damageable damageable;
    [SerializeField] private string bossName;

    // Awake encodes the enemy FSM

    void Awake() {
        //Action chargerAttack = AttackLoop;
        //enterStateLogic.Add(State.ATTACK, chargerAttack);
    }
    
    void Start() {
        
    }

    
}
