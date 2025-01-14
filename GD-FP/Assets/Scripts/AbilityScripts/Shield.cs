using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    private float duration;
    
    public void SetDuration(float dur) {
        duration = dur;
        Destroy(gameObject, duration);
    }
}
