using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    private float duration;

    void Start() {
        Destroy(gameObject, duration);
    }
    
    public void SetDuration(float dur) {
        duration = dur;
    }
}
