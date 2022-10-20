using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;



public class IncrementUI : MonoBehaviour
{

    public GameObject UI;

    [SerializeField] TextMeshProUGUI valueDisplay;

    public GameObject BuyMore;

    bool toggle = false;

    public static int current = 5;

    bool currentiszero = false;

    public void Update(){
        valueDisplay.text = "Increment Mode : \n" + current + " / 5";

        if (current == 0 && !currentiszero){
            UI.SetActive(false);
            BuyMore.SetActive(true);
            StartCoroutine(NoMoreText());
            currentiszero = true;
        }
    }

    public void changeUI()
    {
        if (current != 0){
            toggle = !toggle;
            UI.SetActive(toggle);
        }
        else{
            BuyMore.SetActive(true);
            StartCoroutine(NoMoreText());
        }
        


    }    

    IEnumerator NoMoreText()
    {
        yield return new WaitForSeconds(2);
        BuyMore.SetActive(false);
    }
}
