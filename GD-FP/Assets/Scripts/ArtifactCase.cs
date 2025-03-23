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
    private bool a1 = false;
    private bool a2 = false;
    private bool a3 = false;
    private bool a4 = false;
    private bool a5 = false;

    void Start() {
        rectTransform = gameObject.GetComponent<RectTransform>();
        EventManager.onArtifactObtain += FillKnowledge;
    }

    public void FillKnowledge(int index) {
        Timing.RunCoroutine(_BookSequence(index));
    }

    private IEnumerator<float> _BookSequence(int index) {
        // remove sector 6 block if 5 artifacts found
        if (index == 1) {
            a1 = true;
        } else if (index == 2) {
            a2 = true;
        } else if (index == 3) {
            a3 = true;
        } else if (index == 4) {
            a4 = true;
        } else if (index == 5) {
            a5 = true;
        }

        if (a1 && a2 && a3 && a4 && a5) {
            GameController.fiveArtifactsReclaimed = true;
        }

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
        transform.GetChild(index - 1).gameObject.SetActive(false);

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
