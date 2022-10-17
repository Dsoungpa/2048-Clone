using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController2048 : MonoBehaviour
{
    public static GameController2048 instance;
    public static int ticker;

    [SerializeField] GameObject fillPrefab;
    [SerializeField] Cell2048[] allCells;

    public static Action<string> slide;
    public int myScore;
    [SerializeField] TextMeshProUGUI scoreDisplay;

    int isGameOver;
    [SerializeField] GameObject gameOverPanel;

    public Color[] fillColors;

    [SerializeField] int winningScore;
    [SerializeField] GameObject WinningPanel;
    bool hasWon;

    private void OnEnable()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        StartSpawnFill();
        StartSpawnFill();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {

            SpawnFill();
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            ticker = 0;
            isGameOver = 0;
            slide("w");
            if(ticker != 0)
            {
                SpawnFill();
            }
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            ticker = 0;
            isGameOver = 0;
            slide("d");
            if(ticker != 0)
            {
                SpawnFill();
            }
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            ticker = 0;
            isGameOver = 0;
            slide("s");
            if(ticker != 0)
            {
                SpawnFill();
            }
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            ticker = 0;
            isGameOver = 0;
            slide("a");
            if(ticker != 0)
            {
                SpawnFill();
            }
        }
    }

    public void SpawnFill()
    {
        bool isFull = true;
        for(int i = 0; i < allCells.Length; i++)
        {
            if(allCells[i].fill == null)
            {
                isFull = false;
            }
        }

        if(isFull == true)
        {
            return;
        }
        int whichSpawn = UnityEngine.Random.Range(0, allCells.Length);

        if(allCells[whichSpawn].transform.childCount != 0)                            //checks if that cell is already taken
        {
            //Debug.Log(allCells[whichSpawn].name + " is already filled");
            SpawnFill();
            return;
        }

        float chance = UnityEngine.Random.Range(0f, 1f);
        //Debug.Log(chance);
        if(chance < .8f)                                               // spawns a number 2 
        {
            
            GameObject tempFill = Instantiate(fillPrefab, allCells[whichSpawn].transform);
            //Debug.Log(2);
            Fill2048 tempFillComp = tempFill.GetComponent<Fill2048>();          // pulls number from Fill2048 Script
            allCells[whichSpawn].GetComponent<Cell2048>().fill = tempFillComp;
            tempFillComp.FillValueUpdate(2);

        }
        else                                                                // spawns a number 4
        {
            
            GameObject tempFill = Instantiate(fillPrefab, allCells[whichSpawn].transform);
            //Debug.Log(4);
            Fill2048 tempFillComp = tempFill.GetComponent<Fill2048>();          // pulls number from Fill2048 Script
            allCells[whichSpawn].GetComponent<Cell2048>().fill = tempFillComp;
            tempFillComp.FillValueUpdate(4);
        }
    }

    public void StartSpawnFill()
    {
        int whichSpawn = UnityEngine.Random.Range(0, allCells.Length);

        if(allCells[whichSpawn].transform.childCount != 0)                            //checks if that cell is already taken
        {
            //Debug.Log(allCells[whichSpawn].name + " is already filled");
            SpawnFill();
            return;
        }

        GameObject tempFill = Instantiate(fillPrefab, allCells[whichSpawn].transform);
        //Debug.Log(2);
        Fill2048 tempFillComp = tempFill.GetComponent<Fill2048>();          // pulls number from Fill2048 Script
        allCells[whichSpawn].GetComponent<Cell2048>().fill = tempFillComp;
        tempFillComp.FillValueUpdate(2);
    }

    public void ScoreUpdate(int scoreIn)
    {
        myScore += scoreIn;
        scoreDisplay.text = myScore.ToString();
    }


    public void GameOverCheck()
    {
        isGameOver++;
        if(isGameOver >= 16)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void WinningCheck(int highestFill)
    {
        if(hasWon)
        {
            return;
        }
        if(highestFill == winningScore)
        {
            WinningPanel.SetActive(true);
            hasWon = true;
        }
    }

    public void KeepPlaying()
    {
        WinningPanel.SetActive(false);
    }
}
