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

    public LineBehaviour line;
    public HookBehaviour hook;
    public PlayerBehaviour player;
    public Basin basin;

    //public InputBlocks inputBlocks;
    //List<int> currentInputBlock;

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
    bool gameEnded;
    bool bciInput;
    bool sham;
    bool won;
    bool lost;
    //bool blockEnded;

    //int numberOfSuccesses = 2;
    //bool successBlock;
    //bool lastBlock;

    public bool moving; //registers whether the feedback is still playing
    
    public GameUI ui;
    //bool lastTrial;
    //int blockCount;
    //int blockIndex;


    [Serializable]
    public class OnFishEvent : UnityEvent<string> { }
    public OnFishEvent onFishEvent;


    // Start is called before the first frame update
    void Start()
    {
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

        ui.ShowProgressBar(false);
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
            case TrialType.ExplicitSham:
                player.Sham(1);
                sham = true;
                
                break;

            case TrialType.AssistSuccess:
                //The assisted success movement feedback doesn't trigger until partway into the animation
                player.AssistedSuccess(1);
                
                break;

            case TrialType.AssistFail:
                AssistedFailure();
                
                break;
        }
        gameManager.PauseTrial();
    }

    //public void onNewBlock(BlockData blockData)
    //{
    //    blockCount = blockData.blockCount;
    //    blockIndex = blockData.blockIndex;
    //    numberOfSuccesses = blockData.numberOfSuccesses;
    //    successBlock = blockData.successBlock;
    //    lastBlock = blockData.lastBlock;
    //}

    //Called when the fish is hooked
    public void BCIInputStart()
    {
        
        bciInput = true;
        
        ui.ShowProgressBar(true);
        ui.BCIInput(true);
        
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
        
    }

    public void FishEscaped()
    {
        onFishEvent.Invoke("FishEscaped");
    }

    void NormalSuccess()
    {
        player.NormalSuccess();
        lane--;

        if (lane == 0)
        {
            column = 3;
            won = true;
            gameManager.canEndGame = true;
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
            gameManager.canEndGame = true;
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
            if (bciInput && !won && !lost)
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
    }

    public void FishCaught()
    {
        winCounter++;
        

        player.Win();

        fishRevealed.SetActive(true);

        currentFish.Goodbye();

        Invoke("Restart", 2);
    }

    void FishLost()
    {

        player.Lose();

        currentFish.Escape();

        lane = 0;
        column = 3;
        hook.Move(new Vector3(columnPos[column], lanePos[lane]), true, 0.8f);
        
        Invoke("Restart", 2);
    }

    void Restart()
    {
        ui.ShowProgressBar(false);
        ui.BCIInput(false);
        //ui.RemoveBait(blockIndex);

        if (won)
        {

            ui.AddFish(fishSprites[sprite], winCounter);

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

        SpawnFish();
    }

    public void onGameStateChanged(GameData gameData)
    {
        if(gameData.gameState == GameState.Stopped)
        {
            gameEnded = true;
            Invoke("EndGame", 1);
        }
    }

    public void SpawnFish()
    {
        if (!gameEnded)
        {
            gameManager.canEndGame = false;
            int size = UnityEngine.Random.Range(0, 3);

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
        else
            Debug.Log("No more trials, ending game.");
    }

    void StartGame()
    {
        gameManager.RunGame();
        gameManager.PauseTrial();
        //ui.CreateBaitCounter(blockCount);

        SpawnFish();
        gameStarted = true;
    }

    void EndGame()
    {
        ui.DisableUI();
        //gameManager.EndGame();
    }
}