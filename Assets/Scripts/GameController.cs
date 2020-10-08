using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


//For logging game events
public struct GameEventData
{
    public NarrativeEvent narrativeEvent;
    public FeedbackType feedbackType;
    public BlockResult blockResult;
    public int column;
    public int lane;
    public int winCounter;
    public int lossCounter;
    public int inputCounter;
    public int blockCounter;
    public int trialCounter;
    public string fishType;
    public bool BCIMode;
}


public enum FeedbackType
{
    Failure,
    Success,
    Sham,
    AssistedSuccess,
    AssistedFailure,
    Rejected
}

public enum NarrativeEvent
{
    GameStart,
    HookMoved,
    FishHooked,
    FishMissed,
    InputAccepted,
    Feedback,
    NoInputAccepted,
    NextTrialStarted,
    BlockEnded,
    GameEnd
}

public enum BlockResult
{
    Win,
    Loss
}


public class GameController : MonoBehaviour
{
    //For logging game events
    [Serializable]
    public class GameEventChanged : UnityEvent<GameEventData> { }
    public GameEventChanged onGameEventChanged;

    [Serializable]
    public class Feedback: UnityEvent<GameEventData> { }
    public Feedback onFeedBack;

    [Serializable]
    public class NextTrialStarted : UnityEvent<GameEventData> { }
    public NextTrialStarted onNextTrialStarted;

    [Serializable]
    public class EndOfBlock : UnityEvent<GameEventData> { }
    public EndOfBlock onEndOfBlock;

    public NarrativeEvent narrativeEvent;
    public FeedbackType feedbackType;
    public BlockResult blockResult;

    public int inputCounter;
    public int inputBlockCounter = 1;
    public int winCounter = 0;
    public int lossCounter = 0;
    public int trialCounter;
    public string fishType;

    public LineBehaviour line;
    public HookBehaviour hook;
    public PlayerBehaviour player;
    public Basin basin;
    public GameObject endscreen;

    public InputBlocks inputBlocks;
    List<int> currentInputBlock;

    public GameManager gameManager;
    public GameObject progressBar;

    public FishBehaviour currentFish;
    public GameObject fishRevealed;
    public Sprite[] fishSprites;

    public GameObject[] fish;
    int sprite;
    Vector3 spawnPoint;

    public GameObject allCaughtUIBar;
    public GameObject fishCaughtUI;
    public GameObject fishCaughtUIRef;

    public GameObject arrowKeys;
    public GameObject keySequence;

    public Transform lake;
    float[] lanePos = new float[4];
    float[] columnPos = new float[7];
    int lane = 0;
    int column = 3;
    
    bool gameStarted;
    bool gameEnded;
    bool bciInput;
    bool sham;
    bool won;
    bool lost;

    public bool moving; //registers whether the feedback is still playing


    // Start is called before the first frame update
    void Start()
    {
        progressBar.SetActive(false);
        fishRevealed.SetActive(false);

        //Calculating lane- and column positions based on GameObject that is the height/width of the lake
        float lakeTop;
        float lakeLeft;
        float laneHeight;
        float columnWidth;
        
        //3 lanes and 7 columns
        laneHeight = lake.transform.localScale.y / 3;
        columnWidth = lake.transform.localScale.x / 7;

        //Finding the top lane + leftmost column positions to use as reference
        lakeTop = lake.transform.localPosition.y + laneHeight;
        lakeLeft = lake.transform.localPosition.x - 3 * columnWidth;

        //Lane 0 is the starting position (when the hook is reeled all the way in)
        lanePos[0] = hook.transform.localPosition.y;

        for (int i = 1; i < lanePos.Length; i++)
        {
            lanePos[i] = lakeTop - (i - 1) * laneHeight;
        }

        for (int i = 0; i < columnPos.Length; i++)
        {
            columnPos[i] = lakeLeft + i * columnWidth;
        }
    }

    void Update()
    {
        //Arrow-key movement, only when not in BCI-mode, there isn't feedback playing, and we haven't reached the end of the game
        if (!bciInput && !moving && !gameEnded)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) && lane < 3)
            {
                ArrowKeyDown();
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && lane > 0)
            {
                ArrowKeyUp();
            }
        }
    }

    //For logging
    public GameEventData CreateGameEventData()
    {
        GameEventData gameEvent = new GameEventData();
        gameEvent.narrativeEvent = narrativeEvent;
        gameEvent.feedbackType = feedbackType;
        gameEvent.blockResult = blockResult;
        gameEvent.winCounter = winCounter;
        gameEvent.lossCounter = lossCounter;
        gameEvent.inputCounter = inputCounter;
        gameEvent.blockCounter = inputBlockCounter;
        gameEvent.column = column;
        gameEvent.lane = lane;
        gameEvent.trialCounter = trialCounter;
        gameEvent.fishType = fishType;
        gameEvent.BCIMode = bciInput;
        return gameEvent;
    }

    public void onInputWindowChanged(InputWindowState windowState)
    {
        //Triggering (hook/fish/line movement) feedback
        if (windowState == InputWindowState.Closed && gameManager.inputAccepted)
        {
            narrativeEvent = NarrativeEvent.Feedback;
            switch (currentInputBlock.First())
            {
                case 0:
                    NormalFailure();

                    feedbackType = FeedbackType.Failure;
                    break;

                case 1:
                    NormalSuccess();

                    feedbackType = FeedbackType.Success;
                    break;

                //Sham is ... complicated
                case 2:
                    player.Sham(1);
                    sham = true;

                    feedbackType = FeedbackType.Sham;
                    break;

                case 3:
                    //The assisted success movement feedback doesn't trigger until partway into the animation
                    player.AssistedSuccess(1);

                    feedbackType = FeedbackType.AssistedSuccess;
                    break;

                case 4:
                    AssistedFailure();

                    feedbackType = FeedbackType.AssistedFailure;
                    break;
            }
            GameEventData gameEvent = CreateGameEventData();
            onFeedBack.Invoke(gameEvent);

            gameManager.PauseTrial();

            inputCounter++;
            trialCounter++;

            //If there are more inputs, remove the first(current)
            if (currentInputBlock.Count > 0)
            {
                currentInputBlock.Remove(currentInputBlock.First());
            }
        }

        //If the window closes and no input has been accepted, log it but otherwise do nothing
        else if (windowState == InputWindowState.Closed && !gameManager.inputAccepted)
        {
            narrativeEvent = NarrativeEvent.NoInputAccepted;
            feedbackType = FeedbackType.Rejected;
            GameEventData gameEvent = CreateGameEventData();
            onFeedBack.Invoke(gameEvent);

            trialCounter++;
        }
        //If the window starts again, the next trial has started
        //(2 seconds earlier)
        //(this was mostly just to make the logging more coherent)
        else if(windowState == InputWindowState.Open)
        {
            narrativeEvent = NarrativeEvent.NextTrialStarted;
            GameEventData gameEvent = CreateGameEventData();
            onNextTrialStarted.Invoke(gameEvent);
        }
    }

    //Called when the fish is hooked
    public void BCIInputStart()
    {
        narrativeEvent = NarrativeEvent.FishHooked;
        GameEventData gameEvent = CreateGameEventData();
        onGameEventChanged.Invoke(gameEvent);
        
        bciInput = true;

        arrowKeys.SetActive(false);
        keySequence.SetActive(true);
        progressBar.SetActive(true);

        gameManager.failure = FailureInput(currentInputBlock.First()); //Register whether first input is a success

        gameManager.ResetTrial();
        gameManager.ResumeTrial();
    }

    private bool FailureInput(int input)
    {
        bool failure;

        if(input == 1 || input == 3)
        {
            failure = false;
        }
        else
        {
            failure = true;
        }

        return failure;
    }

    void ArrowKeyUp()
    {
        lane--;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 1);
        player.ReelIn();

        narrativeEvent = NarrativeEvent.HookMoved;
        GameEventData gameEvent = CreateGameEventData();
        onGameEventChanged.Invoke(gameEvent);
    }

    void ArrowKeyDown()
    {
        //First arrowkey down starts the game
        if (!gameStarted)
        {
            StartGame();
        }

        lane++;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 1);
        player.ReelOut();

        narrativeEvent = NarrativeEvent.HookMoved;
        GameEventData gameEvent = CreateGameEventData();
        onGameEventChanged.Invoke(gameEvent);
    }

    void NormalSuccess()
    {
        player.NormalSuccess();
        lane--;

        if (lane == 0)
        {
            column = 3;
            won = true;
        }
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 1);
    }

    void NormalFailure()
    {
        player.NormalFailure();
        currentFish.Struggle();

        //Fish moves left or right, depending on which side it spawned from
        if (spawnPoint.x < 0)
        {
            column--;
        }
        else
        {
            column++;
        }
        
        if (column == 0 || column == 6)
        {
            lost = true;
        }

        hook.Move(new Vector3(columnPos[column], lanePos[lane]), true, 0.8f);
    }

    public void Sham()
    {
        lane--;

        if (lane == 0)
        {
            column = 3;
            won = true;
        }

        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 1);
    }

    public void AssistedSuccess()
    {
        lane -= 2;

        if (lane == 0)
        {
            column = 3;
            won = true;
        }

        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 0.5f);
    }

    void AssistedFailure()
    {
        currentFish.Struggle();
        player.AssistedFailure();
    }

    public void FeedbackFinished()
    {
        //Sham has extra feedback that plays after the movement feedback finishes
        if (sham)
        {
            player.Sham(3);
            sham = false;
        }
        else if (!sham)
        {
            moving = false;

            player.Idle();

            //If we're still in the middle of a block, start next trial
            if (bciInput && !won && !lost)
            {
                gameManager.ResumeTrial();
                currentFish.StopStruggle();

                gameManager.failure = FailureInput(currentInputBlock.First());
            }
            //If we're at the end of the block and the user has won
            else if (won)
            {
                Invoke("FishCaught", 0.2f); //Wait just a lil bit to make it more natural
            }
            //If we're at the end of the block and the user has lost
            else if (lost)
            {
                FishLost();
            }
        }
    }

    public void FishCaught()
    {
        narrativeEvent = NarrativeEvent.BlockEnded;
        blockResult = BlockResult.Win;
        winCounter++;
        fishType = fishSprites[sprite].ToString().Replace(" (UnityEngine.Sprite)",string.Empty);
        GameEventData gameEvent = CreateGameEventData();
        onEndOfBlock.Invoke(gameEvent);

        progressBar.SetActive(false);
        keySequence.SetActive(false);

        player.Win();

        fishRevealed.SetActive(true);

        currentFish.Goodbye();

        Invoke("Restart", 2);
    }

    void FishLost()
    {
        narrativeEvent = NarrativeEvent.BlockEnded;
        blockResult = BlockResult.Loss;
        lossCounter++;
        GameEventData gameEvent = CreateGameEventData();
        onEndOfBlock.Invoke(gameEvent);

        progressBar.SetActive(false);
        keySequence.SetActive(false);

        player.Lose();

        currentFish.Escape();

        lane = 0;
        column = 3;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), true, 0.8f);
        
        Invoke("Restart", 2);
    }

    void Restart()
    {
        inputBlocks.RemoveBait();

        if (won)
        {
            GameObject caughtFish = Instantiate(fishCaughtUI,
                new Vector3(fishCaughtUIRef.transform.position.x - Screen.width / 12 * (winCounter - 1), allCaughtUIBar.transform.position.y),
                Quaternion.identity, allCaughtUIBar.transform);
            caughtFish.GetComponent<Image>().sprite = fishSprites[sprite];

            basin.Splash();
            won = false;
        }

        if (lost)
        {
            currentFish.Goodbye();
            lost = false;
        }

        fishRevealed.SetActive(false);
        gameManager.ResetTrial();

        bciInput = false;

        player.Idle();

        currentInputBlock = inputBlocks.NextBlock();
        inputBlockCounter++;

        if (currentInputBlock == null)
        {
            gameEnded = true;
            Invoke("EndGame", 1);
        }
        else
        {
            Invoke("SpawnFish", 0.1f);
            arrowKeys.SetActive(true);
        }
    }

    public void SpawnFish()
    {
        int size = inputBlocks.NumberOfSuccesses(currentInputBlock) - 1;
        int leftToRight = UnityEngine.Random.Range(0, 2);

        if (leftToRight == 0)
        {
            spawnPoint.x = -lake.transform.localScale.x;
        }
        else
            spawnPoint.x = lake.transform.localScale.x;

        spawnPoint.y = lanePos[size + 1] - fish[size].transform.localScale.y / 3;
        spawnPoint.z = hook.transform.position.z - 0.1f;

        currentFish = Instantiate(fish[size], spawnPoint, Quaternion.identity).GetComponent<FishBehaviour>();

        sprite = UnityEngine.Random.Range(size * 3, size * 3 + 3);
        fishRevealed.GetComponent<SpriteRenderer>().sprite = fishSprites[sprite];
    }

    void StartGame()
    {
        gameManager.RunGame();
        gameManager.PauseTrial();

        currentInputBlock = inputBlocks.NextBlock();

        SpawnFish();
        gameStarted = true;

        inputBlockCounter = 1;
        inputCounter = 1;
        trialCounter = 1;
    }

    void EndGame()
    {
        allCaughtUIBar.SetActive(false);
        gameManager.EndGame();
        endscreen.GetComponent<Animator>().enabled = true;
    }
}