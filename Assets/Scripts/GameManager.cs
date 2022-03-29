using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public struct GameData
{
    public int trials;
    public float interTrialIntervalSeconds;
    public float inputWindowSeconds;
    public GameState gameState;
}

public class Mechanism
{
    public string name = "";
    public TrialType trialType;
    public float rate = -1f;
    public int trialsLeft = -1;
    public int trials = -1;
    public UrnEntryBehavior behavior;
}

public class InputData
{
    public InputValidity validity;
    public InputType type;
    public float confidence;
    public int inputNumber;
}

public enum InputValidity
{
    Accepted,
    Rejected
}

public enum InputType
{
    KeySequence,
    MotorImagery,
    BlinkDetection
}

public class GameDecisionData
{
    public TrialType decision;
}

public struct GameTimers
{
    public float inputWindowTimer;
    public float interTrialTimer;
}

public enum InputWindowState
{
    Closed,
    Open,
}

public enum GameState
{
    Running,
    Paused,
    Stopped,
}

public enum TrialType
{
    AccInput,
    //FabInput,
    RejInput,
    AugSuccess,
    MitigateFail,
    OverrideInput
}

public enum Condition
{
    Control,
    OverrideInput,
    AugmentedSucces,
    MitigatedFailure
}

public class GameManager : MonoBehaviour
{

    
    private int rejTrials = 0;
    private int accTrials = 20;
    private int augSuccessTrials = 0;
    private int mitigateFailTrials = 0;
    private int overrideInputTrials = 0;
    private int PAMtrials = 0;

    [Header("Experiment Setup")]
    public Condition condition = Condition.Control;
    public int trials = 20;
    [Range(0,1)]
    public float positiveRate = 0.5f;
    [Range(0, 1)]
    public float PAMRate = 0.3f;

    
    private int trialsTotal = -1;
    private int currentTrial = -1;
    private TrialType trialResult = TrialType.RejInput;
    private TrialType trialGoal = TrialType.RejInput;

    private Dictionary<string, Mechanism> mechanisms = new Dictionary<string, Mechanism>();

    [Header("InputWindow Settings")]
    [Tooltip("Length of Window and Inter-trial interval.")]
    [SerializeField]
    private float interTrialIntervalSeconds = 4.5f;
    [SerializeField]
    private float inputWindowSeconds = 1f;
    private float inputWindowTimer = 0.0f;
    private float interTrialTimer = 0.0f;
    private InputWindowState inputWindow = InputWindowState.Closed;
    private int inputIndex = 0;

    private GameState gameState = GameState.Stopped;

    [Serializable]
    public class OnGameStateChanged : UnityEvent<GameData> { }
    public OnGameStateChanged onGameStateChanged;
    [Serializable]
    public class GameDecision : UnityEvent<GameDecisionData> { }
    public GameDecision gameDecision;

    [Serializable]
    public class OnInputWindowChanged : UnityEvent<InputWindowState> { }
    public OnInputWindowChanged onInputWindowChanged;

    [Serializable]
    public class OnGameTimeUpdate : UnityEvent<GameTimers> { }
    public OnGameTimeUpdate onGameTimeUpdate;

    private LoggingManager loggingManager;
    private UrnModel urn;

    // Added for PAM
    [HideInInspector]
    public bool augSuccessPossible;
    [HideInInspector]
    public bool gameOver;
    string fish;
    string arrowKey;

    void Start()
    {
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();
        urn = GetComponent<UrnModel>();
        SetupMechanisms();
        SetupUrn();
        LogMeta();
    }

    private void SetupMechanisms()
    {
        accTrials = (int)(trials * positiveRate);
        if (condition == Condition.Control)
        {
            PAMRate = 0;
            PAMtrials = 0;
        }
        else
        {
            PAMtrials = (int)(trials * PAMRate);

            if (condition == Condition.AugmentedSucces)
            {
                //accTrials = accTrials - PAMtrials;
                augSuccessTrials = PAMtrials;
            }
            else if (condition == Condition.MitigatedFailure)
            {
                mitigateFailTrials = PAMtrials;
            }
            else if (condition == Condition.OverrideInput)
            {
                overrideInputTrials = PAMtrials;
            }
        }
        Debug.Log("PAM: " + PAMtrials.ToString());
        Debug.Log("accTrials: " + accTrials.ToString());
        Debug.Log("trials: " + trials.ToString());
        rejTrials = trials - accTrials - PAMtrials;


        if ((condition != Condition.AugmentedSucces && (positiveRate + PAMRate) > 1) ||
            (condition == Condition.AugmentedSucces && positiveRate < PAMRate))
        {
            Debug.LogError("WARNING: Invalid setup. Check the positive/PAM rate.");
        }


        mechanisms["AccInput"] = new Mechanism
        {
            name = "AccInput",
            trialType = TrialType.AccInput,
            rate = 0f,
            trials = accTrials,
            trialsLeft = accTrials,
            behavior = UrnEntryBehavior.Success
        };
        mechanisms["RejInput"] = new Mechanism
        {
            name = "RejInput",
            trialType = TrialType.RejInput,
            rate = 0f,
            trials = rejTrials,
            trialsLeft = rejTrials,
            behavior = UrnEntryBehavior.Override
        };
        mechanisms["AugSuccess"] = new Mechanism
        {
            name = "AugSuccess",
            trialType = TrialType.AugSuccess,
            rate = 0f,
            trials = augSuccessTrials,
            trialsLeft = augSuccessTrials,
            behavior = UrnEntryBehavior.Persist
        };
        mechanisms["MitigateFail"] = new Mechanism
        {
            name = "MitigateFail",
            trialType = TrialType.MitigateFail,
            rate = 0f,
            trials = mitigateFailTrials,
            trialsLeft = mitigateFailTrials,
            behavior = UrnEntryBehavior.PAM
        };
        mechanisms["OverrideInput"] = new Mechanism
        {
            name = "OverrideInput",
            trialType = TrialType.OverrideInput,
            rate = 0f,
            trials = overrideInputTrials,
            trialsLeft = overrideInputTrials,
            behavior = UrnEntryBehavior.PAM
        };
    }

    private void SetupUrn()
    {
        trialsTotal = 0;
        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            urn.AddUrnEntryType(m.name, m.behavior, m.trials);
            trialsTotal += m.trials;
        }

        urn.NewUrn();
        currentTrial = 0;
    }

    private void LogMeta()
    {
        Dictionary<string, object> metaLog = new Dictionary<string, object>() {
            {"Condition", condition},
            {"PosRate", positiveRate},
            {"PAMRate", PAMRate},
            {"AccInputTrials", accTrials},
            {"RejInputTrials", rejTrials},
            {"OverrideInputTrials", overrideInputTrials},
            {"AugSuccessTrials", augSuccessTrials},
            {"MitigateFailTrials", mitigateFailTrials},
            {"Trials", trialsTotal},
            {"InterTrialInterval_sec", interTrialIntervalSeconds},
            {"InputWindow_sec", inputWindowSeconds},
        };
        loggingManager.Log("Meta", metaLog);
    }

    private void LogEvent(string eventLabel)
    {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel},
            {"InputWindow", System.Enum.GetName(typeof(InputWindowState), inputWindow)},
            {"InputWindowOrder", inputIndex},
            {"InterTrialTimer", interTrialTimer},
            {"InputWindowTimer", inputWindowTimer},
            {"GameState", System.Enum.GetName(typeof(GameState), gameState)},
        };

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            gameLog[m.name + "TrialsLeft"] = m.trialsLeft;
            gameLog[m.name + "Rate"] = m.rate;
        }

        if (eventLabel == "GameDecision")
        {
            gameLog["TrialGoal"] = trialGoal;
            gameLog["TrialResult"] = trialResult;
        }
        else
        {
            gameLog["TrialGoal"] = "NA";
            gameLog["TrialResult"] = "NA";
        }

        // Added for PAM
        if (eventLabel == "FishEvent")
        {
            gameLog["FishEvent"] = fish;
        }
        else
        {
            gameLog["FishEvent"] = "NA";
        }

        if(eventLabel == "ArrowKeyInput")
        {
            gameLog["ArrowKeyInput"] = arrowKey;
        }
        else
        {
            gameLog["ArrowKeyInput"] = "NA";
        }


        loggingManager.Log("Game", gameLog);
    }

    public void onFishEvent(string fishEvent)
    {
        fish = fishEvent;
        LogEvent("FishEvent");
    }

    public void onArrowKeyInput(string arrowKeyEvent)
    {
        arrowKey = arrowKeyEvent;
        Debug.Log(arrowKey);
        LogEvent("ArrowKeyInput");
    }
    
    void Update()
    {
        if (gameState == GameState.Running)
        {
            if (inputWindow == InputWindowState.Closed)
            {
                interTrialTimer += Time.deltaTime;
                if (interTrialTimer > interTrialIntervalSeconds && currentTrial < trialsTotal)
                {
                    inputWindow = InputWindowState.Open;
                    onInputWindowChanged.Invoke(inputWindow);
                    LogEvent("InputWindowChange");
                    interTrialTimer = 0f;
                }
            }
            else if (inputWindow == InputWindowState.Open)
            {
                inputWindowTimer += Time.deltaTime;
                if (inputWindowTimer > inputWindowSeconds)
                {
                    //Debug.Log("inputWindow expired.");
                    // The input window expired
                    MakeInputDecision(null, true);
                }
            }
        }

        GameTimers gameTimers = new GameTimers();
        gameTimers.interTrialTimer = interTrialTimer;
        gameTimers.inputWindowTimer = inputWindowTimer;
        onGameTimeUpdate.Invoke(gameTimers);
    }

    public GameData createGameData()
    {
        GameData gameData = new GameData();
        gameData.trials = trialsTotal;
        gameData.interTrialIntervalSeconds = interTrialIntervalSeconds;
        gameData.inputWindowSeconds = inputWindowSeconds;
        gameData.gameState = gameState;
        return gameData;
    }

    public void RunGame()
    {
        CalculateRecogRate();
        gameState = GameState.Running;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
        LogEvent("GameRunning");
    }

    public void EndGame()
    {
        interTrialTimer = 0f;
        if (inputWindow == InputWindowState.Open)
        {
            CloseInputWindow();
        }
        gameState = GameState.Stopped;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
        LogEvent("GameStopped");
        loggingManager.SaveLog("Game");
        loggingManager.SaveLog("Sample");
        loggingManager.SaveLog("Meta");
        loggingManager.ClearAllLogs();
    }

    public void CalculateRecogRate()
    {
        var entriesLeft = urn.GetEntriesLeft();

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            m.trialsLeft = entriesLeft[m.name];
        }

        var entryResults = urn.GetEntryResults();

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            m.rate = (float)entryResults[m.name] / (float)trialsTotal;
        }

        currentTrial = urn.GetIndex();
    }

    public void OnInputReceived(InputData inputData)
    {
        Debug.Log("InputWindowState:" + System.Enum.GetName(typeof(InputWindowState), inputWindow));
        if (inputWindow == InputWindowState.Closed)
        {
            // ignore the input.
            return;
        }
        else
        {
            MakeInputDecision(inputData);
        }
    }

    public void CloseInputWindow()
    {
        // update the window state.
        inputWindow = InputWindowState.Closed;
        //interTrialTimer -= (inputWindowSeconds - inputWindowTimer);
        onInputWindowChanged.Invoke(inputWindow);
        LogEvent("InputWindowChange");
        inputWindowTimer = 0f;

        // store the input decision.
        urn.SetEntryResult(System.Enum.GetName(typeof(TrialType), trialResult));

        Debug.Log(trialResult);

        CalculateRecogRate();
        // Send Decision Data
        GameDecisionData gameDecisionData = new GameDecisionData();
        gameDecisionData.decision = trialResult;
        gameDecision.Invoke(gameDecisionData);
        LogEvent("GameDecision");
        inputIndex++;

        //Added for PAM
        if (currentTrial >= trialsTotal)
        {
            gameOver = true;
        }
    }

    public void MakeInputDecision(InputData inputData = null, bool windowExpired = false)
    {
        string entry = urn.ReadEntry();
        trialGoal = (TrialType)System.Enum.Parse(typeof(TrialType), entry);
        trialResult = TrialType.RejInput;

        if (inputData != null)
        {
            if (trialGoal == TrialType.AccInput)
            {
                if (inputData.validity == InputValidity.Accepted)
                {
                    trialResult = TrialType.AccInput;
                    CloseInputWindow();
                }
                else
                {
                    trialResult = TrialType.RejInput;
                }
            }
            else if (trialGoal == TrialType.RejInput)
            {
                trialResult = TrialType.RejInput;
                // ignore the input.
            }
            else if (trialGoal == TrialType.AugSuccess)
            {
                if (inputData.validity == InputValidity.Accepted)
                {
                    if (augSuccessPossible) {
                        trialResult = TrialType.AugSuccess;
                    } else {
                        // augSuccessPossible is true only if there are more than 2 lanes.
                        // we should default to a normal success and defer the augSuccess
                        // to later.
                        trialResult = TrialType.AccInput;
                    }
                    
                    CloseInputWindow();
                }
                else
                {
                    trialResult = TrialType.RejInput;
                }
            }
        }
        else if (windowExpired)
        {

            if(trialGoal == TrialType.AccInput)
            {
                int rnd = UnityEngine.Random.Range(0, 10);

                Debug.Log("random number: " + rnd);

                if (rnd < 3)
                {
                    if (condition == Condition.OverrideInput && mechanisms["OverrideInput"].trialsLeft != 0)
                    {
                        trialResult = TrialType.OverrideInput;
                    }
                    else if (condition == Condition.MitigatedFailure && mechanisms["MitigateFail"].trialsLeft != 0)
                        trialResult = TrialType.MitigateFail;
                }
            }
            else if (trialGoal == TrialType.MitigateFail)
            {
                trialResult = TrialType.MitigateFail;
            }
            else if (trialGoal == TrialType.OverrideInput)
            {
                trialResult = TrialType.OverrideInput;
            }
            CloseInputWindow();
        }
    }

    public void PauseTrial()
    {
        gameState = GameState.Paused;
    }

    public void ResetTrial()
    {
        inputWindowTimer = 0f;
        interTrialTimer = 0.001f;
        inputWindow = InputWindowState.Closed;
    }

    public void ResumeTrial()
    {
        gameState = GameState.Running;
    }

    // public void SetInputWindowSeconds(float time)
    // {
    //     inputWindowSeconds = time;
    //     GameData gameData = createGameData();
    //     onGameStateChanged.Invoke(gameData);
    // }

    // public void SetInterTrialSeconds(float time)
    // {
    //     interTrialIntervalSeconds = time;
    //     GameData gameData = createGameData();
    //     onGameStateChanged.Invoke(gameData);
    // }

    void OnApplicationQuit()
    {
        if(gameState != GameState.Stopped)
            EndGame();
    }

}
