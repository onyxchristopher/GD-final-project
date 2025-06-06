﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MEC;

public class GameController : MonoBehaviour
{    
    // Boss management
    private GameObject activeBoss;
    private GameObject bossBar;
    private GameObject bossText;
    private Slider bossHealthBar;
    private Text bossNameText;
    private string[] bossNames = new string[] {
    "Planetguard",
    "Voidcharger",
    "Duskwarden",
    "Murkfang",
    "Echoceptor",
    "The Abyssal Forge"
    };

    public static bool fiveArtifactsReclaimed;

    // Abyssal forge memory

    [HideInInspector] public bool rightCoreDefeated;
    [HideInInspector] public bool topCoreDefeated;
    [HideInInspector] public bool leftCoreDefeated;
    [HideInInspector] public bool bottomCoreDefeated;

    // Script refs
    private Generation gen;
    private Compass compass;
    private Scenes scenes;

    // Camera
    public Camera cam;
    [HideInInspector] public Rect cameraRect;

    // Universe building refs
    private GameObject universe;
    [SerializeField] GameObject cluster;
    [SerializeField] GameObject clusterSceneBoundary;

    // Sprites
    private Sprite uncrackedBar;
    [SerializeField] private Sprite crackedBar;

    private float sceneLoadingRadius = 75;

    public float timeToMove = 3;
    public float timeToRespawn = 1.5f;

    [SerializeField] private GameObject introArtifact;

    [SerializeField] private GameObject endscreen;
    private Text timeText;
    private float gameTime;
    public static int completedRuns = 0;

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
        scenes = GameObject.FindWithTag("GameController").GetComponent<Scenes>();

        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        InitializeUniverse();
        Timing.RunCoroutine(_CameraChangeCheck(), Segment.SlowUpdate);

        EventManager.onEnterBossArea += DisplayBossUI;
        EventManager.onExitBossArea += HideBossUI;
        EventManager.onEndGame += EndingSequence;
        EventManager.onPlayAgain += Restart;
        EventManager.onNewGame += EnableStopwatch;
    }

    private void InitializeUniverse() {
        EventManager.NewUniverse();

        // Procedurally generate world and initialize compass
        int seed = UnityEngine.Random.Range(0, 1000000);
        if (completedRuns == 0) {
            seed = 42;
        }
        Debug.Log(seed);
        
        (Cluster level0, Cluster[] level1, Cluster[][] level2) = gen.generate(seed);
        compass.InitializeCompass(level1, level2);

        // Initialize universe and cluster objects
        universe = new GameObject("Universe");
        universe.tag = "Universe";
        for (int i = 0; i < 6; i++) {
            Vector2 boundingSize = level1[i].getBounds().size;
            Vector3 corePos = (Vector3) level1[i].getCorePosition();

            ClusterBoundary.modules = new GameObject[level1.Length];

            // instantiate cluster at core, set collider size, set name and id
            GameObject clusterI = Instantiate(cluster, corePos, Quaternion.identity, universe.transform);
            clusterI.GetComponent<BoxCollider2D>().size = boundingSize;
            clusterI.name = $"Cluster{i+1}";
            clusterI.GetComponent<ClusterBoundary>().setId(level1[i].getId());

            // instantiate cluster scene boundary at core, set collider size, set name and id
            GameObject csb = Instantiate(clusterSceneBoundary, corePos, Quaternion.identity, universe.transform);
            csb.GetComponent<BoxCollider2D>().size = boundingSize + Vector2.one * sceneLoadingRadius * 2;
            csb.name = $"CSB{i+1}";
            csb.GetComponent<ClusterSceneBoundary>().setId(level1[i].getId());
        }

        scenes.InitializeScenes(level1.Length, level0, level1, level2);
    }

    

    private IEnumerator<float> _CompassArrowDelay() {
        yield return Timing.WaitForSeconds(12.5f);
        compass.CalculateAnchorRadius(cam.pixelRect);
    }

    // Checking if camera resolution has changed
    private IEnumerator<float> _CameraChangeCheck() {
        while (true) {
            if (cam.pixelRect.ToString() != cameraRect.ToString()) {
                cameraRect = cam.pixelRect;
                compass.CalculateAnchorRadius(cameraRect);
            }
            yield return Timing.WaitForOneFrame;
        }
        
    }

    // Boss UI

    public void DisplayBossUI(int sectorId) {
        activeBoss = SceneManager.GetSceneByBuildIndex(sectorId).GetRootGameObjects()[0].transform.GetChild(0).gameObject;
        Damageable damageable = activeBoss.GetComponent<Damageable>();
        bossHealthBar.maxValue = damageable.maxHealth;
        SetBossHealthBar(damageable.health);
        bossNameText.text = bossNames[sectorId - 1];
        bossBar.SetActive(true);
        bossText.SetActive(true);
    }

    public void SetBossHealthBar(int health) {
        bossHealthBar.value = health;
    }

    public void HideBossUI() {
        activeBoss = null;
        bossBar.SetActive(false);
        bossText.SetActive(false);
    }

    // Player UI bars

    public void crackBar(Slider bar) {
        Image imgComponent = bar.transform.GetChild(1).GetComponent<Image>();
        uncrackedBar = imgComponent.sprite;
        imgComponent.sprite = crackedBar;
    }

    public void uncrackBar(Slider bar) {
        bar.transform.GetChild(1).GetComponent<Image>().sprite = uncrackedBar;
    }

    // Starting sequence

    public void StartButtonPressed() {
        GameObject startscreen = GameObject.FindWithTag("Startscreen");
        if (startscreen) {
            Destroy(startscreen);
        }
        
        GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().StartTimer();
        GameObject.FindWithTag("MessageDashboard").GetComponent<MessageDashboard>().StartTimer();
        Instantiate(introArtifact, new Vector3(0, 18, 0), Quaternion.identity);
        Transform initialText = GameObject.FindWithTag("InitialText").transform;
        for (int i = 0; i < initialText.childCount; i++) {
            initialText.GetChild(i).GetComponent<Animator>().SetTrigger("Start");
            initialText.GetChild(i).GetComponent<DestroyAfterTime>().DestroyTrigger();
        }

        Timing.RunCoroutine(_CompassArrowDelay());
        EnableStopwatch();
    }

    // Timer and ending sequence

    private void EnableStopwatch() {
        Timing.RunCoroutine(_GameTime(), "gametime");
    }

    private IEnumerator<float> _GameTime() {
        gameTime = 0;
        while (true) {
            gameTime += Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }
    }

    private void EndingSequence() {
        Timing.RunCoroutine(_EndGame());
    }

    private IEnumerator<float> _EndGame() {
        // End stopwatch
        completedRuns++;

        // Wait for ending sequence
        yield return Timing.WaitForSeconds(3);

        // Transport player home, spawn endscreen
        EventManager.SetSpawn(2 * Vector3.up, 0);
        PlayerMovement pMove = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        pMove.transform.position = 2 * Vector3.up;
        pMove.playerInput.actions.FindActionMap("Player").Disable();
        pMove.rb.velocity = Vector2.zero;

        GameObject.FindWithTag("MainCanvas").SetActive(false);

        yield return Timing.WaitForSeconds(0.1f);

        GameObject currentEndscreen = Instantiate(endscreen);

        // Measure time

        TimeSpan spannedTime = TimeSpan.FromSeconds(gameTime);
        string finalTime = "Final time: " + spannedTime.ToString("mm':'ss'.'fff");

        timeText = currentEndscreen.transform.GetChild(0).GetChild(2).GetComponent<Text>();
        timeText.text = finalTime;

        yield return Timing.WaitForSeconds(2);
        pMove.playerInput.actions.FindActionMap("UI").Enable();
    }

    private void Restart() {
        fiveArtifactsReclaimed = false;
        rightCoreDefeated = false;
        topCoreDefeated = false;
        leftCoreDefeated = false;
        bottomCoreDefeated = false;
        Destroy(GameObject.FindWithTag("Universe"));
        InitializeUniverse();
    }
}
