using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    bool logged = false;
    
    // Start is called before the first frame update
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

        StartSwimming(startPos, exitPoint, 15);
    }

    void Update()
    {
        //If player didn't manage to hook the fish and it has swum (?) off screen, spawn a new one and destroy this one
        if((leftToRight && transform.position.x > exitPoint.x - 0.1) || (!leftToRight && transform.position.x < exitPoint.x + 0.1))
        {
            gc.SpawnFish();
            Goodbye();
        }
    }

    public void StartSwimming(Vector3 startPos, Vector3 targetPos, float timeToMove)
    {
        swim = StartCoroutine(Swim(startPos, targetPos, timeToMove));
    }

    private IEnumerator Swim(Vector3 startPos, Vector3 targetPos, float timeToMove)
    {
        anim.Play("Swimming");
        float t = 0;

        while(t < 1)
        {
            t += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            //If player hasn't managed to hook the fish (i.e. it has passed the position of the hook), speed it up
            if((leftToRight && transform.position.x > 0) || (!leftToRight && transform.position.x < 0))
            {
                timeToMove = 7;
                anim.speed = 2;

                if (!logged)
                {
                    gc.narrativeEvent = NarrativeEvent.FishMissed;
                    GameEventData gameEvent = gc.CreateGameEventData();
                    gc.onGameEventChanged.Invoke(gameEvent);
                    logged = true;
                }
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Hook")
        {
            //Fish has to move with the hook, so we set it to its parent
            transform.SetParent(other.GetComponent<Transform>());

            PlaySound(2, 1);
            StopSwimming();

            gc.BCIInputStart();
        }
    }

    void StopSwimming()
    {
        anim.Play("Struggle");
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

    public void Reveal(Sprite fishSprite)
    {
        anim.enabled = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = fishSprite;
    }

    //When the fish is lost, mirror around the x-axis, then swim away
    public void Escape()
    {
        anim.Play("Swimming");

        gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y);
        StartSwimming(transform.position, new Vector3(startPos.x, transform.position.y), 2);
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
