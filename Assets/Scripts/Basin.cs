using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basin : MonoBehaviour
{
    Animator anim;
    
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        
    }

    void Idle()
    {
        anim.Play("Idle");
    }

    public void Splash()
    {
        GetComponent<AudioSource>().Play();
        anim.Play("Splash");
    }
}
