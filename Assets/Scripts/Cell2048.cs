using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell2048 : MonoBehaviour
{
    public Cell2048 right;
    public Cell2048 down;
    public Cell2048 left;
    public Cell2048 up;

    public Fill2048 fill;

    private void OnEnable()
    {
        GameController2048.slide += OnSlide;    
    }

    
    private void OnDisable() 
    {
        GameController2048.slide -= OnSlide;   
    }

    private void OnSlide(string whatWasSent)
    {
        CellCheck();
        //Debug.Log(whatWasSent);
        if(whatWasSent == "w")
        {
            if(up != null)
            {
                return;
            }
            Cell2048 currentCell = this;            // only cells 1-4 can be current cells
            //print(currentCell);
            SlideUp(currentCell);
            
        }

        if(whatWasSent == "d")
        {
            if(right != null)
            {
                return;
            }
            Cell2048 currentCell = this;
            SlideRight(currentCell);
        }

        if(whatWasSent == "s")
        {
            if(down != null)
            {
                return;
            }
            Cell2048 currentCell = this;
            SlideDown(currentCell);
        }

        if(whatWasSent == "a")
        {
            if(left != null)
            {
                return;
            }
            Cell2048 currentCell = this;
            SlideLeft(currentCell);
        }
    }

    void SlideUp(Cell2048 currentCell)
    {
        if(currentCell.down == null)
        {
            return;
        }

        //Debug.Log(currentCell.gameObject);
        if(currentCell.fill != null)
        {
            Cell2048 nextCell = currentCell.down;
            while(nextCell.down != null && nextCell.fill == null)
            {
                nextCell = nextCell.down;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false && currentCell.fill.brick == false)                                       // HERE YOU WILL CHECK IF ITS A BRICK
            {
                //print(nextCell.fill.brick);
                if(currentCell.fill.value == nextCell.fill.value)
                {

                    nextCell.fill.Double();
                    nextCell.fill.transform.parent = currentCell.transform;                 //there was a double so a number got replaced
                    currentCell.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;

                    BreakNearBricks(currentCell);

                }
                else if (currentCell.down.fill != nextCell.fill)
                {
                    //Debug.Log("!doubled");
                    nextCell.fill.transform.parent = currentCell.down.transform;            //there was no double so the number was sent under it
                    currentCell.down.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++; 
                }
            }
        }
        else
        {
            Cell2048 nextCell = currentCell.down;
            while(nextCell.down != null && nextCell.fill == null)
            {
                nextCell = nextCell.down;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false)       // CHECK FOR BRICK
            {
                //print(nextCell.fill.brick);
                nextCell.fill.transform.parent = currentCell.transform;
                currentCell.fill = nextCell.fill;
                nextCell.fill = null;
                GameController2048.ticker++;
                
                SlideUp(currentCell);
                //Debug.Log("Slide to Empty");
            }
        }
        if(currentCell.down == null)
        {
            return;
        }
        SlideUp(currentCell.down);
    }


    void SlideRight(Cell2048 currentCell)
    {
        if(currentCell.left == null)
        {
            return;
        }

        //Debug.Log(currentCell.gameObject);
        if(currentCell.fill != null)
        {
            Cell2048 nextCell = currentCell.left;
            while(nextCell.left != null && nextCell.fill == null)
            {
                nextCell = nextCell.left;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false && currentCell.fill.brick == false)
            {
                if(currentCell.fill.value == nextCell.fill.value)
                {
                    nextCell.fill.Double();
                    nextCell.fill.transform.parent = currentCell.transform;
                    currentCell.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;
                    BreakNearBricks(currentCell);
                    
                }
                else if (currentCell.left.fill != nextCell.fill)
                {
                    //Debug.Log("!doubled");
                    nextCell.fill.transform.parent = currentCell.left.transform;
                    currentCell.left.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;
                }
            }
        }
        else
        {
            Cell2048 nextCell = currentCell.left;
            while(nextCell.left != null && nextCell.fill == null)
            {
                nextCell = nextCell.left;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false)
            {
                nextCell.fill.transform.parent = currentCell.transform;
                currentCell.fill = nextCell.fill;
                nextCell.fill = null;
                GameController2048.ticker++;
                SlideRight(currentCell);
                //Debug.Log("Slide to Empty");
            }
        }


        if(currentCell.left == null)
        {
            return;
        }
        SlideRight(currentCell.left);
    }


    void SlideDown(Cell2048 currentCell)
    {
        if(currentCell.up == null)
        {
            //print(currentCell.name);
            return;
        }

        //Debug.Log(currentCell.gameObject);
        if(currentCell.fill != null)
        {
            Cell2048 nextCell = currentCell.up;
            while(nextCell.up != null && nextCell.fill == null)
            {
                nextCell = nextCell.up;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false && currentCell.fill.brick == false)
            {
                if(currentCell.fill.value == nextCell.fill.value)
                {     
                    nextCell.fill.Double();
                    //print("passed double");
                    nextCell.fill.transform.parent = currentCell.transform;
                    currentCell.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;

                    BreakNearBricks(currentCell);
                }
                else if (currentCell.up.fill != nextCell.fill)
                {
                    //Debug.Log("!doubled");
                    nextCell.fill.transform.parent = currentCell.up.transform;
                    currentCell.up.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;
                }
            }
        }
        else
        {
            Cell2048 nextCell = currentCell.up;
            while(nextCell.up != null && nextCell.fill == null)
            {
                nextCell = nextCell.up;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false)
            {
                nextCell.fill.transform.parent = currentCell.transform;
                currentCell.fill = nextCell.fill;
                nextCell.fill = null;
                GameController2048.ticker++;
                SlideDown(currentCell);
                //Debug.Log("Slide to Empty");
            }
        }
        if(currentCell.up == null)
        {
            return;
        }
        SlideDown(currentCell.up);
    }


    void SlideLeft(Cell2048 currentCell)
    {
        if(currentCell.right == null)
        {
            return;
        }

        //Debug.Log(currentCell.gameObject);
        if(currentCell.fill != null)
        {
            Cell2048 nextCell = currentCell.right;
            while(nextCell.right != null && nextCell.fill == null)
            {
                nextCell = nextCell.right;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false && currentCell.fill.brick == false)
            {
                if(currentCell.fill.value == nextCell.fill.value)
                {
                    nextCell.fill.Double();
                    nextCell.fill.transform.parent = currentCell.transform;
                    currentCell.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;

                    BreakNearBricks(currentCell);
                }
                else if (currentCell.right.fill != nextCell.fill)
                {
                    //Debug.Log("!doubled");
                    nextCell.fill.transform.parent = currentCell.right.transform;
                    currentCell.right.fill = nextCell.fill;
                    nextCell.fill = null;
                    GameController2048.ticker++;
                }
            }
        }
        else
        {
            Cell2048 nextCell = currentCell.right;
            while(nextCell.right != null && nextCell.fill == null)
            {
                nextCell = nextCell.right;
            }
            if(nextCell.fill != null && nextCell.fill.brick == false)
            {
                nextCell.fill.transform.parent = currentCell.transform;
                currentCell.fill = nextCell.fill;
                nextCell.fill = null;
                GameController2048.ticker++;
                SlideLeft(currentCell);
                //Debug.Log("Slide to Empty");
            }
        }
        if(currentCell.right == null)
        {
            return;
        }
        SlideLeft(currentCell.right);
    }


    void CellCheck()
    {
        if(fill == null)
        {
            return;
        }
        if(up != null)
        {
            if(up.fill == null)
            {
                return;
            }
            if(up.fill.value == fill.value && up.fill.brick == false && fill.brick == false)
            {
                return;
            }
        }


        if(right != null)
        {
            if(right.fill == null)
            {
                return;
            }
            if(right.fill.value == fill.value && right.fill.brick == false && fill.brick == false)
            {
                return;
            }
        }


        if(down != null)
        {
            if(down.fill == null)
            {
                return;
            }
            if(down.fill.value == fill.value && down.fill.brick == false && fill.brick == false)
            {
                return;
            }
        }


        if(left != null)
        {
            if(left.fill == null)
            {
                return;
            }
            if(left.fill.value == fill.value && left.fill.brick == false && fill.brick == false)
            {
                return;
            }
        }
        GameController2048.instance.GameOverCheck();
    }

    void BreakNearBricks(Cell2048 currentCell)
    {
        if(currentCell.right != null && currentCell.right.fill != null)
        {
            if(currentCell.right.fill.brick == true)
                {
                    if(currentCell.right.fill.value == currentCell.fill.value){
                        currentCell.right.fill.ReplaceBrick();
                        currentCell.right.fill.brick = false;
                        GameController2048.brickcount--;
                    }
                    
                }
        }
        if(currentCell.up != null && currentCell.up.fill != null)
        {
            if(currentCell.up.fill.brick == true)
            {
                if(currentCell.up.fill.value == currentCell.fill.value){
                    currentCell.up.fill.ReplaceBrick();
                    currentCell.up.fill.brick = false;
                    GameController2048.brickcount--;
                }
            }
        }
        if(currentCell.down != null && currentCell.down.fill != null)
        {
            if(currentCell.down.fill.brick == true)
            {
                if(currentCell.down.fill.value == currentCell.fill.value){
                    currentCell.down.fill.ReplaceBrick();
                    currentCell.down.fill.brick = false;
                    GameController2048.brickcount--;
                }
            }
        }
        if(currentCell.left != null && currentCell.left.fill != null)
        {
            if(currentCell.left.fill.brick == true)
            {
                if(currentCell.left.fill.value == currentCell.fill.value){
                    currentCell.left.fill.ReplaceBrick();
                    currentCell.left.fill.brick = false;
                    GameController2048.brickcount--;
                }
            }
        }
    }
}
