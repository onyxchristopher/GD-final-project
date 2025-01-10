using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generation : MonoBehaviour
{
    [SerializeField] private float largeGridSize;
    [SerializeField] private float smallGridSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Vector2 totalSize;
    [SerializeField] private int numLargeClusters;
    [SerializeField] private Vector2 largeClusterSize;
    [SerializeField] private int numSmallClusters;
    [SerializeField] private Vector2 smallClusterSize;
    
    [SerializeField] private Vector2 coreLocation;
    [SerializeField] private Vector2 coreSize;

    public void generate() {
        // initialize the root core
        Rect universe = new Rect(offset, totalSize * largeGridSize);
        Vector2 corePosition = calculateCorePosition(universe, coreLocation);
        
        Cluster root = new Cluster(0, universe, corePosition, null);

        // make cluster bounds
        Rect[] largeClusterRects = boundingRects(
            largeGridSize,
            offset,
            totalSize,
            numLargeClusters,
            largeClusterSize,
            coreLocation,
            coreSize);
        if (largeClusterRects.Length != numLargeClusters) {
            Debug.Log("Invalid parameters");
        }

        // create level 1 Cluster objects
        Cluster[] level1Clusters = new Cluster[numLargeClusters];
        for (int i = 0; i < numLargeClusters; i++) {
            level1Clusters[i] = new Cluster(
                1,
                largeClusterRects[i],
                calculateCorePosition(largeClusterRects[i], coreLocation),
                root);
        }

        // order level 1 Cluster objects
        Cluster[] orderedLevel1Clusters = orderClusters(level1Clusters, numLargeClusters, root);

        visualizeClusters(orderedLevel1Clusters, largeGridSize * totalSize, offset);

        // level 2

        Cluster[][] level2Clusters = new Cluster[numLargeClusters][];

        for (int i = 0; i < numLargeClusters; i++) {
            // initilize subcluster array and parent
            Cluster[] subclusters = new Cluster[numSmallClusters];
            Cluster parent = orderedLevel1Clusters[i];

            // calculate offset for rect clustering method
            Vector2 smallOffset = parent.getCorePosition();
            smallOffset = smallOffset - parent.getBounds().size / 2;

            // get rectangles
            Rect[] smallClusterRects = boundingRects(
                smallGridSize,
                smallOffset,
                parent.getBounds().size,
                numSmallClusters,
                smallClusterSize,
                coreLocation,
                coreSize
            );
            if (smallClusterRects.Length != numSmallClusters) {
                Debug.Log("Invalid parameters");
            }

            // initialize subclusters
            for (int j = 0; j < numSmallClusters; j++) {
                subclusters[j] = new Cluster(
                    2,
                    smallClusterRects[j],
                    calculateCorePosition(smallClusterRects[j], coreLocation),
                    parent);
            }

            // add the subcluster array to the main array
            level2Clusters[i] = subclusters;
        }
    }

    /*
    float gridSize, the size of the grid squares
    
    Vector2 offset, the offset of the grid
    
    Vector2 totalSize, the total size of the generated area in grid-chunks

    int numClusters, the number of clusters

    Vector2 clusterSize, the size of each cluster in grid-chunks

    Vector2 coreLocation, the normalized location of the core relative to the bottom-left

    Vector2 coreSize, the normalized size of the core

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
        Vector2 coreSize,
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

        float coreWidth = totalSize.x * coreSize.x;
        float coreHeight = totalSize.y * coreSize.y;

        Rect coreBounds = new Rect(
            core.x - coreWidth / 2,
            core.y - coreHeight / 2,
            coreWidth,
            coreHeight
        );
        
        // place random points in bounding rect set back from edge
        int outerIterations = 0;
        int maxIterations = 100;
        bool overlap = true;
        while (overlap && outerIterations < maxIterations){
            overlap = false;
            for (int i = 0; i < numClusters; i++) {
                Rect rect = randomizeCluster(zeroRect, clusterSize);
                // re-randomize if there is overlap
                int innerIterations = 0;
                while (checkOverlap(clusters, i, rect, coreBounds) && innerIterations < maxIterations) {
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
        if (outerIterations >= maxIterations) {
            Debug.Log("Arrangement not found");
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

    // helper for returning random rectangles within the bounding box
    private Rect randomizeCluster(Rect zeroRect, Vector2 clusterSize) {
        float xCoord = Random.Range(0, zeroRect.xMax - clusterSize.x);
        float yCoord = Random.Range(0, zeroRect.yMax - clusterSize.y);
        return new Rect(xCoord, yCoord, clusterSize.x, clusterSize.y);
    }

    // helper for checking the overlap of the random rectangle with others and the core's
    private bool checkOverlap(Rect[] clusters, int numClusters, Rect rect, Rect coreBounds) {
        if (rect.Overlaps(coreBounds)) {
            return true;
        }
        for (int i = 0; i < numClusters; i++) {
            if (clusters[i].Overlaps(rect)) {
                return true;
            }
        }
        return false;
    }

    // helper to calculate core position from bounds and location
    private Vector2 calculateCorePosition(Rect bounds, Vector2 coreLocation) {
        return bounds.position + Vector2.right * bounds.width * coreLocation.x + Vector2.up * bounds.height * coreLocation.y;
    }

    private Cluster[] orderClusters(Cluster[] unorderedClusters, int numClusters, Cluster root) {
        // sort Clusters in an order s.t. consecutive clusters are close together
        Cluster[] orderedClusters = new Cluster[numClusters];

        List<Cluster> ordered = new List<Cluster>();

        for (int i = 0; i < numClusters; i++) {
            if (i == 0) {
                ordered = unorderedClusters.OrderBy(
                    x => (x.getCorePosition() - root.getCorePosition()).magnitude
                ).ToList();
                orderedClusters[0] = ordered[0];
            } else {
                ordered = ordered.OrderBy(
                    x => (x.getCorePosition() - orderedClusters[i - 1].getCorePosition()).magnitude
                ).ToList();
                ordered.RemoveAt(0);
                orderedClusters[i] = ordered[0];
            }
            orderedClusters[i].setId(i);
        }

        return orderedClusters;
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
            Debug.DrawLine(bottomLeft, bottomRight, color, 10+clusters[i].getId(), false);
            Debug.DrawLine(bottomRight, topRight, color, 10+clusters[i].getId(), false);
            Debug.DrawLine(topRight, topLeft, color, 10+clusters[i].getId(), false);
            Debug.DrawLine(topLeft, bottomLeft, color, 10+clusters[i].getId(), false);
            /*Debug.DrawLine((Vector3) clusters[i].getCorePosition(),
            (Vector3) clusters[i].getParentCore().getCorePosition(),
            Color.white, 10+clusters[i].getId(), false);*/
            if (i == 0) {
                Debug.DrawLine((Vector3) clusters[0].getCorePosition(),
                (Vector3) clusters[0].getParentCore().getCorePosition(),
                Color.yellow, 15+clusters[i].getId(), false);
            } else {
                Debug.DrawLine((Vector3) clusters[i-1].getCorePosition(),
                (Vector3) clusters[i].getCorePosition(),
                Color.yellow, 15+clusters[i].getId(), false);
            }
        }
    }
}