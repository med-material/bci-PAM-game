using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Tobii.Gaming;
using System;
using System.IO;

public enum EyeState
{
    EyesOpen,
    EyesClosed
}

public enum DetectorState
{
    Started,
    Stopped
}

public class BlinkDetector : MonoBehaviour
{
    private EyeState eyeState = EyeState.EyesOpen;
    private DetectorState state = DetectorState.Stopped;

    private float duration = 0f;
    private string timestamp = "Unavailable";

    [Serializable]
    public class OnBlink : UnityEvent<InputData> { }
    public OnBlink onBlink;

    private int blinkNo = 0;

    // TODO, get where people are looking?

    private LoggingManager loggingManager;

    void Start()
    {
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();
    }

    void Update()
    {
        if (state == DetectorState.Started)
        {
            // If eyes are CLOSED
            if (!TobiiAPI.GetGazePoint().IsRecent(0.1f))
            //if (Input.GetKeyDown(KeyCode.W)) // for testing without eye tracker
            {
                if (eyeState == EyeState.EyesOpen)
                {
                    eyeState = EyeState.EyesClosed;
                    LogBlink();
                }
                duration = 0f;
            }
            // if eyes are OPEN
            else
            //else if (Input.GetKeyUp(KeyCode.W)) // for testing without eye tracker
            {
                if (eyeState == EyeState.EyesClosed)
                {
                    eyeState = EyeState.EyesOpen;
                    timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "bleh";
                    blinkNo++;
                    InputData inputData = new InputData();
                    inputData.validity = InputValidity.Accepted;
                    inputData.type = InputType.BlinkDetection;
                    inputData.confidence = 1f;
                    inputData.inputNumber = blinkNo;
                    onBlink.Invoke(inputData);
                }
                duration += Time.deltaTime;
            }
        }
    }

    private void LogBlink()
    {
        loggingManager.Log("BlinkLog", "Timestamp", timestamp); // NOTE: THIS TIMESTAMP IS BEING OVERWRITTEN!!!
        loggingManager.Log("BlinkLog", "Event", "Blink");
        loggingManager.Log("BlinkLog", "Duration_s", duration);
        loggingManager.Log("BlinkLog", "BlinkNo", blinkNo);
        loggingManager.SaveLog("BlinkLog");
        loggingManager.ClearLog("BlinkLog");
    }

    public void onGameStateChanged(GameData gameData)
    {
        if (gameData.gameState == GameState.Running)
        {
            state = DetectorState.Started;
        }
        else if (gameData.gameState == GameState.Paused)
        {
            state = DetectorState.Stopped;
        }
    }

    public void StartBlinkDetection()
    {
        state = DetectorState.Started;
    }

    public void StopBlinkDetection()
    {
        state = DetectorState.Stopped;
    }
}