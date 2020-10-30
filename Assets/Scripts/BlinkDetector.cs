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
    EyesClosed,
    Unintialized
}

public enum DetectorState
{
    Started,
    Stopped
}

public class BlinkDetector : MonoBehaviour
{
    private EyeState eyeState = EyeState.Unintialized;
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
            // If eyes are OPEN
            //Debug.Log(TobiiAPI.GetGazePoint().IsRecent(0.1f));
            //Debug.Log("eyestate: " + Enum.GetName(typeof(EyeState), eyeState));

            if (Input.GetKeyUp(KeyCode.Space)) // for debugging without eyetracker
            //if (TobiiAPI.GetGazePoint().IsRecent(0.1f))
            {
                if (eyeState == EyeState.EyesClosed || eyeState == EyeState.Unintialized)
                {
                    eyeState = EyeState.EyesOpen;
                    LogEyeOpen();
                    blinkNo++;
                    InputData inputData = new InputData();
                    inputData.validity = InputValidity.Accepted;
                    inputData.type = InputType.BlinkDetection;
                    inputData.confidence = 1f;
                    inputData.inputNumber = blinkNo;
                    onBlink.Invoke(inputData);
                }
                duration = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.Space)) // for debugging without eyetracker
            //else
            {
                if (eyeState == EyeState.EyesOpen || eyeState == EyeState.Unintialized)
                {
                    eyeState = EyeState.EyesClosed;
                    timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                    LogEyeClose();
                }
                duration += Time.deltaTime;
            }
        }
    }

    private void LogEyeOpen()
    {
        loggingManager.Log("BlinkLog", "Event", "EyeOpening");
        loggingManager.Log("BlinkLog", "BlinkNo", blinkNo);
        loggingManager.Log("BlinkLog", "DurationClosed_s", duration);
        loggingManager.SaveLog("BlinkLog");
        loggingManager.ClearLog("BlinkLog");
    }

    private void LogEyeClose()
    {
        //loggingManager.Log("BlinkLog", "TimestampEye", timestamp);
        loggingManager.Log("BlinkLog", "Event", "EyeClosing");
        loggingManager.Log("BlinkLog", "BlinkNo", blinkNo);
        loggingManager.SaveLog("BlinkLog");
        loggingManager.ClearLog("BlinkLog");
    }

    // Added for kiwi-runner to start blink detection
    public void onGameStateChanged(GameData gameData)
    {
        if (gameData.gameState == GameState.Running)
        {
            state = DetectorState.Started;
        }
        else if (gameData.gameState == GameState.Stopped)
            //gameData.gameState == GameState.Paused)
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