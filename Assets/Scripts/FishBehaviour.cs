﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class FishBehaviour : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip[] clips;

    Coroutine swim;
    Animator anim;

    bool leftToRight;
    Vector3 startPos;
    Vector3 exitPoint;

    GameController gc;
    public bool escaped = false;
    bool onHook;

    void Start()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        startPos = transform.position;
        exitPoint = new Vector3(-startPos.x, startPos.y);

        if (gameObject.transform.position.x < 0)
        {
            leftToRight = true;
        }
        else //If fish is swimming in from the right, mirror it around the x-axis
        {
            leftToRight = false;
            gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
        }

        StartSwimming(startPos, exitPoint, 0.8f);
    }

    void Update()
    {
        if (((leftToRight && transform.position.x > 0) || (!leftToRight && transform.position.x < 0))
            && !escaped && !onHook)
        {
            StopSwimming();
            Escape();
        }
    }

    public void StartSwimming(Vector3 startPos, Vector3 targetPos, float speed)
    {
        swim = StartCoroutine(Swim(startPos, targetPos, speed));
    }

    private IEnumerator Swim(Vector3 startPos, Vector3 targetPos, float speed)
    {
        anim.Play("Swimming");
        float t = 0;

        float timeToMove = Mathf.Abs(startPos.x - targetPos.x) / 2;

        while (t < 1)
        {
            t += (Time.deltaTime / timeToMove) * speed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        if (escaped && !onHook)
        {
            gc.SpawnFish();
            Goodbye();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Hook")
        {
            //Fish has to move with the hook, so we set it to its parent
            transform.SetParent(other.GetComponent<Transform>());

            PlaySound(2, 1);
            anim.Play("Struggle");
            StopSwimming();
            onHook = true;

            gc.BCIInputStart();
        }
    }

    void StopSwimming()
    {
        StopCoroutine(swim);
    }

    public void Struggle()
    {
        PlaySound(0, 0.3f);
        PlaySound(1, 0.3f);

        anim.speed = 4;
    }

    public void StopStruggle()
    {
        anim.speed = 1;
    }

    //When the fish is lost, mirror around the x-axis, then swim away
    public void Escape()
    {
        escaped = true;
        anim.Play("Swimming");
        anim.speed = 2;

        if (onHook)
        {
            gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
            StartSwimming(transform.position, new Vector3(startPos.x, transform.position.y), 2);
            transform.SetParent(null);
        }
        else
        {
            StartSwimming(transform.position, new Vector3(exitPoint.x, transform.position.y), 2);
        }
    }

    public void Goodbye()
    {
        Destroy(gameObject);
    }

    void PlaySound(int clipIndex, float volume)
    {
        audioSource.PlayOneShot(clips[clipIndex], volume);
    }
}
