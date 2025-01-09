using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start(){
        
    }

    public void generate() {

        float gridSize = 10;
        Vector2 offset = Vector2.zero;
        Vector2Int totalSize = new Vector2Int(25, 25);
        int numLargeClusters = 8;
        Vector2Int largeClusterSize = new Vector2Int(5, 5);
        coreLocation = Vector2.zero;

        Rect[] largeClusters = rectClusters(gridSize, offset, totalSize, numLargeClusters, largeClusterSize);
        // visualizeClusters(largeClusters, gridSize * (Vector2) totalSize, offset);
    }

    /*
    float gridSize, the size of the grid squares
    
    Vector2 offset, the offset of the grid
    
    Vector2Int totalSize, the total size of the generated area in grid-chunks

    int numClusters, the number of clusters

    Vector2Int clusterSize, the size of each cluster in grid-chunks

    int seed, the random seed

    NOTE: this algorithm will throw an exception for tightly-packed, large areas;
    it is best used to find sparse clusters
    */

    public Rect[] rectClusters(
        float gridSize,
        Vector2 offset,
        Vector2Int totalSize,
        int numClusters,
        Vector2Int clusterSize,
        int seed = 42) {
        
        // set seed
        Random.InitState(seed);

        // initialize bounding rect
        RectInt zeroRect = new RectInt(Vector2Int.zero, totalSize);

        RectInt[] clusters = new RectInt[numClusters];
        
        // place random points in bounding rect set back from edge
        
        for (int i = 0; i < numClusters; i++) {
            RectInt rect = randomizeCluster(zeroRect, clusterSize);
            // re-randomize if there is overlap
            int iterations = 0;
            int maxIterations = 1000;
            while (checkOverlap(clusters, i, rect) && iterations < maxIterations) {
                rect = randomizeCluster(zeroRect, clusterSize);
                iterations++;
            }
            if (iterations > maxIterations) {
                Debug.Log("Clustering arrangement not found");
            }
            clusters[i] = rect;
        }

        Rect[] scaledClusters = new Rect[numClusters];

        for (int i = 0; i < numClusters; i++) {
            scaledClusters[i] = new Rect(
                (Vector2) clusters[i].position * gridSize + offset,
                (Vector2) clusters[i].size * gridSize);
        }
        
        return scaledClusters;
    }

    private RectInt randomizeCluster(RectInt zeroRect, Vector2Int clusterSize) {
        int xCoord = Random.Range(0, zeroRect.xMax - clusterSize.x + 1);
        int yCoord = Random.Range(0, zeroRect.yMax - clusterSize.y + 1);
        return new RectInt(xCoord, yCoord, clusterSize.x, clusterSize.y);
    }

    private bool checkOverlap(RectInt[] clusters, int numClusters, RectInt rect) {
        bool overlap = false;
        for (int i = 0; i < numClusters; i++) {
            if (clusters[i].Overlaps(rect)) {
                overlap = true;
                break;
            }
        }
        return overlap;
    }

    private void visualizeClusters(Rect[] clusters, Vector2 totalSize, Vector2 offset) {
        for (int i = 0; i < clusters.Length; i++) {
            Color color = Random.ColorHSV(0.3f, 0.6f, 1f, 1f, 1f, 1f, 1f, 1f);
            Vector3 bottomLeft = (Vector3) (clusters[i].position);
            Vector3 bottomRight = (Vector3) (clusters[i].position + Vector2.right * clusters[i].width);
            Vector3 topLeft = (Vector3) (clusters[i].position + Vector2.up * clusters[i].height);
            Vector3 topRight = (Vector3) (clusters[i].position + Vector2.right * clusters[i].width + Vector2.up * clusters[i].height);
            Debug.DrawLine(bottomLeft, bottomRight, color, 20f, false);
            Debug.DrawLine(bottomRight, topRight, color, 20f, false);
            Debug.DrawLine(topRight, topLeft, color, 20f, false);
            Debug.DrawLine(topLeft, bottomLeft, color, 20f, false);
        }
        Vector3 totalBottomLeft = (Vector3) offset;
        Vector3 totalBottomRight = totalBottomLeft + totalSize.x * Vector3.right;
        Vector3 totalTopLeft = totalBottomLeft + totalSize.y * Vector3.up;
        Vector3 totalTopRight = totalTopLeft + totalSize.x * Vector3.right;
        Debug.DrawLine(totalBottomLeft, totalBottomRight, Color.red, 20f, false);
        Debug.DrawLine(totalBottomRight, totalTopRight, Color.red, 20f, false);
        Debug.DrawLine(totalTopRight, totalTopLeft, Color.red, 20f, false);
        Debug.DrawLine(totalTopLeft, totalBottomLeft, Color.red, 20f, false);
    }
}
