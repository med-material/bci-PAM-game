using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    Animator anim;
    AudioSource audioSource;
    public AudioClip[] audioClips;

    public GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        anim = gameObject.GetComponent<Animator>();
    }

    public void Idle()
    {
        anim.Play("Idle");
    }

    public void NormalSuccess()
    {
        anim.Play("ReelInSuccess");
    }

    public void NormalFailure()
    {
        anim.Play("SubtaskFailure");
    }

    public void ReelIn()
    {
        anim.Play("ReelIn");
    }

    public void ReelOut()
    {
        anim.Play("ReelOut");
    }

    public void AssistedFailure()
    {
        anim.Play("AssistedFailure");
    }

    public void AssistedSuccess(int part)
    {
        if (part == 1)
        {
            anim.Play("AssistedSuccess");
            PlaySound(9);
        }
        else if (part == 2)
        {
            gameController.AssistedSuccess();
        }
    }

    public void Sham(int part)
    {
        if (part == 1)
        {
            PlaySound(4);
            anim.Play("ShamOne");
        }
        else if (part == 2)
        {
            anim.Play("ShamTwo");
            gameController.Sham();
        }
        else if (part == 3)
        {
            PlaySound(4);
            anim.Play("ShamThree");
        }
    }

    void StopFish()
    {
        GameObject.FindGameObjectWithTag("Fish").GetComponent<FishBehaviour>().StopStruggle();
    }

    public void Win()
    {
        anim.Play("Win");
        PlaySound(0);
    }

    public void Lose()
    {
        anim.Play("Failure");
        PlaySound(1);
    }

    void AnimationDone()
    {
        gameController.FeedbackFinished();
    }

    void PlaySound(int clipIndex)
    {
        audioSource.PlayOneShot(audioClips[clipIndex]);
    }
}
