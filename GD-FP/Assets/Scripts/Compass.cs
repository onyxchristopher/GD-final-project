using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

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

    // the vector locations of all nearby minor objectives
    private List<int> indices = new List<int>();

    // player rigidbody
    private Rigidbody2D playerRB;

    // Progression tracker
    private List<int> majorProg = new List<int>();
    private List<int> minorProg = new List<int>();

    // Compass arrow references
    private RectTransform majorArrowTransform;
    private RectTransform[] minorArrowTransforms = new RectTransform[16];

    // Visual elements
    [SerializeField] private GameObject majorCompassArrow;
    [SerializeField] private GameObject minorCompassArrow;

    private bool majorShown = true;
    private bool minorShown = false;

    void Start()
    {
        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        EventManager.onArtifactPickup += UpdateProgression;
        EventManager.onEnterCluster += EnteringCluster;
        EventManager.onExitCluster += LeavingCluster;
        EventManager.onEnterBossArea += HideCompass;
        EventManager.onExitBossArea += ShowCompass;
        EventManager.onPlayerDeath += ShowCompass;
    }

    public void InitializeCompass(Cluster[] l1, Cluster[][] l2) {
        level1 = l1;
        level2 = l2;
        UpdateMajor(1);

        // create arrows which are children of the compass
        for (int i = 0; i < minorArrowTransforms.Length; i++) {
            GameObject minorArrow = Instantiate(minorCompassArrow, Vector3.zero, Quaternion.identity, transform);
            minorArrowTransforms[i] = minorArrow.GetComponent<RectTransform>();
            minorArrow.GetComponent<Image>().color = Color.clear;
        }

        GameObject majorArrow = Instantiate(majorCompassArrow, Vector3.zero, Quaternion.identity, transform);
        majorArrowTransform = majorArrow.GetComponent<RectTransform>();
        majorArrow.GetComponent<Image>().color = new Color(0, 0.5f, 1, 0.5f);

        majorShown = true;
    }

    public void EnteringCluster(int clusterNum) {
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

    public void HideCompass(string _) {
        HideMajor();
        HideMinor();
        Debug.Log(".");
        for (int i = 0; i < indices.Count; i++) {
            Debug.Log($"{indices[i]}");
        }
    }

    public void ShowCompass() {
        ShowMajor();
        ShowMinor();
    }

    private void ShowMajor() {
        if (!majorShown) {
            majorArrowTransform.gameObject.GetComponent<Animator>().SetTrigger("ShowCompass");
            majorShown = true;
        }
    }

    private void ShowMinor() {
        if (!minorShown) {
            for (int i = 0; i < indices.Count; i++) {
                minorArrowTransforms[indices[i]].gameObject.GetComponent<Animator>().SetTrigger("ShowCompass");
            }
            minorShown = true;
        }
    }

    private void HideMajor() {
        if (majorShown) {
            majorArrowTransform.gameObject.GetComponent<Animator>().SetTrigger("HideCompass");
            majorShown = false;
        }
    }

    private void HideMinor() {
        if (minorShown) {
            for (int i = 0; i < indices.Count; i++) {
                minorArrowTransforms[indices[i]].gameObject.GetComponent<Animator>().SetTrigger("HideCompass");
            }
            minorShown = false;
        }
    }

    public void UpdateProgression(int id) {
        int firstDigit = id / 10;
        int secondDigit = id % 10;

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

    // calculate radius, re-called on camera rect change
    public void CalculateAnchorRadius(Rect cameraRect) {
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

    public void Restart() {
        majorShown = false;
        minorShown = false;
        for (int i = 0; i < transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
