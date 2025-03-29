using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using MEC;

public class Compass : MonoBehaviour
{
     // the camera
    private Camera cam;

    // how much of the screen the compass radius takes
    [SerializeField] private float cameraRatio;

    // First and second-level clusters
    private Cluster[] level1;
    private Cluster[][] level2;

    // the cluster in which the player is currently located
    public int currCluster = 0;

    // the vector location of the next major objective
    private Vector2 major;

    // the array indices of all active minor objectives
    private List<int> indices = new List<int>();

    // player rigidbody
    private Rigidbody2D playerRB;

    // Progression tracker
    private List<int> majorProg = new List<int>();
    private List<int> minorProg = new List<int>();

    // Compass arrow references
    public RectTransform majorArrowTransform;
    private RectTransform[] minorArrowTransforms = new RectTransform[12];

    // Visual elements
    [SerializeField] private GameObject majorCompassArrow;
    [SerializeField] private GameObject minorCompassArrow;

    private bool majorShown = true;
    private bool minorShown = false;
    private int respawnSector = 0;

    void Start()
    {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
    }

    public void InitializeCompass(Cluster[] l1, Cluster[][] l2) {
        EventManager.onArtifactPickup += UpdateProgression;
        EventManager.onEnterCluster += EnteringCluster;
        EventManager.onExitCluster += LeavingCluster;
        EventManager.onEnterBossArea += HideCompass;
        EventManager.onExitBossArea += ShowCompassCheck;
        EventManager.onSetSpawn += SetRespawnSector;
        EventManager.onPlayerDeath += DeactivateAll;
        EventManager.onPlayerRespawn += RespawnAlignment;

        currCluster = 0;
        level1 = l1;
        level2 = l2;
        UpdateMajor(1);

        if (majorArrowTransform) {
            Destroy(majorArrowTransform.gameObject);
        }

        // create arrows which are children of the compass
        for (int i = 0; i < minorArrowTransforms.Length; i++) {
            GameObject minorArrow = Instantiate(minorCompassArrow, Vector3.zero, Quaternion.identity, transform);
            minorArrowTransforms[i] = minorArrow.GetComponent<RectTransform>();
            minorArrow.SetActive(false);
        }

        GameObject majorArrow = Instantiate(majorCompassArrow, Vector3.zero, Quaternion.identity, transform);
        majorArrowTransform = majorArrow.GetComponent<RectTransform>();

        majorShown = true;

        // sector 6 has no minor objectives
        minorProg.Add(61);
        minorProg.Add(62);
    }

    public void EnteringCluster(int clusterNum) {
        if (GameObject.FindWithTag("Player").GetComponent<PlayerCollision>().inactive) {
            return;
        }
        currCluster = clusterNum;

        // update the minor arrows to the current cluster
        UpdateMinor(clusterNum);
        
        // show each arrow that should be shown
        ShowMinor();
    }

    public void LeavingCluster(int clusterNum) {
        currCluster = 0;
        
        HideMinor();
    }

    public void HideCompass(int _) {
        HideMajor();
        HideMinor();
    }

    private void ShowCompassCheck() {
        Timing.RunCoroutine(_ShowCompassCheck());
    }

    private IEnumerator<float> _ShowCompassCheck() {
        majorShown = true;
        minorShown = true;
        yield return Timing.WaitForOneFrame;
        if (!GameObject.FindWithTag("Player").GetComponent<PlayerCollision>().inactive) {
            ShowCompass();
        }
    }

    public void ShowCompass() {
        ShowMajor();
        ShowMinor();
    }

    private void ShowMajor() {
        majorShown = true;
        majorArrowTransform.gameObject.SetActive(true);
    }

    private void ShowMinor() {
        if (currCluster == 0 || currCluster == 6) {
            HideMinor();
            return;
        }
        minorShown = true;
        for (int i = 0; i < indices.Count; i++) {
            minorArrowTransforms[indices[i]].gameObject.SetActive(true);
        }
    }

    private void HideMajor() {
        majorArrowTransform.gameObject.SetActive(false);
        majorShown = false;
    }

    private void HideMinor() {
        for (int i = 0; i < indices.Count; i++) {
            minorArrowTransforms[indices[i]].gameObject.SetActive(false);
        }
        minorShown = false;
    }

    public void UpdateProgression(int id) {
        int firstDigit = id / 10;
        int secondDigit = id % 10;
        if (id == 60) {
            DestroyAllArrows();
            return;
        }
        // major progression
        if (secondDigit == 0) {
            majorProg.Add(id);
            for (int i = 1; i <= level1.Length; i++) {
                // the next goal is the first artifact that is not in the list of reached artifacts
                if (!majorProg.Contains(i * 10)) {
                    UpdateMajor(i);
                    break;
                }
            }
        } else { // minor
            minorProg.Add(id);
            minorArrowTransforms[2 * (firstDigit - 1) + (secondDigit - 1)].gameObject.SetActive(false);
            UpdateMinor(firstDigit);
            minorShown = true;
        }
    }

    // point to the next major or zero if not
    private void UpdateMajor(int clusterNum) {
        if (clusterNum < level1.Length) {
            major = level1[clusterNum - 1].getCorePosition();
        } else {
            major = Vector2.zero;
        }
    }

    // point to the minor objectives of the cluster entered
    private void UpdateMinor(int clusterNum) {
        if (clusterNum == 0) {
            indices = new List<int>();
            return;
        }
        int clusterIndex = clusterNum - 1;
        int numMinor = level2[clusterIndex].Length;
        indices = new List<int>();
        for (int i = 0; i < numMinor; i++) {
            // if not already done that minor obj, add it to list
            if (!minorProg.Contains(clusterNum * 10 + i + 1)) {
                minorArrowTransforms[2 * (clusterNum - 1) + i].gameObject.SetActive(true);
                indices.Add(2 * (clusterNum - 1) + i);
            }
        }
    }

    private void SetRespawnSector(Vector3 _, int sectorNum) {
        respawnSector = sectorNum;
    }

    private void DeactivateAll() {
        HideMajor();
        HideMinor();
    }

    private void RespawnAlignment() {
        ShowMajor();
        EnteringCluster(respawnSector);
    }

    // calculate radius, re-called on camera rect change
    public void CalculateAnchorRadius(Rect cameraRect) {
        return;
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        float aspect = cam.aspect;
        RectTransform canvasTransform = transform.parent.gameObject.GetComponent<RectTransform>();
        RectTransform compassTransform = gameObject.GetComponent<RectTransform>();
        if (aspect >= 1) { // landscape screen
            float insetDistance = (canvasTransform.rect.width - canvasTransform.rect.height) / 2;
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                insetDistance,
                canvasTransform.rect.width - 2 * insetDistance);
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                0,
                canvasTransform.rect.height);
        } else { // portrait
            float insetDistance = (canvasTransform.rect.height - canvasTransform.rect.width) / 2;
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                insetDistance,
                canvasTransform.rect.height - 2 * insetDistance);
            compassTransform.SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Left,
                0,
                canvasTransform.rect.width);
        }
    }

    private Vector2 DiffToCompassSpace(Vector2 diff) {
        // return a normalized anchor vector weighted from the center
        return (Vector2.one / 2) + diff.normalized * cameraRatio;
    }

    // update compass arrows
    void Update() {
        if (majorShown) {
            Vector2 majorDiff = major - playerRB.position;
            // major
            Vector2 majorArrowAnchor = DiffToCompassSpace(majorDiff);
            majorArrowTransform.anchorMin = majorArrowAnchor;
            majorArrowTransform.anchorMax = majorArrowAnchor;
            majorArrowTransform.anchoredPosition = Vector2.zero;
            majorArrowTransform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, majorDiff));
            // minor
            if (minorShown && currCluster > 0) {
                for (int i = 0; i < indices.Count; i++) {
                    Vector2 minorDiff = level2[currCluster - 1][indices[i] % 2].getCorePosition() - playerRB.position;

                    Vector2 minorArrowAnchor = DiffToCompassSpace(minorDiff);
                    minorArrowTransforms[indices[i]].anchorMin = minorArrowAnchor;
                    minorArrowTransforms[indices[i]].anchorMax = minorArrowAnchor;
                    minorArrowTransforms[indices[i]].anchoredPosition = Vector2.zero;
                    minorArrowTransforms[indices[i]].localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, minorDiff));
                }
            }
        }
    }

    public void DestroyAllArrows() {
        majorShown = false;
        minorShown = false;
        if (majorArrowTransform) {
            Destroy(majorArrowTransform.gameObject);
        }
        for (int i = 0; i < minorArrowTransforms.Length; i++) {
            Destroy(minorArrowTransforms[i].gameObject);
        }
        majorProg.Clear();
        minorProg.Clear();

        EventManager.onArtifactPickup -= UpdateProgression;
        EventManager.onEnterCluster -= EnteringCluster;
        EventManager.onExitCluster -= LeavingCluster;
        EventManager.onEnterBossArea -= HideCompass;
        EventManager.onExitBossArea -= ShowCompassCheck;
        EventManager.onSetSpawn -= SetRespawnSector;
        EventManager.onPlayerDeath -= DeactivateAll;
        EventManager.onPlayerRespawn -= RespawnAlignment;
    }
}
