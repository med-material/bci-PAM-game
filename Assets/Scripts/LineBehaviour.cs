using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBehaviour : MonoBehaviour
{
    public Transform endRodPos;
    public Transform hookPos;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<LineRenderer>().SetPosition(0, endRodPos.transform.position);
        gameObject.GetComponent<LineRenderer>().SetPosition(1, hookPos.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        //The line always follows the position of the hook
        gameObject.GetComponent<LineRenderer>().SetPosition(1, hookPos.transform.position);
    }
}
