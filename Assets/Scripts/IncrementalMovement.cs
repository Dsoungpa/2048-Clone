using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IncrementalMovement : MonoBehaviour
{

    public Button button;
    public Cell2048 FirstCell;
    
    bool firstwent = false;
    bool lastwent = false;

    public void IncrementLeft(Cell2048 FirstCell)
    {
        IncrementUI.current--;

        Cell2048 LastCell = FirstCell.right.right.right;        //Cell 4(last one at the right)                          
        Cell2048 SecondCell = FirstCell.right;                  //Cell 2
        Cell2048 ThirdCell = FirstCell.right.right;             //Cell 3
        firstwent = false;                                      // bool to check first cell 

        if (LastCell.fill != null){
            Fill2048 LastCellClone = Fill2048.Instantiate(LastCell.fill, LastCell.fill.transform.position, LastCell.fill.transform.rotation);   //clones

            if (FirstCell.fill != null){
                FirstCell.fill.transform.parent = LastCell.transform;   //Moves Cell 1 location to Cell 4 Location
                LastCell.fill = FirstCell.fill;                         //Moves Cell 1 Fill to Cell 4 Fill
                FirstCell.fill = null;                                  // clears anything in Cell 1
                firstwent = true;
            }
            
            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = FirstCell.transform; //Moves Cell 2 location to Cell 1 Location
                FirstCell.fill = SecondCell.fill;                       //Moves Cell 2 Fill to Cell 1 Fill
                SecondCell.fill = null;
            }
            
            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = SecondCell.transform; //Moves Cell 3 location to Cell 2 Location
                SecondCell.fill = ThirdCell.fill;                       //Moves Cell 3 fill to Cell 2 Fill
                ThirdCell.fill = null;
            }
            
            if (LastCell.fill != null && !firstwent){                               // CASE WHEN LAST CELL MOVES TO THIRD CELL AND NO CELL NEEDS TO BE OVERWRITTEN IN FOURTH CELL(LAST)
                LastCell.fill.transform.parent = ThirdCell.transform;
                LastCellClone.transform.parent = ThirdCell.transform;
                ThirdCell.fill = LastCellClone;
                ThirdCell.fill.transform.localScale = new Vector3(1, 1, 1);
                LastCell.fill = null;
                
            }

            if (LastCell.fill != null && firstwent){                                //CASE WHEN LAST CELL GETS OVERWRITTEN BY FIRST CELL THEREFORE YOU NEED CLONE
                LastCellClone.transform.parent = ThirdCell.transform;
                ThirdCell.fill = LastCellClone;
                ThirdCell.fill.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else{                                                                                                       // NO OVERWRITES ARE NEEDED SIMPLY MOVE
            if (FirstCell.fill != null){
                FirstCell.fill.transform.parent = LastCell.transform;   //Moves Cell 1 location to Cell 4 Location
                LastCell.fill = FirstCell.fill;                         //Moves Cell 1 Fill to Cell 4 Fill
                FirstCell.fill = null;                                  // clears anything in Cell 1
                firstwent = true; 
            }
            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = FirstCell.transform; //Moves Cell 2 location to Cell 1 Location
                FirstCell.fill = SecondCell.fill;                       //Moves Cell 2 Fill to Cell 1 Fill
                SecondCell.fill = null;
            }
            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = SecondCell.transform; //Moves Cell 3 location to Cell 2 Location
                SecondCell.fill = ThirdCell.fill;                       //Moves Cell 3 fill to Cell 2 Fill
                ThirdCell.fill = null;
            }
           if (LastCell.fill != null && !firstwent){
                LastCell.fill.transform.parent = ThirdCell.transform;
                ThirdCell.fill = LastCell.fill;
                LastCell.fill = null;
           }
        } 
    }

    public void IncrementRight(Cell2048 FirstCell)
    {
        IncrementUI.current--;

        Cell2048 LastCell = FirstCell.right.right.right;        //Cell 4(last one at the right)                          
        Cell2048 SecondCell = FirstCell.right;                  //Cell 2
        Cell2048 ThirdCell = FirstCell.right.right;             //Cell 3
        lastwent = false;

        if (FirstCell.fill != null){
            Fill2048 FirstCellClone = Fill2048.Instantiate(FirstCell.fill, FirstCell.fill.transform.position, FirstCell.fill.transform.rotation);

            if (LastCell.fill != null){
                LastCell.fill.transform.parent = FirstCell.transform;   //Moves Cell 4 location to Cell 1 Location
                FirstCell.fill = LastCell.fill;                         //Moves Cell 4 fill to Cell 1 fill
                LastCell.fill = null;
                lastwent = true;
            }

            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = LastCell.transform;
                LastCell.fill = ThirdCell.fill;
                ThirdCell.fill = null;
            }

            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = ThirdCell.transform;
                ThirdCell.fill = SecondCell.fill;
                SecondCell.fill = null;
            }

            if (FirstCell.fill != null && !lastwent){
                FirstCell.fill.transform.parent = SecondCell.transform;
                FirstCellClone.transform.parent = SecondCell.transform;
                SecondCell.fill = FirstCellClone;
                SecondCell.fill.transform.localScale = new Vector3(1, 1, 1);
                FirstCell.fill = null;
            }

            if (FirstCell.fill != null && lastwent){
                FirstCellClone.transform.parent = SecondCell.transform;
                SecondCell.fill = FirstCellClone;
                SecondCell.fill.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else{
            if (LastCell.fill != null){
                LastCell.fill.transform.parent = FirstCell.transform;   //Moves Cell 4 location to Cell 1 Location
                FirstCell.fill = LastCell.fill;                         //Moves Cell 4 fill to Cell 1 fill
                LastCell.fill = null;
                lastwent = true;
            }

            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = LastCell.transform;
                LastCell.fill = ThirdCell.fill;
                ThirdCell.fill = null;
            }

            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = ThirdCell.transform;
                ThirdCell.fill = SecondCell.fill;
                SecondCell.fill = null;
            }

            if (FirstCell.fill != null && !lastwent){
                FirstCell.fill.transform.parent = SecondCell.transform;
                SecondCell.fill = FirstCell.fill;
                FirstCell.fill = null;
            }
        }
    }

    public void IncrementUp(Cell2048 FirstCell)
    {
        IncrementUI.current--;

        Cell2048 LastCell = FirstCell.down.down.down;
        Cell2048 SecondCell = FirstCell.down;
        Cell2048 ThirdCell = FirstCell.down.down;
        firstwent = false;

        if (LastCell.fill != null){
            Fill2048 LastCellClone = Fill2048.Instantiate(LastCell.fill, LastCell.fill.transform.position, LastCell.fill.transform.rotation);   //clones

            if (FirstCell.fill != null){
                FirstCell.fill.transform.parent = LastCell.transform;   //Moves Cell 1 location to Cell 4 Location
                LastCell.fill = FirstCell.fill;                         //Moves Cell 1 Fill to Cell 4 Fill
                FirstCell.fill = null;                                  // clears anything in Cell 1
                firstwent = true;
            }
            
            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = FirstCell.transform; //Moves Cell 2 location to Cell 1 Location
                FirstCell.fill = SecondCell.fill;                       //Moves Cell 2 Fill to Cell 1 Fill
                SecondCell.fill = null;
            }
            
            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = SecondCell.transform; //Moves Cell 3 location to Cell 2 Location
                SecondCell.fill = ThirdCell.fill;                       //Moves Cell 3 fill to Cell 2 Fill
                ThirdCell.fill = null;
            }
            
            if (LastCell.fill != null && !firstwent){                               // CASE WHEN LAST CELL MOVES TO THIRD CELL AND NO CELL NEEDS TO BE OVERWRITTEN IN FOURTH CELL(LAST)
                LastCell.fill.transform.parent = ThirdCell.transform;
                LastCellClone.transform.parent = ThirdCell.transform;
                ThirdCell.fill = LastCellClone;
                ThirdCell.fill.transform.localScale = new Vector3(1, 1, 1);
                LastCell.fill = null;
                
            }

            if (LastCell.fill != null && firstwent){                                //CASE WHEN LAST CELL GETS OVERWRITTEN BY FIRST CELL THEREFORE YOU NEED CLONE
                LastCellClone.transform.parent = ThirdCell.transform;
                ThirdCell.fill = LastCellClone;
                ThirdCell.fill.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else{                                                                                                       // NO OVERWRITES ARE NEEDED SIMPLY MOVE
            if (FirstCell.fill != null){
                FirstCell.fill.transform.parent = LastCell.transform;   //Moves Cell 1 location to Cell 4 Location
                LastCell.fill = FirstCell.fill;                         //Moves Cell 1 Fill to Cell 4 Fill
                FirstCell.fill = null;                                  // clears anything in Cell 1
                firstwent = true; 
            }
            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = FirstCell.transform; //Moves Cell 2 location to Cell 1 Location
                FirstCell.fill = SecondCell.fill;                       //Moves Cell 2 Fill to Cell 1 Fill
                SecondCell.fill = null;
            }
            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = SecondCell.transform; //Moves Cell 3 location to Cell 2 Location
                SecondCell.fill = ThirdCell.fill;                       //Moves Cell 3 fill to Cell 2 Fill
                ThirdCell.fill = null;
            }
           if (LastCell.fill != null && !firstwent){
                LastCell.fill.transform.parent = ThirdCell.transform;
                ThirdCell.fill = LastCell.fill;
                LastCell.fill = null;
           }
        } 
    }

    public void IncrementDown(Cell2048 FirstCell)
    {
        IncrementUI.current--;
        
        Cell2048 LastCell = FirstCell.down.down.down;
        Cell2048 SecondCell = FirstCell.down;
        Cell2048 ThirdCell = FirstCell.down.down;
        lastwent = false;

        if (FirstCell.fill != null){
            Fill2048 FirstCellClone = Fill2048.Instantiate(FirstCell.fill, FirstCell.fill.transform.position, FirstCell.fill.transform.rotation);

            if (LastCell.fill != null){
                LastCell.fill.transform.parent = FirstCell.transform;   //Moves Cell 4 location to Cell 1 Location
                FirstCell.fill = LastCell.fill;                         //Moves Cell 4 fill to Cell 1 fill
                LastCell.fill = null;
                lastwent = true;
            }

            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = LastCell.transform;
                LastCell.fill = ThirdCell.fill;
                ThirdCell.fill = null;
            }

            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = ThirdCell.transform;
                ThirdCell.fill = SecondCell.fill;
                SecondCell.fill = null;
            }

            if (FirstCell.fill != null && !lastwent){
                FirstCell.fill.transform.parent = SecondCell.transform;
                FirstCellClone.transform.parent = SecondCell.transform;
                SecondCell.fill = FirstCellClone;
                SecondCell.fill.transform.localScale = new Vector3(1, 1, 1);
                FirstCell.fill = null;
            }

            if (FirstCell.fill != null && lastwent){
                FirstCellClone.transform.parent = SecondCell.transform;
                SecondCell.fill = FirstCellClone;
                SecondCell.fill.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else{
            if (LastCell.fill != null){
                LastCell.fill.transform.parent = FirstCell.transform;   //Moves Cell 4 location to Cell 1 Location
                FirstCell.fill = LastCell.fill;                         //Moves Cell 4 fill to Cell 1 fill
                LastCell.fill = null;
                lastwent = true;
            }

            if (ThirdCell.fill != null){
                ThirdCell.fill.transform.parent = LastCell.transform;
                LastCell.fill = ThirdCell.fill;
                ThirdCell.fill = null;
            }

            if (SecondCell.fill != null){
                SecondCell.fill.transform.parent = ThirdCell.transform;
                ThirdCell.fill = SecondCell.fill;
                SecondCell.fill = null;
            }

            if (FirstCell.fill != null && !lastwent){
                FirstCell.fill.transform.parent = SecondCell.transform;
                SecondCell.fill = FirstCell.fill;
                FirstCell.fill = null;
            }
        }
    }
}
