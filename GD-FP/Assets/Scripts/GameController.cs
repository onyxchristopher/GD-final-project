using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class GameController : MonoBehaviour
{    
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
    private Scenes scenes;

    // Camera
    private Camera cam;
    private Rect cameraRect;

    // Universe building refs
    private GameObject universe;
    [SerializeField] GameObject cluster;
    [SerializeField] GameObject clusterSceneBoundary;


    private float sceneLoadingRadius = 75;
    

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
        cameraRect = cam.pixelRect;

        InitializeUniverse();
        Timing.RunCoroutine(_CameraChangeCheck(), Segment.SlowUpdate);
    }

    private void InitializeUniverse() {
        EventManager.NewUniverse();

        // Procedurally generate world and then initialize compass
        int seed = 42; // Random.Range(0, 1000000);
        (Cluster level0, Cluster[] level1, Cluster[][] level2) = gen.generate(seed);
        compass.InitializeCompass(level1, level2);

        // Initialize universe and cluster objects
        universe = new GameObject("Universe");
        for (int i = 0; i < level1.Length; i++) {
            Vector2 boundingSize = level1[i].getBounds().size;
            Vector3 corePos = (Vector3) level1[i].getCorePosition();

            // instantiate cluster at core, set collider size, set name and id
            GameObject clusterI = Instantiate(cluster, corePos, Quaternion.identity, universe.transform);
            clusterI.GetComponent<BoxCollider2D>().size = boundingSize;
            clusterI.name = $"Cluster{i+1}";
            clusterI.GetComponent<ClusterBoundary>().setId(level1[i].getId());

            // instantiate cluster scene boundary at core, set collider size, set name and id
            GameObject csb = Instantiate(clusterSceneBoundary, corePos, Quaternion.identity);
            csb.GetComponent<BoxCollider2D>().size = boundingSize + Vector2.one * sceneLoadingRadius * 2;
            csb.name = $"CSB{i+1}";
            csb.GetComponent<ClusterSceneBoundary>().setId(level1[i].getId());
        }

        scenes.InitializeScenes(level1.Length, level1, level2);
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
