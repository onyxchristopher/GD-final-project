using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCollision : MonoBehaviour
{
    private int health;
    [SerializeField] private int maxHealth = 5;
    private bool invuln = false;
    private Slider healthBarSlider;
    
    void Start() {
        health = maxHealth;
        healthBarSlider = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = health;

        EventManager.onPlayerDeath += ResetPlayerHealth;
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
            StartCoroutine(iFrames());
        }
    }

    private IEnumerator iFrames() {
        yield return new WaitForSeconds(2);
        invuln = false;
    }

    public void ResetPlayerHealth() {
        health = maxHealth;
    }
}
