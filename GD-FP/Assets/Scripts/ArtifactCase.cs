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
    private bool a1found = false;
    private bool a2found = false;
    private bool a3found = false;
    private bool a4found = false;
    private bool a5found = false;

    void Start() {
        rectTransform = gameObject.GetComponent<RectTransform>();
        EventManager.onArtifactObtain += FillKnowledge;
        EventManager.onPlayAgain += Restart;
    }

    public void FillKnowledge(int index) {
        Timing.RunCoroutine(_BookSequence(index));
    }

    private IEnumerator<float> _BookSequence(int index) {
        // sector 6 is blocked by a forcefield which is removed when all artifacts are found
        if (index == 1) {
            a1found = true;
        } else if (index == 2) {
            a2found = true;
        } else if (index == 3) {
            a3found = true;
        } else if (index == 4) {
            a4found = true;
        } else if (index == 5) {
            a5found = true;
        }

        if (a1found && a2found && a3found && a4found && a5found) { // if five artifacts found
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

        if (index == 6) {
            EventManager.EndGame();
        }
    }

    public void Restart() {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(true);
            a1found = false;
            a2found = false;
            a3found = false;
            a4found = false;
            a5found = false;
        }
    }
}
