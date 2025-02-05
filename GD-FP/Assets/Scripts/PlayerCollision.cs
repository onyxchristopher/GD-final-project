using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class PlayerCollision : MonoBehaviour {

    [SerializeField] private int startingMaxHealth;
    private int health;
    private int maxHealth;

    private bool inactive = false;

    [SerializeField] private int badCollisionDamage = 1;
    private bool invuln = false;
    private float invulnDuration = 2;
    private Slider healthBarSlider;

    // GameController ref
    private GameController gControl;
    
    void Start() {
        healthBarSlider = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
        gControl = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        EventManager.onPlayerDamage += Damage;
        EventManager.onPlayerDeath += CollisionInactive;
        EventManager.onPlayerRespawn += CollisionActive;
        EventManager.onNewUniverse += InitializeHealth;
    }

    public void InitializeHealth() {
        maxHealth = startingMaxHealth;
        SetHealth(maxHealth);
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = health;
    }

    public void IncreaseMaxHealth(int hp) {
        maxHealth += hp;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
            Damage(badCollisionDamage);
        }
    }

    public void Damage(int damage) {
        if (!invuln && !inactive){
            health -= damage;
            if (health < 0) {
                health = 0;
            }
            healthBarSlider.value = health;
            if (health == 0) {
                EventManager.PlayerDeath();
                gameObject.GetComponent<Animator>().SetTrigger("Death");
                gControl.crackBar(healthBarSlider);
                return;
            } else {
                gameObject.GetComponent<Animator>().SetTrigger("Damage");   
            }

            EventManager.PlayerHit();

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

    public void CollisionInactive() {
        inactive = true;
    }

    public void CollisionActive() {
        inactive = false;
        SetHealth(maxHealth);
        gControl.uncrackBar(healthBarSlider);
    }
}
