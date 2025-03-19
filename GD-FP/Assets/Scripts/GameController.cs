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
    private int activeBossHealth;
    private GameObject bossBar;
    private GameObject bossText;
    private Slider bossHealthBar;
    private Text bossNameText;
    private string[] bossNames = new string[] {
    "Planetguard",
    "Voidcharger",
    "Duskwarden",
    "Murkfang",
    "",
    "",
    "",
    ""};

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

        Timing.RunCoroutine(_CompassArrowDelay());
    }

    private void InitializeUniverse() {
        EventManager.NewUniverse();

        // Procedurally generate world and initialize compass
        int seed = 42; // Random.Range(0, 1000000);
        (Cluster level0, Cluster[] level1, Cluster[][] level2) = gen.generate(seed);
        compass.InitializeCompass(level1, level2);

        // Initialize universe and cluster objects
        universe = new GameObject("Universe");
        for (int i = 0; i < level1.Length; i++) {
            Vector2 boundingSize = level1[i].getBounds().size;
            Vector3 corePos = (Vector3) level1[i].getCorePosition();

            ClusterBoundary.modules = new GameObject[level1.Length];

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

        scenes.InitializeScenes(level1.Length, level0, level1, level2);
    }

    private IEnumerator<float> _CompassArrowDelay() {
        yield return Timing.WaitForSeconds(12.5f);
        compass.CalculateAnchorRadius(cam.pixelRect);
    }

    public void Pause() {

    }

    // Checking if camera resolution has changed
    private IEnumerator<float> _CameraChangeCheck() {
        if (cam.pixelRect.ToString() != cameraRect.ToString()) {
            cameraRect = cam.pixelRect;
            compass.CalculateAnchorRadius(cameraRect);
        }
        yield return Timing.WaitForOneFrame;
        Timing.RunCoroutine(_CameraChangeCheck(), Segment.SlowUpdate);
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
}
