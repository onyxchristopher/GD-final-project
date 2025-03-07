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
    private LineRenderer lr;
    public GameObject protectiveForcefield;
    [SerializeField] private GameObject healthbar;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 3.5f, 0);
    private Slider healthBarSlider;

    void Start() {
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        health = maxHealth;
    }

    void OnEnable() {
        health = maxHealth;
    }
    
    public void Damage(int damage, bool suppressSound = false) {
        if (!invuln && !protectiveForcefield){
            health -= damage;

            // a normal enemy should have hit sounds at all times
            // a boss should have hit sounds but not on death
            if (isBoss) {
                gameController.SetBossHealthBar(health);
            }

            if (health <= 0) {
                enemy.EnemyDeath();
                if (!isBoss && !suppressSound) {
                    EventManager.EnemyHit();
                }
                if (linkedForcefield) {
                    linkedForcefield.GetComponent<Forcefield>().CheckForcefield();
                }
            } else {
                if (!isBoss) {
                    DisplayHealthbar(); // display healthbar if non-boss not dead
                }
                if (!suppressSound) {
                    EventManager.EnemyHit();
                }
            }

            invuln = true;
            Timing.RunCoroutine(_IFrames());
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
