using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameController2048 : MonoBehaviour
{
    [SerializeField] GameObject fillPrefab;
    [SerializeField] Transform[] allCells;

    public static Action<string> slide;
    // Start is called before the first frame update
    void Start()
    {
        
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
            slide("w");
        }

        if(Input.GetKeyDown(KeyCode.D))
        {
            slide("d");
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            slide("s");
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            slide("a");
        }
    }

    public void SpawnFill()
    {
        int whichSpawn = UnityEngine.Random.Range(0, allCells.Length);

        if(allCells[whichSpawn].childCount != 0)                            //checks if that cell is already taken
        {
            Debug.Log(allCells[whichSpawn].name + " is already filled");
            SpawnFill();
            return;
        }

        float chance = UnityEngine.Random.Range(0f, 1f);
        Debug.Log(chance);
        if(chance < .2f)                                                    // doesn't spawn anything 
        {
            return;
        }
        else if(chance < .8f)                                               // spawns a number 2 
        {
            
            GameObject tempFill = Instantiate(fillPrefab, allCells[whichSpawn]);
            Debug.Log(2);
            Fill2048 tempFillComp = tempFill.GetComponent<Fill2048>();          // pulls number from Fill2048 Script
            allCells[whichSpawn].GetComponent<Cell2048>().fill = tempFillComp;
            tempFillComp.FillValueUpdate(2);

        }
        else                                                                // spawns a number 4
        {
            
            GameObject tempFill = Instantiate(fillPrefab, allCells[whichSpawn]);
            Debug.Log(4);
            Fill2048 tempFillComp = tempFill.GetComponent<Fill2048>();          // pulls number from Fill2048 Script
            allCells[whichSpawn].GetComponent<Cell2048>().fill = tempFillComp;
            tempFillComp.FillValueUpdate(4);
        }
    }
}
