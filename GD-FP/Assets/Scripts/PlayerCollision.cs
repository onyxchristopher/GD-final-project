using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class PlayerCollision : MonoBehaviour {
    private int health;
    [SerializeField] private int maxHealth;
    private int badCollisionDamage = 2;
    private bool invuln = false;
    private float invulnDuration = 2;
    private Slider healthBarSlider;
    
    
    void Start() {
        health = maxHealth;
        healthBarSlider = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = health;

        EventManager.onPlayerDamage += Damage;
        EventManager.onPlayerDeath += ResetPlayerHealth;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            Damage(badCollisionDamage);
        }
    }

    public void Damage(int damage) {
        if (!invuln){
            health -= damage;
            healthBarSlider.value = health;
            if (health <= 0) {
                EventManager.PlayerDeath();
                return;
            }
            invuln = true;
            Timing.RunCoroutine(_IFrames());
        }
    }

    private IEnumerator<float> _IFrames() {
        yield return Timing.WaitForSeconds(invulnDuration);
        invuln = false;
    }

    public void ResetPlayerHealth() {
        health = maxHealth;
        healthBarSlider.value = health;
    }
}
