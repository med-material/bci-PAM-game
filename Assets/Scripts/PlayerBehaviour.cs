using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    Animator anim;
    AudioSource audioSource;
    public AudioClip[] audioClips;

    public GameController gameController;

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
        anim.Play("SubtaskSuccess");
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

    public void MitigatedFailure()
    {
        anim.Play("MitigatedFailure");
    }

    public void AugmentedSuccess(int part)
    {
        if (part == 1)
        {
            anim.Play("AugmentedSuccess");
            PlaySound(9);
        }
        else if (part == 2)
        {
            gameController.AugmentedSuccess();
        }
    }

    public void OverrideInput(int part)
    {
        if (part == 1)
        {
            PlaySound(4);
            anim.Play("OI1");
        }
        else if (part == 2)
        {
            anim.Play("OI2");
            gameController.OverrideInput();
        }
        else if (part == 3)
        {
            PlaySound(4);
            anim.Play("OI3");
        }
    }

    // Referenced in animation
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

    // Referenced in animation
    void AnimationDone()
    {
        gameController.FeedbackFinished();
    }

    void PlaySound(int clipIndex)
    {
        audioSource.PlayOneShot(audioClips[clipIndex]);
    }
}
