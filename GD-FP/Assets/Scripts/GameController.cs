using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class GameController : MonoBehaviour {
    
    // Boss management
    private GameObject activeBoss;
    private int activeBossHealth;
    private GameObject bossBar;
    private GameObject bossText;
    private Slider bossHealthBar;
    private Text bossNameText;

    // Script refs
    private Generation gen;
    private Compass compass;

    // Camera
    private Camera cam;
    private Rect cameraRect;
    

    void Awake() {
        bossBar = GameObject.FindWithTag("BossBar");
        bossText = GameObject.FindWithTag("BossText");
        bossHealthBar = bossBar.GetComponent<Slider>();
        bossNameText = bossText.GetComponent<Text>();
        bossBar.SetActive(false);
        bossText.SetActive(false);
    }
    
    void Start() {
        gen = gameObject.GetComponent<Generation>();
        compass = GameObject.FindWithTag("Compass").GetComponent<Compass>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        cameraRect = cam.pixelRect;

        InitializeUniverse();
        Timing.RunCoroutine(_CameraChangeCheck(), Segment.SlowUpdate);
    }

    private void InitializeUniverse() {
        EventManager.NewUniverse();
        int seed = 42; // Random.Range(0, 1000000);
        (Cluster level0, Cluster[] level1, Cluster[][] level2) = gen.generate(seed);
        compass.InitializeCompass(level1, level2);
        EventManager.EnterCluster(1);
    }

    public void Pause() {

    }

    private IEnumerator<float> _CameraChangeCheck() {
        if (cam.pixelRect.ToString() != cameraRect.ToString()) {
            cameraRect = cam.pixelRect;
            compass.CalculatePixelRadius(cameraRect);
        }
        yield return Timing.WaitForOneFrame;
        Timing.RunCoroutine(_CameraChangeCheck(), Segment.SlowUpdate);
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

    private IEnumerator<float> _ResetTimer() {
        yield return Timing.WaitForSeconds(3);
        EventManager.PlayerDeath();
    }
}
