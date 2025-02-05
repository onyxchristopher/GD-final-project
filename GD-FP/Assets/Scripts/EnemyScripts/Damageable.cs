using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class Damageable : MonoBehaviour {

    private GameController gameController;
    public int maxHealth;
    [HideInInspector] public int health;
    [SerializeField] private bool isBoss;
    private bool invuln = false;
    private float invulnDuration = 0.3f;
    public Enemy enemy;

    void Start() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        health = maxHealth;
    }
    
    public void Damage(int damage) {
        if (!invuln){
            health -= damage;

            // a normal enemy should have hit sounds at all times
            // a boss should have hit sounds but not on death
            if (isBoss) {
                gameController.SetBossHealthBar(health);
            }

            if (health <= 0) {
                enemy.EnemyDeath();
                if (!isBoss) {
                    EventManager.EnemyHit();
                }
            } else {
                EventManager.EnemyHit();
            }

            invuln = true;
            Timing.RunCoroutine(_IFrames());
        }
    }

    private IEnumerator<float> _IFrames() {
        yield return Timing.WaitForSeconds(invulnDuration);
        invuln = false;
    }
}
