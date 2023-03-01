using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    public bool Obstacle = false;
    public bool clicked = false;
    private GameManager gameManagerScript;
    private ColorTheme colorThemeScript;
    public GameObject clickedIndicator;

    public Vector2 Pos => transform.position;
    public SpriteRenderer renderer;
    public TextMeshPro text;

    void Awake()
    {
        // Enable the collider component
        GetComponent<Collider2D>().enabled = true;
    }

     void Start()
    {
        // Find the game object with the tag "GameManager"
        GameObject gameManagerObject = GameObject.FindWithTag("GameManager");

        // Get the GameManager script component
        gameManagerScript = gameManagerObject.GetComponent<GameManager>();
    }

    void Update(){
        // if(!clicked){
        //     clickedIndicator.SetActive(false);
        // }

        // Create a ray from the mouse position to the world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Create a RaycastHit2D object to store the result of the raycast
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        // Check if the raycast hit a collider
        if (hit.collider != null)
        {
            // Print the name of the game object that was hit
            // Debug.Log(hit.collider.gameObject.name);
        }
    }

    public void Init(BlockType type) {
        Value = type.Value;
        renderer.color = type.Color;
        text.color = (Value == 2 || Value == 4) ? new Color32(0, 0, 0, 150) : new Color32(255, 255, 255, 255);
        text.text = type.Value.ToString();
    }

    public void SetBlock(Node node) {
        if(Node != null) Node.OccupiedBlock = null;
        Node = node;
        Node.OccupiedBlock = this;
        if (Obstacle) Node.Obstacle = true;
    }

    public void ClearBlock() {
        Node.OccupiedBlock = null;
        Node.Obstacle = false;
        Node = null;
    }

    public void MergeBlock(Block blockToMergeWith) {
        // Set the block we are merging with
        MergingBlock = blockToMergeWith;

        // Set current node as unoccupied to allow blocks to use it
        Node.OccupiedBlock = null;

        // Set the base block as merging, so it does not get used twice
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(int value) => Obstacle == false && value == Value && !Merging && MergingBlock == null;


    // Keyboard Version
    // void OnMouseUp()
    // {
    //     if (gameManagerScript.disableControl) return; // disable control while updating username

    //     if(gameManagerScript.phase > 4){
    //         if(!gameManagerScript.cyclesMode){
    //         clickedIndicator.SetActive(true);
    //         clicked = true;
    //         StartCoroutine(CycleCoolDown());
    //         gameManagerScript.clickedBlock = this;
    //         gameManagerScript.SetCycleTrue();
    //         print("block selected");
    //         Debug.Log(this.gameObject.name + " clicked!");
    //     }
    
    //     else if (clicked){
    //         print("Deselected");
    //         //gameManagerScript.clearClickedIndicator();
    //     }
    //     }
        
        
    // }

    IEnumerator CycleCoolDown(){
        yield return new WaitForSeconds(.1f);
        gameManagerScript.inCycle = true;
    }

    // // Mobile Version
    // void OnTouchDown()
    // {
    // if(!gameManagerScript.cyclesMode){
    //     if(Input.touchCount > 0) {
    //         Touch touch = Input.GetTouch(0);
    //         if(touch.phase == TouchPhase.Began) {
    //             clickedIndicator.SetActive(true);
    //             clicked = true;
    //             gameManagerScript.clickedBlock = this;
    //             gameManagerScript.SetCycleTrue();
    //             Debug.Log(this.gameObject.name + " touched!");
    //         }
    //     }
    // }
    // else{
    //     print("else");
    //     gameManagerScript.clearClickedIndicator();
    //     } 
    // }
}
