using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class Damageable : MonoBehaviour
{

    private GameController gameController;
    public int maxHealth;
    [HideInInspector] public int health;
    [SerializeField] private bool isBoss;
    private bool invuln = false;
    private float invulnDuration = 0.3f;
    public Enemy enemy;
    public GameObject linkedForcefield;
    public GameObject protectiveForcefield;
    [SerializeField] private GameObject healthbar;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 3.5f, 0);
    private Slider healthBarSlider;

    void Awake() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        health = maxHealth;
    }

    void OnEnable() {
        health = maxHealth;
    }
    
    public void Damage(int damage, bool suppressSound = false, string source = "null") {
        // if the enemy is invuln and the source is a blade, return
        if (invuln && source == "blade") {
            return;
        } else if (source == "blade") {
            invuln = true; // a blade grants 0.3 seconds invuln from more blade attacks
            Timing.RunCoroutine(_IFrames());
        }
        if (!protectiveForcefield){ // if the enemy is not being protected by a forcefield
            health -= damage; // subtract damage from health

            if (isBoss) {
                gameController.SetBossHealthBar(health); // manage boss health bar
            }

            // Damage the Abyssal Forge if one of its cores is damaged
            if (gameObject.name == "AbyssCoreEnemy") {
                // if the core is overkilled (health < 0) send only the remaining damage to the boss
                if (health <= 0) {
                    transform.parent.parent.GetComponent<Damageable>().Damage(health + damage, true);
                } else {
                    transform.parent.parent.GetComponent<Damageable>().Damage(damage, true);
                }
                if (maxHealth == 100) {
                    GetComponent<AbyssforgeGreaterCore>().GreaterCoreDamaged();
                }
                
            }

            // a normal enemy should have hit sounds at all times
            // a boss should have hit sounds but not on death

            if (health <= 0) { // enemy is dead
                enemy.EnemyDeath(); // call the attached enemy's EnemyDeath function
                if (!isBoss && !suppressSound) {
                    EventManager.EnemyHit(); // plays the enemy hit sound
                }
                // if the enemy was linked to a forcefield, check if all linked enemies are dead
                if (linkedForcefield) {
                    linkedForcefield.GetComponent<Forcefield>().CheckForcefield();
                }
            } else { // enemy is not dead
                if (!isBoss) {
                    DisplayHealthbar(); // display healthbar if non-boss not dead
                }
                if (!suppressSound) {
                    EventManager.EnemyHit(); // plays the enemy hit sound
                }
            }
        }
    }

    private IEnumerator<float> _IFrames() {
        yield return Timing.WaitForSeconds(invulnDuration);
        invuln = false;
    }

    // Non-boss health bars

    private void DisplayHealthbar() {
        if (healthbar) {
            if (!healthBarSlider) {
                GameObject hp = Instantiate(healthbar, transform.position, Quaternion.identity,
                                            GameObject.FindWithTag("WSCanvas").transform);
                healthBarSlider = hp.GetComponent<Slider>();
                healthBarSlider.maxValue = maxHealth;
                Timing.RunCoroutine(_MoveHealthbar(hp).CancelWith(gameObject));
            }
            healthBarSlider.value = health;
        }
    }

    private IEnumerator<float> _MoveHealthbar(GameObject hp) {
        while (true) {
            hp.transform.position = transform.position + healthBarOffset;
            yield return Timing.WaitForOneFrame;
            yield return Timing.WaitForOneFrame;
        }
    }

    void OnDisable() {
        if (healthBarSlider) {
            Destroy(healthBarSlider.gameObject);
        }
    }
}
