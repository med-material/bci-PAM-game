using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HookBehaviour : MonoBehaviour
{

    public GameController gc;

    public void Move(Vector3 targetPos, bool xAxis, float speed)
    {
        StartCoroutine(MoveHook(targetPos, xAxis, speed));
    }
    
    private IEnumerator MoveHook(Vector3 targetPos, bool xAxis, float speed)
    {
        GetComponent<AudioSource>().Play();
        gc.moving = true;
        
        Vector3 currentPos = transform.position;
        float t = 0;
        float timeToMove;
        
        //The time to move varies based on how far it's moving
        if (xAxis)
            timeToMove = Mathf.Abs(transform.position.x - targetPos.x) / 2;
        else
            timeToMove = Mathf.Abs(transform.position.y - targetPos.y) / 2;

        while (t < 1)
        {
            t += Time.deltaTime / (timeToMove * speed);
            transform.position = Vector3.Lerp(currentPos, targetPos, t);
            yield return null;
        }

        gameObject.transform.localPosition = targetPos;

        GetComponent<AudioSource>().Stop();
        gc.FeedbackFinished();
    }
}
