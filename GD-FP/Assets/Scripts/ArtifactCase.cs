using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public class ArtifactCase : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 offScreenPosition = new Vector3(0, 130, 0);
    private Vector3 onScreenPosition = new Vector3(0, 20, 0);
    private float timeToSlide = 0.8f;
    private float timeBeforeBook = 1;
    private float timeAfterBook = 1;

    void Start() {
        rectTransform = gameObject.GetComponent<RectTransform>();
        EventManager.onArtifactObtain += FillKnowledge;
    }

    public void FillKnowledge(int id) {
        Timing.RunCoroutine(_BookSequence(id));
    }

    private IEnumerator<float> _BookSequence(int id) {
        // slide case down
        float time = 0;
        while (time < timeToSlide) {
            rectTransform.anchoredPosition = Vector3.Lerp(offScreenPosition, onScreenPosition, time / timeToSlide);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        rectTransform.anchoredPosition = onScreenPosition;

        // wait to place book
        yield return Timing.WaitForSeconds(timeBeforeBook);

        // place book
        transform.GetChild(id - 1).gameObject.SetActive(false);

        // wait to slide case up
        yield return Timing.WaitForSeconds(timeAfterBook);

        // slide case up
        time = 0;
        while (time < timeToSlide) {
            rectTransform.anchoredPosition = Vector3.Lerp(onScreenPosition, offScreenPosition, time / timeToSlide);
            yield return Timing.WaitForOneFrame;
            time += Time.deltaTime;
        }
        rectTransform.anchoredPosition = offScreenPosition;
    }

    public void Restart() {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
