using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputBlocks : MonoBehaviour
{
    public GameObject baitCounter;
    public GameObject bait;
    List<GameObject> baitsLeft = new List<GameObject>();
    List<List<int>> inputBlocks = new List<List<int>>();
    int[,] orderOfBlocks = new int[4, 6]{ { 1, 2, 3, 4, 5, 6 },
                                          { 5, 4, 3, 2, 6, 1 },
                                          { 3, 4, 2, 6, 1, 5 },
                                          { 4, 6, 2, 1, 5, 3 } };

    [Range(0, 4)]
    public int condition;
    [Range(1, 4)]
    public int order;

    // Start is called before the first frame update
    void Start()
    {
        //CreateInputBlocks(condition, order);
        CreateTestingBlocks();
        CreateBaitCounter();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateInputBlocks(int condition, int order)
    {
        List<List<int>> temporary = new List<List<int>>();
        switch (condition)
        {
            //Practice mode
            case 0:
                inputBlocks.Add(new List<int>() { 1, 0, 1 });
                break;

            case 1:
                temporary = ControlBlocks();
                break;

            case 2:
                temporary = ShamBlocks();
                break;

            case 3:
                temporary = AssistedSuccessBlocks();
                break;

            case 4:
                temporary = AssistedFailureBlocks();
                break;
        }


        if (condition != 0)
        {
            //Add blocks to the list of blocks according to the order
            for (int i = 0; i < orderOfBlocks.GetLength(1); i++)
            {
                inputBlocks.Add(temporary[orderOfBlocks[order - 1, i] - 1]);
                PrintList(temporary[orderOfBlocks[order - 1, i] - 1]);
            }
        }
    }

    void CreateTestingBlocks()
    {
        //For testing during implementation
        inputBlocks.Add(new List<int>() { 1, 1 });
        inputBlocks.Add(new List<int>() { 0, 0, 0 });
        //inputBlocks.Add(new List<int>() { 1, 0, 1 });
        //inputBlocks.Add(new List<int>() { 0, 1, 3 });
        //inputBlocks.Add(new List<int>() { 0, 0, 1, 0 });
        //inputBlocks.Add(new List<int>() { 0, 2, 0, 1, 0 });
        inputBlocks.Add(new List<int>() {  0, 4, 1 });
        inputBlocks.Add(new List<int>() { 2, 2 });
        //inputBlocks.Add(new List<int>() { 1, 0, 2 });
        inputBlocks.Add(new List<int>() { 3 });
        //inputBlocks.Add(new List<int>() { 0, 2, 4, 1 });
        //inputBlocks.Add(new List<int>() { 3, 4, 2 });
    }

    List<List<int>> ControlBlocks()
    {
        List<List<int>> controlBlocks = new List<List<int>>
        {
            new List<int>() { 1, 1, 0, 1 },
            new List<int>() { 1, 0, 0, 1, 0 },
            new List<int>() { 0, 0, 1, 1 },
            new List<int>() { 0, 1 },
            new List<int>() { 0, 1, 0, 0 },
            new List<int>() { 1, 0, 1 }
        };

        return controlBlocks;
    }

    List<List<int>> ShamBlocks()
    {
        List<List<int>> shamBlocks = new List<List<int>>
        {
            new List<int>() { 1, 2, 1 },
            new List<int>() { 2, 0, 1, 0, 0 },
            new List<int>() { 1, 0, 1, 1 },
            new List<int>() { 1, 2 },
            new List<int>() { 0, 1, 1, 0, 0 },
            new List<int>() { 1, 0, 1 }
        };

        return shamBlocks;
    }

    List<List<int>> AssistedSuccessBlocks()
    {
        List<List<int>> assistedSuceessBlocks = new List<List<int>>
        {
            new List<int>() { 0, 3 },
            new List<int>() { 1, 0, 0, 1, 0 },
            new List<int>() { 3, 0, 1 },
            new List<int>() { 0, 3, 0, 1 },
            new List<int>() { 1, 0, 1, 0, 0 },
            new List<int>() { 0, 1, 1 }
        };

        return assistedSuceessBlocks;
    }

    List<List<int>> AssistedFailureBlocks()
    {
        List<List<int>> assistedFailureBlocks = new List<List<int>>
        {
            new List<int>() { 4, 1, 1 },
            new List<int>() { 1, 0, 1, 0, 0 },
            new List<int>() { 1, 4, 1 },
            new List<int>() { 1, 1, 0, 1 },
            new List<int>() { 0, 4, 1, 0, 0 },
            new List<int>() { 0, 1 }
        };

        return assistedFailureBlocks;
    }

    //Creating the bait counter according to the number of inputblocks
    void CreateBaitCounter()
    {
        for (int i = 0; i < inputBlocks.Count; i++)
        {
            baitsLeft.Add(Instantiate(bait, new Vector3(baitCounter.transform.position.x + Screen.width / 30 * i,
                baitCounter.transform.position.y), Quaternion.identity, baitCounter.transform));
        }
    }
    
    int i = 1;

    //Removing a the bait at the end of the list when a block is done
    public void RemoveBait()
    {
        Destroy(baitsLeft[baitsLeft.Count() - i]);
        i++;
    }

    //Also used for testing
    public void PrintList(List<int> toPrint)
    {
        string block = "";
        if (SuccessBlock(toPrint))
        {
            block += "Success block: ";
        }
        else
        {
            block += "Failure block: ";
        }

        foreach (int i in toPrint)
        {
            block += " " + i;
        }

        Debug.Log(block);
    }

    int blockCount = 0;

    public List<int> NextBlock()
    {
        List<int> currentBlock;
        //if (!first)
        //{
        //    inputBlocks.RemoveAt(0);
        //}

        //if (inputBlocks.Count == 0)
        //{
        //    return null;
        //}
        //else
        //{
        //    currentBlock = inputBlocks.First();
        //    PrintList(currentBlock);
        //    return currentBlock;
        //}

        if(blockCount == inputBlocks.Count())
        {
            return null;
        }
        else
        {
            currentBlock = inputBlocks[blockCount];
            PrintList(currentBlock);
            blockCount++;
            return currentBlock;
        }
    }

    //If the last input results in a reel in, it's a success block
    bool SuccessBlock(List<int> inputBlock)
    {
        bool successBlock;
        int lastInput = inputBlock[inputBlock.Count - 1];
        if (lastInput == 1 || lastInput == 2 || lastInput == 3)
        {
            successBlock = true;
        }
        else
        {
            successBlock = false;
        }
        return successBlock;
    }

    //Counting how many successes are in a block, to calculate the lane the fish should spawn in
    public int NumberOfSuccesses(List<int> inputBlock)
    {
        int laneNumber = 0;

        foreach (int i in inputBlock)
        {
            if (i == 1 || i == 2)
            {
                laneNumber += 1;
            }
            else if (i == 3)
            {
                laneNumber += 2;
            }
        }

        //If it's a failure block, it needs to spawn one lane further down
        if (SuccessBlock(inputBlock) == false)
        {
            laneNumber += 1;
        }

        return laneNumber;
    }

    //Code for shuffling (removed after we designed all the input blocks)
    //List<int> ShuffleInputs(List<int> inputBlock)
    //{
    //    int lastInput = inputBlock[inputBlock.Count - 1];
    //    inputBlock.RemoveAt(inputBlock.Count - 1);
    //    Utils.Shuffle(inputBlock);
    //    inputBlock.Add(lastInput);
    //    return inputBlock;
    //}
}
