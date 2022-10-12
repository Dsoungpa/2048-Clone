using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Fill2048 : MonoBehaviour
{
    public int value;
    [SerializeField] TextMeshProUGUI valueDisplay;
    [SerializeField] float speed;

    bool hasCombine;

    Image myImage;

    public void FillValueUpdate(int valueIn)
    {
        value = valueIn;
        valueDisplay.text = value.ToString();

        int colorIndex = GetColorIndex(value);
        Debug.Log(colorIndex + " color index");
        myImage = GetComponent<Image>();
        myImage.color = GameController2048.instance.fillColors[colorIndex];
    }

    int GetColorIndex(int valueIn)
    {
        int index = 0;
        while(valueIn != 1)
        {
            index++;
            valueIn /= 2;
        }

        index--;
        return index;
    }

    private void Update()
    {
        if(transform.localPosition != Vector3.zero)
        {
            hasCombine = false;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, speed * Time.deltaTime);
        }
        else if(hasCombine == false)
        {
            if(transform.parent.GetChild(0) != this.transform)
            {
                Destroy(transform.parent.GetChild(0).gameObject);
            }
            hasCombine = true;
        }
    }

    public void Double()                                                // when 2 fill objects are combined
    {
        value *= 2;
        GameController2048.instance.ScoreUpdate(value);
        valueDisplay.text = value.ToString();

        int colorIndex = GetColorIndex(value);
        Debug.Log(colorIndex + " color index");
        
        myImage.color = GameController2048.instance.fillColors[colorIndex];

        GameController2048.instance.WinningCheck(value);

    }
}
