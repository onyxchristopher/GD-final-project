using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetguardHullEnemy : Enemy {
    
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite damagedHull;
    private Damageable bossHealth;
    private bool damaged = false;

    void Start() {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        gameObject.GetComponent<Damageable>().enemy = this;
        bossHealth = transform.parent.gameObject.GetComponent<Damageable>();
    }

    public override void EnemyDeath() {
        spriteRenderer.sprite = damagedHull;
        if (!damaged) {
            bossHealth.Damage(5);
            damaged = true;
        }
    }
}
