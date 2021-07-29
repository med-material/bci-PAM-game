using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBehaviour : MonoBehaviour
{
    public Transform endRodPos;
    public Transform hookPos;

    void Start()
    {
        gameObject.GetComponent<LineRenderer>().SetPosition(0, endRodPos.transform.position);
        gameObject.GetComponent<LineRenderer>().SetPosition(1, hookPos.transform.position);
    }

    void Update()
    {
        //The line always follows the position of the hook
        gameObject.GetComponent<LineRenderer>().SetPosition(1, hookPos.transform.position);
    }
}
