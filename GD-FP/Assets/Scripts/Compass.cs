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

    // the radius in non-rounded pixels
    private float radius;

    // First and second-level clusters
    private Cluster[] level1;
    private Cluster[][] level2;

    // the cluster in which the player is currently located
    private int currCluster = 0;

    // the vector location of the next major objective
    private Vector2 major;

    // the vector locations of all nearby minor objectives
    private List<Vector2> minor = new List<Vector2>();

    // player rigidbody
    private Rigidbody2D playerRB;

    // Progression tracker
    private List<int> majorProg = new List<int>();
    private List<int> minorProg = new List<int>();

    // Compass arrow references
    private RectTransform majorArrowTransform;
    private RectTransform[] minorArrowTransforms;

    // Visual elements
    [SerializeField] private GameObject compassArrow;
    private Color majorArrowColor = new Color(0, 0, 1, 0.5f);
    private Color minorArrowColor = new Color(1, 1, 1, 0.5f);

    // Whether to display the compass
    private bool showCompass = false;

    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();

        CalculatePixelRadius(cam.pixelRect);

        EventManager.onArtifactPickup += UpdateProgression;
        EventManager.onEnterCluster += UpdateMinor;
    }

    public void InitializeCompass(Cluster[] l1, Cluster[][] l2) {
        level1 = l1;
        level2 = l2;
        UpdateMajor(1);

        // create arrows which are children of the compass
        GameObject majorArrow = Instantiate(compassArrow, Vector3.zero, Quaternion.identity, transform);
        majorArrowTransform = majorArrow.GetComponent<RectTransform>();
        majorArrow.GetComponent<Image>().color = majorArrowColor;

        minorArrowTransforms = new RectTransform[level2[0].Length];
        for (int i = 0; i < minorArrowTransforms.Length; i++) {
            GameObject minorArrow = Instantiate(compassArrow, Vector3.zero, Quaternion.identity, transform);
            minorArrowTransforms[i] = minorArrow.GetComponent<RectTransform>();
            minorArrow.GetComponent<Image>().color = Color.clear;
        }
        showCompass = true;
    }

    private void LeavingCluster(int clusterNum) {
        currCluster = 0;
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
            UpdateMinor(firstDigit);
        }
    }

    // point to the next major or zero if not
    private void UpdateMajor(int clusterNum) {
        Debug.Log(clusterNum);
        if (clusterNum < level1.Length) {
            major = level1[clusterNum - 1].getCorePosition();
        } else {
            major = Vector2.zero;
        }
    }

    // point to the minor objectives of the cluster entered
    private void UpdateMinor(int clusterNum) {
        currCluster = clusterNum;
        int clusterIndex = clusterNum - 1;
        int numMinor = level2[clusterIndex].Length;
        minor.Clear();
        for (int i = 0; i < numMinor; i++) {
            if (!minorProg.Contains(clusterNum * 10 + i + 1)) {
                minorArrowTransforms[i].gameObject.GetComponent<Image>().color = minorArrowColor;
                minor.Add(level2[clusterIndex][i].getCorePosition());
            }
        }

        // any remaining unused arrows set opacity 0
        for (int i = minor.Count; i < numMinor; i++) {
            minorArrowTransforms[i].gameObject.GetComponent<Image>().color = Color.clear;
        }
    }

    // calculate radius, re-called on camera rect change
    public void CalculatePixelRadius(Rect cameraRect) {
        if (cam.aspect >= 1) { // landscape screen
            radius = cameraRect.height * cameraRatio;
        } else { // portrait
            radius = cameraRect.width * cameraRatio;
        }
    }

    // convert a diff to a compass vector
    private Vector2 DiffToCompassSpace(Vector2 diff) {
        return radius * diff.normalized;
    }

    // update compass arrows
    void Update() {
        if (showCompass) {
            Vector2 majorDiff = major - playerRB.position;
            // major
            majorArrowTransform.anchoredPosition = DiffToCompassSpace(majorDiff);
            majorArrowTransform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, majorDiff));
            // minor
            if (currCluster > 0) {
                for (int i = 0; i < minor.Count; i++) {
                    Vector2 minorDiff = minor[i] - playerRB.position;
                    minorArrowTransforms[i].anchoredPosition = DiffToCompassSpace(minorDiff);
                    minorArrowTransforms[i].localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, minorDiff));
                }
            }
        }
    }
}
