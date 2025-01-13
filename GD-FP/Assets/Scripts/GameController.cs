using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class GameController : MonoBehaviour {
    
    private GameObject activeBoss;
    private int activeBossHealth;
    private GameObject bossBar;
    private GameObject bossText;
    private Slider bossHealthBar;
    private Text bossNameText;
    private Generation gen;

    void Awake() {
        Screen.SetResolution(1080, 1080, true);
        bossBar = GameObject.FindWithTag("BossBar");
        bossText = GameObject.FindWithTag("BossText");
        bossHealthBar = bossBar.GetComponent<Slider>();
        bossNameText = bossText.GetComponent<Text>();
        bossBar.SetActive(false);
        bossText.SetActive(false);
    }
    
    void Start() {
        EventManager.onArtifactPickup += Upgrade;
        gen = gameObject.GetComponent<Generation>();

        int seed = 42;

        (Cluster[] level1, Cluster[][] level2) = gen.generate(seed);
    }

    public void Pause() {

    }

    public void DisplayBossUI(string name) {
        activeBoss = GameObject.FindWithTag(name);
        Damageable damageable = activeBoss.GetComponent<Damageable>();
        bossHealthBar.maxValue = damageable.maxHealth;
        SetBossHealthBar(damageable.health);
        bossNameText.text = name;
        bossBar.SetActive(true);
        bossText.SetActive(true);
    }

    public void SetBossHealthBar(int health) {
        bossHealthBar.value = health;
    }

    public void HideBossUI() {
        bossBar.SetActive(false);
        bossText.SetActive(false);
    }

    public void Upgrade(int id) {
        Timing.RunCoroutine(_ResetTimer());
    }

    private IEnumerator<float> _ResetTimer() {
        yield return Timing.WaitForSeconds(3);
        EventManager.PlayerDeath();
    }
}
