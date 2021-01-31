using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartTheMadness : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public RectTransform[] transformPoints;

    public Vector3 bending = Vector3.up;
    public float timeToTravel = 10f;
    public int attemptsToSucces = 3;



    Vector3 startPosition;
    Vector3 endPosition;
    int currentAttempt = 0;
    bool onPointer = false;
    bool theEnd = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!onPointer && !theEnd)
        {
            GetComponent<Animator>().SetTrigger("Jittering");
            onPointer = true;
            StopAllCoroutines();
            StartCoroutine(MoveToPosition());
            currentAttempt++;
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        onPointer = false;

    }


    public void OnPointerClick(PointerEventData pointerEventData)
    {

        if (currentAttempt >= attemptsToSucces)
        {
            GetComponent<Animator>().SetTrigger("Select");
            theEnd = true;
            StopAllCoroutines();
            Invoke("StartGame", 1f);
        }
        else
        {
            GetComponent<Animator>().SetTrigger("Jittering");
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }



    IEnumerator MoveToPosition()
    {
        int target = Random.Range(0, transformPoints.Length);
        while (GetComponentInParent<RectTransform>().position == transformPoints[target].position)
        {
            target = Random.Range(0, transformPoints.Length);
        }
        startPosition = GetComponentInParent<RectTransform>().position;
        endPosition = transformPoints[target].position;


        float timeStamp = Time.time;
        while (Time.time < timeStamp + timeToTravel)
        {
            Vector3 currentPos = Vector3.Lerp(startPosition, endPosition, (Time.time - timeStamp) / timeToTravel);
            currentPos.x += bending.x*Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * Mathf.PI);
            currentPos.y += bending.y * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * Mathf.PI);
            currentPos.z += bending.z * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * Mathf.PI);
            GetComponentInParent<RectTransform>().position = currentPos;
            yield return new WaitForEndOfFrame();
        }
    }


}
