using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generation : MonoBehaviour
{
    [SerializeField] private float largeGridSize;
    [SerializeField] private float smallGridSize;
    [SerializeField] private Vector2 largeOffset;
    [SerializeField] private Vector2 totalSize;
    [SerializeField] private int numLargeClusters;
    [SerializeField] private Vector2 largeClusterSize;
    [SerializeField] private int numSmallClusters;
    [SerializeField] private Vector2 smallClusterSize;
    
    [SerializeField] private Vector2 coreLocation;
    [SerializeField] private Vector2 largeCoreSize;
    [SerializeField] private Vector2 smallCoreSize;

    private Rect[] orderedPrevPathRects;
    private Rect[] orderedNextPathRects;

    public static Vector2 farAway = new Vector2(0, 3000);

    public (Cluster, Cluster[], Cluster[][]) generate(int seed) {
        Random.InitState(seed); // set seed

        // initialize the root core
        Rect universe = new Rect(largeOffset, totalSize * largeGridSize);
        Vector2 corePosition = calculateCorePosition(universe, coreLocation);
        Cluster root = new Cluster(0, universe, corePosition, null);

        // make cluster bounds
        Rect[] largeClusterRects = boundingRects(
            largeGridSize,
            largeOffset,
            totalSize,
            numLargeClusters,
            largeClusterSize,
            coreLocation,
            largeCoreSize
        );

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

        orderedPrevPathRects = new Rect[numLargeClusters];
        orderedNextPathRects = new Rect[numLargeClusters];
        for (int i = 0; i < numLargeClusters; i++) {
            (Rect prevRect, Rect nextRect) = pathCalculator(i, orderedLevel1Clusters);
            orderedPrevPathRects[i] = prevRect;
            orderedNextPathRects[i] = nextRect;
        }

        visualizeClusters(orderedLevel1Clusters, largeGridSize * totalSize, largeOffset);

        // level 2

        Cluster[][] orderedLevel2Clusters = new Cluster[numLargeClusters][];

        for (int i = 0; i < numLargeClusters; i++) {
            // initilize subcluster array and parent
            Cluster[] subclusters = new Cluster[numSmallClusters];
            Cluster parent = orderedLevel1Clusters[i];

            // calculate offset for rect clustering method
            Vector2 smallOffset = parent.getBounds().position;

            // get rectangles
            Rect[] smallClusterRects = boundingRects(
                smallGridSize,
                smallOffset,
                parent.getBounds().size / smallGridSize,
                numSmallClusters,
                smallClusterSize,
                coreLocation,
                smallCoreSize,
                i
            );

            // initialize subclusters
            for (int j = 0; j < numSmallClusters; j++) {
                subclusters[j] = new Cluster(
                    2,
                    smallClusterRects[j],
                    calculateCorePosition(smallClusterRects[j], coreLocation),
                    parent);
            }

            // add the subcluster array to the main array
            orderedLevel2Clusters[i] = orderClusters(subclusters, numSmallClusters, parent);

            visualizeClusters(orderedLevel2Clusters[i], largeClusterSize, smallOffset);
        }

        return (root, orderedLevel1Clusters, orderedLevel2Clusters);
    }

    /*
    float gridSize, the size of the grid squares
    
    Vector2 offset, the offset of the grid
    
    Vector2 boundedSize, the total size of the generated area in grid-chunks

    int numClusters, the number of clusters

    Vector2 clusterSize, the size of each cluster in grid-chunks

    Vector2 coreLocation, the normalized location of the core relative to the bottom-left

    Vector2 coreSize, the normalized size of the core

    int boundsIndex, the index of the bounding rectangle (-1 if top-level)

    RETURNS randomized non-overlapping clusters

    NOTE: this algorithm will fail for tightly-packed, large areas;
    it is best used to generate sparse clusters
    */

    public Rect[] boundingRects(
        float gridSize,
        Vector2 offset,
        Vector2 boundedSize,
        int numClusters,
        Vector2 clusterSize,
        Vector2 coreLocation,
        Vector2 coreSize,
        int boundsIndex = -1) {

        // initialize bounding rect
        Rect zeroRect = new Rect(Vector2.zero, boundedSize);

        // initialize cluster array
        Rect[] clusters = new Rect[numClusters];

        // find core position
        Vector2 core = new Vector2(
            boundedSize.x * coreLocation.x,
            boundedSize.y * coreLocation.y);

        float coreWidth = boundedSize.x * coreSize.x;
        float coreHeight = boundedSize.y * coreSize.y;

        Rect coreBounds = new Rect(
            core.x - coreWidth / 2,
            core.y - coreHeight / 2,
            coreWidth,
            coreHeight
        );
        
        // place random points in bounding rect set back from edge
        int outerIterations = 0; // number of inner loops
        int maxIterations = 100; // iteration limit
        bool overlap = true;
        while (overlap && outerIterations < maxIterations) { // end loop if successful or iteration limit exceeded
            overlap = false;
            for (int i = 0; i < numClusters; i++) {
                Rect rect = randomizeCluster(zeroRect, clusterSize); // generate a random rectangle
                // re-randomize until there is no overlap
                int innerIterations = 0; // number of re-randomizations
                while (checkOverlap(clusters, i, rect, coreBounds, boundsIndex, offset) && innerIterations < maxIterations) {
                    rect = randomizeCluster(zeroRect, clusterSize);
                    innerIterations++;
                }
                if (innerIterations >= maxIterations) {
                    overlap = true; // tells the outer loop to rerandomize
                }
                clusters[i] = rect; // places the rect in the main array
            }
            outerIterations++;
        }
        if (outerIterations >= maxIterations) {
            Debug.Log("Arrangement not found");
        }
        
        // rescale clusters to be in world space rather than algorithm space
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
    private bool checkOverlap(Rect[] clusters, int numClusters, Rect rect, Rect coreBounds, int boundsIndex, Vector2 smallOffset) {
        // checks overlap with the core
        if (rect.Overlaps(coreBounds)) {
            return true;
        }
        // checks overlap with other rectangles
        for (int i = 0; i < numClusters; i++) {
            if (clusters[i].Overlaps(rect)) {
                return true;
            }
        }
        // checks overlap with paths
        if (boundsIndex >= 0) {
            Rect transformedRect = new Rect(rect.position * smallGridSize + smallOffset, rect.size * smallGridSize);
            if (transformedRect.Overlaps(orderedPrevPathRects[boundsIndex])
            || transformedRect.Overlaps(orderedNextPathRects[boundsIndex])) {
                return true;
            }
        }
        return false;
    }

    // helper to calculate core position from bounds and location
    private Vector2 calculateCorePosition(Rect bounds, Vector2 coreLocation) {
        return bounds.position + Vector2.right * bounds.width * coreLocation.x + Vector2.up * bounds.height * coreLocation.y;
    }

    private (Rect, Rect) pathCalculator(int clusterIndex, Cluster[] orderedl1) {
        Vector2 prevCore;
        Vector2 currentCore = orderedl1[clusterIndex].getCorePosition();
        Vector2 nextCore;
        // finds the previous and next core
        if (clusterIndex == 0) {
            prevCore = Vector2.zero;
            nextCore = orderedl1[clusterIndex + 1].getCorePosition();
        } else if (clusterIndex == numLargeClusters - 1) {
            prevCore = orderedl1[clusterIndex - 1].getCorePosition();
            nextCore = Vector2.zero;
        } else {
            prevCore = orderedl1[clusterIndex - 1].getCorePosition();
            nextCore = orderedl1[clusterIndex + 1].getCorePosition();
        }

        // finds the paths connecting the cores
        Vector2 currToPrev = prevCore - currentCore; 
        Vector2 currToNext = nextCore - currentCore;
        
        currToPrev = currToPrev * 0.5f / Mathf.Max(Mathf.Abs(currToPrev.x), Mathf.Abs(currToPrev.y));
        currToNext = currToNext * 0.5f / Mathf.Max(Mathf.Abs(currToNext.x), Mathf.Abs(currToNext.y));
        currToPrev = Rect.NormalizedToPoint(orderedl1[clusterIndex].getBounds(), currToPrev + Vector2.one / 2);
        currToNext = Rect.NormalizedToPoint(orderedl1[clusterIndex].getBounds(), currToNext + Vector2.one / 2);
        
        // creates rectangles to represent the paths
        Rect prevRect = Rect.MinMaxRect(
            Mathf.Min(currentCore.x, currToPrev.x),
            Mathf.Min(currentCore.y, currToPrev.y),
            Mathf.Max(currentCore.x, currToPrev.x),
            Mathf.Max(currentCore.y, currToPrev.y)
        );
        Rect nextRect = Rect.MinMaxRect(
            Mathf.Min(currentCore.x, currToNext.x),
            Mathf.Min(currentCore.y, currToNext.y),
            Mathf.Max(currentCore.x, currToNext.x),
            Mathf.Max(currentCore.y, currToNext.y)
        );

        return (prevRect, nextRect);
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
            orderedClusters[i].setId(i + 1);
        }

        return orderedClusters;
    }

    // Debug method for visualizing clusters
    private void visualizeClusters(Cluster[] clusters, Vector2 siz, Vector2 off) {
        /*Vector3 totalBottomLeft = (Vector3) off;
        Vector3 totalBottomRight = totalBottomLeft + siz.x * Vector3.right;
        Vector3 totalTopLeft = totalBottomLeft + siz.y * Vector3.up;
        Vector3 totalTopRight = totalTopLeft + siz.x * Vector3.right;
        Debug.DrawLine(totalBottomLeft, totalBottomRight, Color.red, 20f, false);
        Debug.DrawLine(totalBottomRight, totalTopRight, Color.red, 20f, false);
        Debug.DrawLine(totalTopRight, totalTopLeft, Color.red, 20f, false);
        Debug.DrawLine(totalTopLeft, totalBottomLeft, Color.red, 20f, false);*/

        for (int i = 0; i < clusters.Length; i++) {
            Color color = Random.ColorHSV(0.3f, 0.6f, 1f, 1f, 1f, 1f, 1f, 1f);
            Vector3 bottomLeft = (Vector3) (clusters[i].getBounds().position);
            Vector3 bottomRight = bottomLeft + Vector3.right * clusters[i].getBounds().width;
            Vector3 topLeft = bottomLeft + Vector3.up * clusters[i].getBounds().height;
            Vector3 topRight = bottomRight + Vector3.up * clusters[i].getBounds().height;
            Debug.DrawLine(bottomLeft, bottomRight, color, 15+clusters[i].getId(), false);
            Debug.DrawLine(bottomRight, topRight, color, 15+clusters[i].getId(), false);
            Debug.DrawLine(topRight, topLeft, color, 15+clusters[i].getId(), false);
            Debug.DrawLine(topLeft, bottomLeft, color, 15+clusters[i].getId(), false);
            /*
            Debug.DrawLine((Vector3) clusters[i].getCorePosition(),
            (Vector3) clusters[i].getParentCore().getCorePosition(),
            Color.white, 10+clusters[i].getId(), false);
            */
            if (off == largeOffset) {
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
}