using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputBlocks : MonoBehaviour
{
    List<TrialType> currentInputBlock;

    public List <TrialType> CreateInputBlock(List <TrialType> designedInputOrder)
    {
        if(designedInputOrder.Count < 6)
        {
            currentInputBlock = designedInputOrder;
        }
        else
        {
            int blockLength = Random.Range(2, 6);

        }

        return currentInputBlock;
    }




    //    int[,] orderOfBlocks = new int[4, 6]{ { 1, 2, 3, 4, 5, 6 },
    //                                          { 5, 4, 3, 2, 6, 1 },
    //                                          { 3, 4, 2, 6, 1, 5 },
    //                                          { 4, 6, 2, 1, 5, 3 } };


    //    // Start is called before the first frame update
    //    void Start()
    //    {

    //    }

    //    // Update is called once per frame
    //    void Update()
    //    {

    //    }

    //    public List<List<InputTypes>> CreateInputBlocks(SetupType setupType, int blockOrder)
    //    {
    //        List<List<InputTypes>> inputBlocks = new List<List<InputTypes>>();
    //        List<List<InputTypes>> temp = new List<List<InputTypes>>();
    //        bool realTest = true;

    //        switch (setupType)
    //        {
    //            case SetupType.Control:
    //                temp = ControlBlocks();
    //                break;


    //            case SetupType.ShamInput:
    //                temp = ShamBlocks();
    //                break;


    //            case SetupType.AssistedSuccess:
    //                temp = AssistedSuccessBlocks();
    //                break;


    //            case SetupType.AssistedFailure:
    //                temp = AssistedFailureBlocks();
    //                break;

    //            case SetupType.Testing:
    //                temp = TestingBlocks();
    //                realTest = false;
    //                break;
    //        }


    //        if (realTest)
    //        {
    //            Debug.Log(blockOrder);
    //            //Add blocks to the list of blocks according to the order
    //            for (int i = 0; i < orderOfBlocks.GetLength(1); i++)
    //            {
    //                inputBlocks.Add(temp[orderOfBlocks[blockOrder - 1, i] - 1]);
    //            }
    //        }
    //        else
    //            inputBlocks = temp;

    //        return inputBlocks;
    //    }

    //    public List<List<InputTypes>> TestingBlocks()
    //    {
    //        List<List<InputTypes>> testingBlocks = new List<List<InputTypes>>
    //        {
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    //InputTypes.AcceptAllInput,
    //                    //InputTypes.RejectAllInput,
    //                    //InputTypes.AcceptAllInput,
    //            },
    //            //new List<InputTypes>() {
    //            //        InputTypes.AcceptAllInput,
    //            //        InputTypes.ShamInput,
    //            //        InputTypes.AcceptAllInput,
    //            //},
    //            //new List<InputTypes>() {
    //            //        InputTypes.RejectAllInput,
    //            //        InputTypes.AssistedSuccess,
    //            //},
    //            //new List<InputTypes>() {
    //            //        InputTypes.RejectAllInput,
    //            //        InputTypes.AssistedFailure,
    //            //        InputTypes.RejectAllInput,
    //            //        InputTypes.AcceptAllInput,
    //            //        InputTypes.RejectAllInput,
    //            //}
    //        };

    //        return testingBlocks;
    //    }

    //    public List<List<InputTypes>> ControlBlocks()
    //    {
    //        List<List<InputTypes>> controlBlocks = new List<List<InputTypes>>
    //        {
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            }
    //            //new List<int>() { 1, 1, 0, 1 },
    //            //new List<int>() { 1, 0, 0, 1, 0 },
    //            //new List<int>() { 0, 0, 1, 1 },
    //            //new List<int>() { 0, 1 },
    //            //new List<int>() { 0, 1, 0, 0 },
    //            //new List<int>() { 1, 0, 1 }
    //        };

    //        return controlBlocks;
    //    }

    //    public List<List<InputTypes>> ShamBlocks()
    //    {
    //        List<List<InputTypes>> shamBlocks = new List<List<InputTypes>>
    //        {
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.ShamInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.ShamInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.ShamInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            }
    //        //new List<int>() { 1, 2, 1 },
    //        //new List<int>() { 2, 0, 1, 0, 0 },
    //        //new List<int>() { 1, 0, 1, 1 },
    //        //new List<int>() { 1, 2 },
    //        //new List<int>() { 0, 1, 1, 0, 0 },
    //        //new List<int>() { 1, 0, 1 }
    //        };

    //        return shamBlocks;
    //    }

    //    public List<List<InputTypes>> AssistedSuccessBlocks()
    //    {
    //        List<List<InputTypes>> assistedSuceessBlocks = new List<List<InputTypes>>
    //        {
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AssistedSuccess,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AssistedSuccess,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AssistedSuccess,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //            }
    //            //new List<int>() { 0, 3 },
    //            //new List<int>() { 1, 0, 0, 1, 0 },
    //            //new List<int>() { 3, 0, 1 },
    //            //new List<int>() { 0, 3, 0, 1 },
    //            //new List<int>() { 1, 0, 1, 0, 0 },
    //            //new List<int>() { 0, 1, 1 }
    //        };

    //        return assistedSuceessBlocks;
    //    }

    //    public List<List<InputTypes>> AssistedFailureBlocks()
    //    {
    //        List<List<InputTypes>> assistedFailureBlocks = new List<List<InputTypes>>
    //        {
    //            new List<InputTypes>() {
    //                    InputTypes.AssistedFailure,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,},
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AssistedFailure,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AssistedFailure,
    //                    InputTypes.AcceptAllInput,
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.RejectAllInput,
    //            },
    //            new List<InputTypes>() {
    //                    InputTypes.RejectAllInput,
    //                    InputTypes.AcceptAllInput,
    //            }
    //            //new List<int>() { 4, 1, 1 },
    //            //new List<int>() { 1, 0, 1, 0, 0 },
    //            //new List<int>() { 1, 4, 1 },
    //            //new List<int>() { 1, 1, 0, 1 },
    //            //new List<int>() { 0, 4, 1, 0, 0 },
    //            //new List<int>() { 0, 1 }
    //        };

    //        return assistedFailureBlocks;
    //    }

    //    //If the last input results in a reel in, it's a success block
    //    public bool SuccessBlock(List<InputTypes> inputBlock)
    //    {
    //        bool successBlock;
    //        InputTypes lastInput = inputBlock[inputBlock.Count - 1];
    //        if (lastInput == InputTypes.AcceptAllInput || lastInput == InputTypes.ShamInput || lastInput == InputTypes.AssistedSuccess)
    //        {
    //            successBlock = true;
    //        }
    //        else
    //        {
    //            successBlock = false;
    //        }
    //        Debug.Log("s: " + successBlock);
    //        return successBlock;
    //    }

    //    //Counting how many successes are in a block, to calculate the lane the fish should spawn in
    //    public int NumberOfSuccesses(List<InputTypes> inputBlock)
    //    {
    //        int successes = 0;

    //        foreach (InputTypes inputType in inputBlock)
    //        {
    //            if (inputType == InputTypes.AcceptAllInput || inputType == InputTypes.ShamInput)
    //            {
    //                successes += 1;
    //            }
    //            else if (inputType == InputTypes.AssistedSuccess)
    //            {
    //                successes += 2;
    //            }
    //        }

    //        return successes;
    //    }
}
