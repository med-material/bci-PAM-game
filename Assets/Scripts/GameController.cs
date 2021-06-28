using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public int winCounter = 0;

    public HookBehaviour hook;
    public PlayerBehaviour player;
    public Basin basin;
    public GameUI ui;

    public GameManager gameManager;

    public FishBehaviour currentFish;
    public GameObject fishRevealed;
    public Sprite[] fishSprites;

    public GameObject[] fish;
    int sprite;
    Vector3 spawnPoint;

    public Transform lake;
    float[] lanePos = new float[4];
    float[] columnPos = new float[7];
    int lane = 0;
    int column = 3;

    bool gameStarted;
    bool bciInput;
    bool sham;
    bool won;
    bool lost;

    public bool moving; //registers whether the feedback is still playing

    [Serializable]
    public class OnFishEvent : UnityEvent<string> { }
    public OnFishEvent onFishEvent;

    [Serializable]
    public class OnArrowKeyInput : UnityEvent<string> { }
    public OnArrowKeyInput onArrowKeyInput;

    // Start is called before the first frame update
    void Start()
    {
        fishRevealed.SetActive(false);
        SetupLake();
        ui.ShowProgressBar(false);
    }

    void Update()
    {
        //Arrow-key movement, only when not in BCI-mode, there isn't feedback playing, and we haven't reached the end of the game
        if (!bciInput && !moving && !gameManager.gameOver)
        {
            if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && lane < 3)
            {
                HookDown();
                onArrowKeyInput.Invoke("ArrowKeyDown");
            }
            if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && lane > 0)
            {
                HookUp();
                onArrowKeyInput.Invoke("ArrowKeyUp");
            }
        }
    }

    void SetupLake()
    {
        //Calculating lane- and column positions based on (invisible) GameObject that is the height/width of the lake
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

    public void onGameDecision(GameDecisionData gameDecision)
    {
        switch (gameDecision.decision)
        {
            case TrialType.RejInput:
                NormalFailure();

                break;

            case TrialType.AccInput:
                NormalSuccess();

                break;

            //Sham is ... complicated
            case TrialType.OverrideInput:
                player.Sham(1);
                sham = true;

                break;

            case TrialType.AugSuccess:
                //The assisted success movement feedback doesn't trigger until partway into the animation
                player.AssistedSuccess(1);

                break;

            case TrialType.MitigateFail:
                AssistedFailure();

                break;
        }
        gameManager.PauseTrial();
    }

    //Called when the fish is hooked
    public void BCIInputStart()
    {

        bciInput = true;

        ui.ShowProgressBar(true);
        ui.BCIInput(true);

        gameManager.ResumeTrial();
    }

    void HookUp()
    {
        lane--;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 1);
        player.ReelIn();

    }

    void HookDown()
    {
        //First down key starts the game
        if (!gameStarted)
        {
            StartGame();
        }

        lane++;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), false, 1);
        player.ReelOut();

    }

    public void FishEscaped()
    {
        onFishEvent.Invoke("FishNotHooked");
    }

    void NormalSuccess()
    {
        player.NormalSuccess();
        lane--;

        if (lane == 0)
        {
            column = 3;
            won = true;
            //gameManager.canEndGame = true;
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

            if (lane < 2)
            {
                gameManager.assistSuccessPossible = false;
            }
            else
            {
                gameManager.assistSuccessPossible = true;
            }


            //If we're still in the middle of a block, start next trial
            if (bciInput && !won && !lost && !gameManager.gameOver)
            {
                gameManager.ResumeTrial();
                currentFish.StopStruggle();
            }
            //If we're at the end of the block and the user has won
            else if (won)
            {
                onFishEvent.Invoke("FishCaught");
                Invoke("FishCaught", 0.2f); //Wait just a lil bit to make it more natural
            }
            //If we're at the end of the block and the user has lost
            else if (lost)
            {
                onFishEvent.Invoke("FishLost");
                FishLost();
            }
        }

        if (gameManager.gameOver)
        {
            if (lane != 0)
            {
                FishLost();
                onFishEvent.Invoke("GameOver");
            }
        }
    }

    public void FishCaught()
    {
        winCounter++;

        ui.ShowProgressBar(false);
        player.Win();

        fishRevealed.SetActive(true);
        currentFish.Goodbye();

        Invoke("Restart", 2);
    }

    void FishLost()
    {
        ui.ShowProgressBar(false);
        player.Lose();

        currentFish.Escape();

        lane = 0;
        column = 3;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), true, 1.2f);

        if(!gameManager.gameOver)
            Invoke("Restart", 2);  
    }

    void Restart()
    {
            
        ui.BCIInput(false);

        if (won)
        {
            //ui.AddFish(fishSprites[sprite], winCounter);

            basin.Splash();
            won = false;
            Invoke("SpawnFish", 1);
        }

        if (lost)
        {
            currentFish.Goodbye();
            lost = false;
            Invoke("SpawnFish", 1.5f);
        }

        fishRevealed.SetActive(false);

        gameManager.ResetTrial();

        bciInput = false;

        player.Idle();

        if (gameManager.gameOver)
        {
            Invoke("EndGame", 1);
            Debug.Log("No more trials, ending game.");
        }

    }

    public void SpawnFish()
    {
        if (!gameManager.gameOver)
        {

            int size = UnityEngine.Random.Range(1, 3);

            int leftToRight = UnityEngine.Random.Range(0, 2);


            if (leftToRight == 0)
            {
                spawnPoint.x = -lake.transform.localScale.x + 3;
            }
            else
            {
                spawnPoint.x = lake.transform.localScale.x - 3;
            }

            spawnPoint.y = lanePos[size + 1] - fish[size].transform.localScale.y / 3;
            spawnPoint.z = hook.transform.position.z - 0.1f;

            currentFish = Instantiate(fish[size], spawnPoint, Quaternion.identity).GetComponent<FishBehaviour>();

            sprite = UnityEngine.Random.Range(size * 3, size * 3 + 3);

            fishRevealed.GetComponent<SpriteRenderer>().sprite = fishSprites[sprite];
        }
    }

    void StartGame()
    {
        gameManager.RunGame();
        gameManager.PauseTrial();

        SpawnFish();
        gameStarted = true;
    }

    void EndGame()
    {
        gameManager.EndGame();
        ui.DisableUI();
    }
}