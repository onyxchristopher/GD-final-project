using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class PlayerCollision : MonoBehaviour {

    private int startingMaxHealth = 5;
    private int health;
    private int maxHealth;

    [SerializeField] private int badCollisionDamage = 1;
    private bool invuln = false;
    private float invulnDuration = 2;
    private Slider healthBarSlider;
    
    
    void Start() {
        healthBarSlider = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = health;

        EventManager.onPlayerDamage += Damage;
        EventManager.onPlayerDeath += ResetPlayerHealth;
        EventManager.onNewUniverse += InitializeHealth;
    }

    public void InitializeHealth() {
        maxHealth = startingMaxHealth;
        SetHealth(maxHealth);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            Damage(badCollisionDamage);
        }
    }

    public void Damage(int damage) {
        if (!invuln){
            health -= damage;
            if (health < 0) {
                health = 0;
            }
            healthBarSlider.value = health;
            if (health == 0) {
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

    public void SetHealth(int healthToGain) {
        health += healthToGain;
        if (health >= maxHealth) {
            health = maxHealth;
        }
        healthBarSlider.value = health;
    }
}
