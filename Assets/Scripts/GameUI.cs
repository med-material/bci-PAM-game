using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameObject baitCounter;
    public GameObject bait;
    List<GameObject> baitsLeft = new List<GameObject>();
    
    public GameObject allCaughtUIBar;
    public GameObject allCaughtUIBar2;
    public GameObject fishCaughtUI;
    public GameObject fishCaughtUIRef;

    public GameObject arrowKeys;
    public GameObject bciInput;
    public GameObject progressBar;
    public GameObject endScreen;
    public GameObject UIElements;

    // Start is called before the first frame update
    void Start()
    {
        endScreen.GetComponent<Animator>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateBaitCounter(int inputBlocks)
    {
        for (int i = 0; i < inputBlocks; i++)
        {
            baitsLeft.Add(Instantiate(bait, new Vector3(baitCounter.transform.position.x + Screen.width / 30 * i,
                baitCounter.transform.position.y), Quaternion.identity, baitCounter.transform));
        }
    }

    public void RemoveBait(int i)
    {
        Destroy(baitsLeft[baitsLeft.Count - i]);
    }

    public void ShowProgressBar(bool show)
    {
        if (show)
            progressBar.SetActive(true);
        else
            progressBar.SetActive(false);
    }

    public void BCIInput(bool bciMode)
    {
        if (!bciMode)
        {
            arrowKeys.SetActive(true);
            bciInput.SetActive(false);
        }
        else
        {
            arrowKeys.SetActive(false);
            bciInput.SetActive(true);
        }
    }

    public void AddFish(Sprite fishSprite, int winCounter)
    {
        GameObject caughtFish;
        if (winCounter < 6)
        {
            caughtFish = Instantiate(fishCaughtUI,
                new Vector3(fishCaughtUIRef.transform.position.x - Screen.width / 13 * (winCounter - 1), allCaughtUIBar.transform.position.y),
                Quaternion.identity, allCaughtUIBar.transform);
        }
        else
        {
            if(winCounter == 6)
            {
                allCaughtUIBar2.SetActive(true);
            }
            caughtFish = Instantiate(fishCaughtUI,
                new Vector3(fishCaughtUIRef.transform.position.x - Screen.width / 13 * (winCounter - 6), allCaughtUIBar2.transform.position.y),
                Quaternion.identity, allCaughtUIBar2.transform);
        }

        caughtFish.GetComponent<Image>().sprite = fishSprite;
    }

    public void DisableUI()
    {
        UIElements.SetActive(false);
        ShowProgressBar(false);
        endScreen.GetComponent<Animator>().enabled = true;
    }
}
