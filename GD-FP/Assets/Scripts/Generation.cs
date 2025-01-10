using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generation : MonoBehaviour
{
    [SerializeField] private float gridSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 totalSize;
    [SerializeField] private int numLargeClusters;
    [SerializeField] private Vector2 largeClusterSize;
    [SerializeField] private Vector2 coreLocation;

    // Start is called before the first frame update
    void Start(){
        
    }

    public void generate() {
        // initialize the root core
        Rect universe = new Rect(offset, totalSize * gridSize);
        Vector2 corePosition = calculateCorePosition(universe, coreLocation);
        
        Cluster rootCore = new Cluster(0, universe, corePosition, null);

        // make cluster bounds
        Rect[] largeClusterRects = boundingRects(
            gridSize, offset, totalSize, numLargeClusters, largeClusterSize, coreLocation);
        if (largeClusterRects.Length != numLargeClusters) {
            Debug.Log("Invalid parameters");
        }

        Cluster[] level1Clusters = new Cluster[numLargeClusters];
        for (int i = 0; i < numLargeClusters; i++) {
            level1Clusters[i] = new Cluster(1,
            largeClusterRects[i],
            calculateCorePosition(largeClusterRects[i], coreLocation),
            rootCore);
        }

        Cluster[] sortedLevel1Clusters = level1Clusters.OrderBy(
            x => (x.getCorePosition() - rootCore.getCorePosition()).magnitude
        ).ToArray();

        for (int i = 0; i < numLargeClusters; i++) {
            sortedLevel1Clusters[i].setId(i);
        }

        visualizeClusters(sortedLevel1Clusters, gridSize * totalSize, offset);
    }

    /*
    float gridSize, the size of the grid squares
    
    Vector2 offset, the offset of the grid
    
    Vector2 totalSize, the total size of the generated area in grid-chunks

    int numClusters, the number of clusters

    Vector2 clusterSize, the size of each cluster in grid-chunks

    Vector2 coreLocation, the normalized location of the core relative to the bottom-left

    int seed, the random seed

    RETURNS randomized non-overlapping clusters

    NOTE: this algorithm will fail for tightly-packed, large areas;
    it is best used to generate sparse clusters
    */

    public Rect[] boundingRects(
        float gridSize,
        Vector2 offset,
        Vector2 totalSize,
        int numClusters,
        Vector2 clusterSize,
        Vector2 coreLocation,
        int seed = 42) {
        
        // set seed
        Random.InitState(seed);

        // initialize bounding rect
        Rect zeroRect = new Rect(Vector2.zero, totalSize);

        // initialize cluster array
        Rect[] clusters = new Rect[numClusters];

        // find core position
        Vector2 core = new Vector2(
            totalSize.x * coreLocation.x,
            totalSize.y * coreLocation.y);
        
        // place random points in bounding rect set back from edge
        int outerIterations = 0;
        int maxIterations = 50;
        bool overlap = true;
        while (overlap && outerIterations < maxIterations){
            overlap = false;
            for (int i = 0; i < numClusters; i++) {
                Rect rect = randomizeCluster(zeroRect, clusterSize);
                // re-randomize if there is overlap
                int innerIterations = 0;
                while (checkOverlap(clusters, i, rect, core) && innerIterations < maxIterations) {
                    rect = randomizeCluster(zeroRect, clusterSize);
                    innerIterations++;
                }
                if (innerIterations >= maxIterations) {
                    overlap = true;
                }
                clusters[i] = rect;
            }
            outerIterations++;
        }
        
        
        // rescale clusters to grid and offset
        Rect[] scaledClusters = new Rect[numClusters];

        for (int i = 0; i < numClusters; i++) {
            scaledClusters[i] = new Rect(
                clusters[i].position * gridSize + offset,
                clusters[i].size * gridSize);
        }
        
        return scaledClusters;
    }

    private Rect randomizeCluster(Rect zeroRect, Vector2 clusterSize) {
        float xCoord = Random.Range(0, zeroRect.xMax - clusterSize.x);
        float yCoord = Random.Range(0, zeroRect.yMax - clusterSize.y);
        return new Rect(xCoord, yCoord, clusterSize.x, clusterSize.y);
    }

    private bool checkOverlap(Rect[] clusters, int numClusters, Rect rect, Vector2 coreLocation) {
        bool overlap = false;
        if (rect.Contains(coreLocation)) {
            overlap = true;
            return overlap;
        }
        for (int i = 0; i < numClusters; i++) {
            if (clusters[i].Overlaps(rect)) {
                overlap = true;
                break;
            }
        }
        return overlap;
    }

    private Vector2 calculateCorePosition(Rect bounds, Vector2 coreLocation) {
        return bounds.position + Vector2.right * bounds.width * coreLocation.x + Vector2.up * bounds.height * coreLocation.y;
    }

    // Debug method for visualizing clusters
    private void visualizeClusters(Cluster[] clusters, Vector2 totalSize, Vector2 offset) {
        Vector3 totalBottomLeft = (Vector3) offset;
        Vector3 totalBottomRight = totalBottomLeft + totalSize.x * Vector3.right;
        Vector3 totalTopLeft = totalBottomLeft + totalSize.y * Vector3.up;
        Vector3 totalTopRight = totalTopLeft + totalSize.x * Vector3.right;
        Debug.DrawLine(totalBottomLeft, totalBottomRight, Color.red, 20f, false);
        Debug.DrawLine(totalBottomRight, totalTopRight, Color.red, 20f, false);
        Debug.DrawLine(totalTopRight, totalTopLeft, Color.red, 20f, false);
        Debug.DrawLine(totalTopLeft, totalBottomLeft, Color.red, 20f, false);

        for (int i = 0; i < clusters.Length; i++) {
            Color color = Random.ColorHSV(0.3f, 0.6f, 1f, 1f, 1f, 1f, 1f, 1f);
            Vector3 bottomLeft = (Vector3) (clusters[i].getBounds().position);
            Vector3 bottomRight = bottomLeft + Vector3.right * clusters[i].getBounds().width;
            Vector3 topLeft = bottomLeft + Vector3.up * clusters[i].getBounds().height;
            Vector3 topRight = bottomRight + Vector3.up * clusters[i].getBounds().height;
            Debug.DrawLine(bottomLeft, bottomRight, color, 20f, false);
            Debug.DrawLine(bottomRight, topRight, color, 20f, false);
            Debug.DrawLine(topRight, topLeft, color, 20f, false);
            Debug.DrawLine(topLeft, bottomLeft, color, 20f, false);
        }
    }
}
