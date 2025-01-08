using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    

    // Start is called before the first frame update
    void Start(){
        
    }

    /*
    float gridSize, the size of the grid squares

    
    Vector2 offset, the offset of the grid
    
    Vector2Int totalSize, the total size of the generated area in grid-chunks

    int numClusters, the number of clusters

    Vector2Int clusterSize, the size of each cluster in grid-chunks

    Vector2Int coreLocation, the location of the cluster core

    NOTE: this algorithm is inefficient for tightly-packed, large-sized grids;
    it is best used for sparse cluster sizes
    */
    public RectInt[] generate(
        float gridSize,
        Vector2 offset,
        Vector2Int totalSize,
        int numClusters,
        Vector2Int clusterSize,
        Vector2Int coreLocation,
        int seed) {
        
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
            while (checkOverlap(clusters, numClusters, rect) || iterations > maxIterations) {
                rect = randomizeCluster(zeroRect, clusterSize);
                iterations++
            }
            if (iterations > maxIterations) {
                throw new InvalidOperationException(
                    "Clustering arrangement not found.")
            }
            clusters[i] = rect;
        }
        
        return clusters;
    }

    private RectInt randomizeCluster(RectInt zeroRect, Vector2 clusterSize) {
        int xCoord = Random.Range(0, zeroRect.xMax - clusterSize.x + 1);
        int yCoord = Random.Range(0, zeroRect.yMax - clusterSize.y + 1);
        return new RectInt(xCoord, yCoord, clusterSize.x, clusterSize.y);
    }

    private bool checkOverlap(RectInt[] clusters, int numClusters, rect) {
        bool overlap = false;
        for (int i = 0; i < numClusters; i++) {
            if (clusters[i].Overlaps(rect)) {
                overlap = true;
                break;
            }
        }
        return overlap;
    }
}
