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
    // Start is called before the first frame update
    void Start() {
        health = maxHealth;
        healthBarSlider = GameObject.FindWithTag("HealthBar").GetComponent<Slider>();
        healthBarSlider.maxValue = maxHealth;
        healthBarSlider.value = health;
    }

    public void OnAction1(){
        Damage(3);
    }

    public void Damage(int damage) {
        if (invuln == false){
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

    // Update is called once per frame
    void Update() {
        
    }
}
