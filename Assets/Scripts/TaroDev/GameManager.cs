using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{

    [SerializeField] public Leaderboard leaderboard;

    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Block obstaclePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private int winCondition = 2048;
    [SerializeField] private bool gameOver;
    [SerializeField] private int score = 0;
    [SerializeField] private int possibleHighScore;
    [SerializeField] private int cycleMovesLeft = 5;
    [SerializeField] private int countUntilObstacle = 5;
    [SerializeField] private int obstacleCount = 0;
   

    [Header("Brick Values")]
    [SerializeField] private int[] brickValues = new int[0];
    [SerializeField] private int[] weightedBrickValues;
    [SerializeField] private int currentHighestValue;

    [Header("UI")]
    [SerializeField] private GameObject winScreen, loseScreen;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highscoreText;
    [SerializeField] private TMP_Text cycleMoves;
    [SerializeField] private GameObject cycleUI;

    [Header("Audio")]
    [SerializeField] private AudioClip swipe;
    [SerializeField] private AudioClip merge;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool muted = false;
    [SerializeField] private AudioListener audioListener;

    [Header("Mobile")]
    private Vector3 startTouchPosition;
    private Vector3 currentPosition;
    private Vector3 endTouchPosition;
    [SerializeField] private bool stopTouch = false;
    [SerializeField] private float swipeRange;

    private List<Node> nodes;
    private List<Block> blocks;
    private List<Block> obstacles;
    private GameState state;
    private int round;

    // Get a Blocktype by the int value you pass
    // It takes from the list of types that already has a color attached to the value
    private BlockType GetBlockTypeByValue(int value) => types.First(t=>t.Value == value);

    void Start()
    {
        highscoreText.text = PlayerPrefs.HasKey("myHighScore") ? PlayerPrefs.GetInt("myHighScore").ToString() : "0";
        ChangeState(GameState.GenerateLevel);
        weightedBrickValues = brickValues;
        currentHighestValue = brickValues[brickValues.Length - 1];
    }

    void Update()
    {
        cycleMoves.text = cycleMovesLeft.ToString();
        possibleHighScore = score;

        if(state != GameState.WaitingInput) return;

        // Keyboard Input
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);

        // Touch Input
        // if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
        //     startTouchPosition = Input.GetTouch(0).position;
        // }

        // if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved){
        //     currentPosition = Input.GetTouch(0).position;
        //     Vector3 Distance = currentPosition - startTouchPosition;

        //     //if (Input.GetKeyDown(KeyCode.W)) {
        //     if(Distance.y > swipeRange){
        //         Shift(Vector2.up);
        //         stopTouch = true;
        //     } 

        //     else if (Distance.y < -swipeRange){
        //     //else if (Input.GetKeyDown(KeyCode.S)) {
        //         Shift(Vector2.down);
        //         stopTouch = true;
        //     }

        //     //if (Input.GetKeyDown(KeyCode.A)) {
        //     else if(Distance.x < -swipeRange){
        //         Shift(Vector2.left);
        //         stopTouch = true;
        //     } 
        //     else if (Distance.x > swipeRange){
        //     //else if (Input.GetKeyDown(KeyCode.D)) {
        //         Shift(Vector2.right);
        //         stopTouch = true;
        //     }
        // }
    }

    private void ChangeState(GameState newState)
    {
        state = newState;

        switch(newState)
        {
            case GameState.GenerateLevel:
                //dprint("Generating...");
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                //print("Spawning..");
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                //print("WaitingInput...");
                break;
            case GameState.Moving:
                //print("Moving...");
                break;
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                loseScreen.SetActive(true);
                break; 
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);      
        }
    }

    void GenerateGrid()
    {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();
        obstacles = new List<Block>();

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x,y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var center = new Vector2((float) width / 2 - 0.5f, (float) height / 2 - 0.5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        ChangeState(GameState.SpawningBlocks);
    }

    void UpdateBrickValue() {
        int[] newBrickValues = new int[brickValues.Length + 1];
        currentHighestValue *= 2; // double current highest value
        brickValues.CopyTo(newBrickValues, 0);
        newBrickValues[newBrickValues.Length - 1] = currentHighestValue;

        brickValues = newBrickValues;
    }

    void UpdateWeightedBrickValues(int[] unweightedBrickValues) {
        int[] tempWeightedBrickValues = new int[AdditionFactorial(unweightedBrickValues.Length)];
        int index = 0;

        int counter = unweightedBrickValues.Length;
        int tempCounter = counter;
        for (int i = 0; i < counter; i++) {
            for (int k = tempCounter; k > 0; k--) {
                tempWeightedBrickValues[index++] = unweightedBrickValues[i];
            }
            tempCounter--;
        }

        weightedBrickValues = tempWeightedBrickValues;
    }

    private int AdditionFactorial(int startNum) {
        int total = 0;

        for (int i = 1; i <= startNum; i++) {
            total += i;
        }

        return total;
    }

    void SpawnBlocks(int amount)
    {
        // Get a list of nodes that are not Occupied by a block from the list of nodes
        var freeNodes = nodes.Where(n=>n.OccupiedBlock == null).OrderBy(b=>Random.value).ToList();

        if(round > 1 && Random.value > 0.9f && obstacleCount < 3)
        {
            foreach (var nodes in freeNodes.Take(amount))
            {
                SpawnObstacle(nodes);
            }
        }

        else
        {
            // For each of the free nodes get a certain amount of Nodes that you want to use
            // for spawning the blocks
            foreach (var nodes in freeNodes.Take(amount))
            {
                SpawnBlock(nodes, Random.value > 0.8f ? 4 : 2);
            }

            if (freeNodes.Count() == 1) {
                //print("FN: " + freeNodes.Count());
                // if(cycleMovesLeft == 0)
                //     return;
                
                var GameOver = (GameOverCheck(Vector2.left) == false && GameOverCheck(Vector2.right) == false && GameOverCheck(Vector2.up) == false && GameOverCheck(Vector2.down) == false) ? true : false;
                //print("Game Over: " + GameOver);
                if(GameOver && cycleMovesLeft <= 0)
                {
                    ChangeState(GameState.Lose);
                    //here
                    StartCoroutine(SubmitScore(score));
                    return;
                }

                else
                {
                    ChangeState(GameState.WaitingInput);
                    return;
                }
            }     
        }

        // "b=>b" - "Is there any..."
        ChangeState(blocks.Any(b=>b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    IEnumerator SubmitScore(int score){
        yield return leaderboard.SubmitScoreRoutine(score);
    }

    void SpawnBlock(Node node, int value)
    {
        // Instantiate a block prefab at the chosen node location
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);    
        block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.5f).SetEase(Ease.OutBounce);

        // Take the block game object and initialize it as a BlockType
        block.Init(GetBlockTypeByValue(value));

        // Assign the node to the Block and visa versa
        block.SetBlock(node);

        // Add block to the list
        blocks.Add(block);
    }

    void SpawnObstacle(Node node)
    {   
            // Instantiate a block prefab at the chosen node location
            var block = Instantiate(obstaclePrefab, node.Pos, Quaternion.identity);    
            block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.5f).SetEase(Ease.OutBounce);
            node.Obstacle = true;
            block.Obstacle = true;

            // List<int> fivePercents = new List<int>(new int[] {16, 32, 64}) ;
            // List<int> twoPercents = new List<int>(new int[] {128, 256, 512, 1024});
            UpdateWeightedBrickValues(brickValues);
            int value = weightedBrickValues[Random.Range(0, weightedBrickValues.Length)];

            // if(Random.value <= 0.6f){
            //     value = 4;
            // }
            // else if(Random.value <= 0.75f){
            //     value = 16;
            // }
            // else if(Random.value <= 0.90f){
            //     value = fivePercents[Random.Range(0, 3)];
            // }
            // else if(Random.value <= 1f){
            //     value = twoPercents[Random.Range(0, 4)];
            // }

            block.Value = value;
            block.text.text = block.Value.ToString();

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
            obstacleCount++;
    }

    private Block SpawnBlockWithNoNode(Vector2 spawn, Node node, int value, bool obstacle)
    {
        Block block;
        // Instantiate a block prefab at the chosen node location
        if(obstacle)
        {
            block = Instantiate(obstaclePrefab, spawn, Quaternion.identity);    
            block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.01f).SetEase(Ease.OutBounce);
            block.Obstacle = true;
            node.Obstacle = true;

            block.Value = value;
            block.text.text = block.Value.ToString();

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
        }
        else
        {
            // Take the block game object and initialize it as a BlockType
            block = Instantiate(blockPrefab, spawn, Quaternion.identity);    
            block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.01f).SetEase(Ease.OutBounce);
            block.Init(GetBlockTypeByValue(value));

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
        }
        

        
        return block;
    }

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        // Order the block array by x then by y if it is moving left or down
        // Or change it to ordered by y then x if it is moving right or up
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x ).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        bool blocksMoved = false;

        // Go through the ordered list of blocks
        foreach (var block in orderedBlocks)
        {
            if(block.Obstacle)
                continue;
            // I think of "next" like current node that the block is on
            var next = block.Node;
            do {
                // Make sure the current block sets this as the node
                block.SetBlock(next);

                // Get the next Node in the direction you want to move
                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                //If there is a possible node in that direction
                if(possibleNode != null)
                {
                    if(possibleNode.Obstacle == false)
                    {
                        // We know a node is present
                        // If its possible to merge, set merge
                        if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                        {
                            block.MergeBlock(possibleNode.OccupiedBlock);
                            blocksMoved = true;
                        }

                        // Otherwise can we move to this spot?
                        else if(possibleNode.OccupiedBlock == null){
                            next = possibleNode;
                            blocksMoved = true;
                        }
                    }
                    
                    // None hit? End do while loop
                }
            } while (next != block.Node);
               
        }

        if(blocksMoved)
        {
            var sequence = DOTween.Sequence();

            foreach(var block in orderedBlocks)
            {
                var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

                sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
            }

            sequence.OnComplete(() => {
                var mergeBlocks = orderedBlocks.Where(b => b.MergingBlock != null).ToList();

                foreach (var block in mergeBlocks)
                {
                    // foreach(var obstacle in obstacles)
                    //     {
                            
                    //         var nodeUp = GetNodeAtPosition(obstacle.Pos + Vector2.up);
                    //         print(nodeUp.Pos);
                    //         var nodeDown = GetNodeAtPosition(obstacle.Pos + Vector2.down);
                    //         print(nodeDown.Pos);
                    //         var nodeLeft = GetNodeAtPosition(obstacle.Pos + Vector2.left);
                    //         print(nodeLeft.Pos);
                    //         var nodeRight = GetNodeAtPosition(obstacle.Pos + Vector2.right);
                    //         print(nodeRight.Pos);

                    //         if((nodeUp && block.Node == nodeUp) || (nodeDown && block.Node == nodeDown) || (nodeLeft && block.Node == nodeLeft) || (nodeRight && block.Node == nodeRight))
                    //         {
                    //             //obstacles.Remove(obstacle);
                    //             print(obstacle.Pos);
                    //         }
                            
                    //     }
                    var nodeUp = GetNodeAtPosition(block.Pos + Vector2.up);
                    var nodeDown = GetNodeAtPosition(block.Pos + Vector2.down);
                    var nodeLeft = GetNodeAtPosition(block.Pos + Vector2.left);
                    var nodeRight = GetNodeAtPosition(block.Pos + Vector2.right);

                    if(nodeUp && nodeUp.Obstacle && nodeUp.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeUp.Obstacle = false;
                        RemoveBlock(nodeUp.OccupiedBlock);
                    }

                    if(nodeDown && nodeDown.Obstacle && nodeDown.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeDown.Obstacle = false;
                        RemoveBlock(nodeDown.OccupiedBlock);
                    }

                    if(nodeLeft && nodeLeft.Obstacle && nodeLeft.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeLeft.Obstacle = false;
                        RemoveBlock(nodeLeft.OccupiedBlock);
                    }

                    if(nodeRight && nodeRight.Obstacle && nodeRight.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeRight.Obstacle = false;
                        RemoveBlock(nodeRight.OccupiedBlock);
                    }

                    MergeBlocks(block.MergingBlock, block);
                    
                    
                }

                if(mergeBlocks.Any()) audioSource.PlayOneShot(merge, 0.2f);
                
                ChangeState(GameState.SpawningBlocks);
            });

            audioSource.PlayOneShot(swipe, 0.2f);
        }

        
        else
        {
            ChangeState(GameState.WaitingInput);
        }
        
    }

    public void Cycle(GameObject locationCheck)
    {
        if(cycleMovesLeft == 0)
            return; 

        int blockCoordinate = 0;
        int loopCoordinateCheck = 0;

        Vector2 moveDestroyedBlock = new Vector2(0f, 0f);
        Vector2 moveTo = new Vector2(0f, 0f);
        Vector2 noNodeBlockLocation = new Vector2(0f, 0f);
        Vector2 directionOfCycle = new Vector2(0f, 0f);

        bool xAxis = false;
        bool yAxis = false;
        bool reverse = false;

        List<Block> orderedBlocks = new List<Block>();
        var tag = locationCheck.tag;
        switch(tag){
            case "FirstRowLeft":
                blockCoordinate = 3;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 3f);
                moveTo = new Vector2(3f, 3f);
                noNodeBlockLocation = new Vector2(4f, 3f);
                directionOfCycle = Vector2.left;

                yAxis = true;
                break;

            case "FirstRowRight":
                blockCoordinate = 3;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 3f);
                moveTo = new Vector2(0f, 3f);
                noNodeBlockLocation = new Vector2(-1f, 3f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "SecRowLeft":
                blockCoordinate = 2;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 2f);
                moveTo = new Vector2(3f, 2f);
                noNodeBlockLocation = new Vector2(4f, 2f);
                directionOfCycle = Vector2.left;
                
                yAxis = true;
                break;

            case "SecRowRight":
                blockCoordinate = 2;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 2f);
                moveTo = new Vector2(0f, 2f);
                noNodeBlockLocation = new Vector2(-1f, 2f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "ThirdRowLeft":
                blockCoordinate = 1;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 1f);
                moveTo = new Vector2(3f, 1f);
                noNodeBlockLocation = new Vector2(4f, 1f);
                directionOfCycle = Vector2.left;

                yAxis = true;
                break;

            case "ThirdRowRight":
                blockCoordinate = 1;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 1f);
                moveTo = new Vector2(0f, 1f);
                noNodeBlockLocation = new Vector2(-1f, 1f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "FourthRowLeft":
                blockCoordinate = 0;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 0f);
                moveTo = new Vector2(3f, 0f);
                noNodeBlockLocation = new Vector2(4f, 0f);
                directionOfCycle = Vector2.left;

                yAxis = true;
                break;

            case "FourthRowRight":
                blockCoordinate = 0;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 0f);
                moveTo = new Vector2(0f, 0f);
                noNodeBlockLocation = new Vector2(-1f, 0f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "FirstColUp":
                blockCoordinate = 0;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(0f, -1f);
                moveTo = new Vector2(0f, 0f);
                noNodeBlockLocation = new Vector2(0f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;

            case "FirstColDown":
                blockCoordinate = 0;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(0f, 4f);
                moveTo = new Vector2(0f, 3f);
                noNodeBlockLocation = new Vector2(0f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;

            case "SecColUp":
                blockCoordinate = 1;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(1f, -1f);
                moveTo = new Vector2(1f, 0f);
                noNodeBlockLocation = new Vector2(1f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;

            case "SecColDown":
                blockCoordinate = 1;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(1f, 4f);
                moveTo = new Vector2(1f, 3f);
                noNodeBlockLocation = new Vector2(1f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;

            case "ThirdColUp":
                blockCoordinate = 2;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(2f, -1f);
                moveTo = new Vector2(2f, 0f);
                noNodeBlockLocation = new Vector2(2f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;
            case "ThirdColDown":
                blockCoordinate = 2;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(2f, 4f);
                moveTo = new Vector2(2f, 3f);
                noNodeBlockLocation = new Vector2(2f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;
            case "FourthColUp":
                blockCoordinate = 3;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(3f, -1f);
                moveTo = new Vector2(3f, 0f);
                noNodeBlockLocation = new Vector2(3f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;
            case "FourthColDown":
                blockCoordinate = 3;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(3f, 4f);
                moveTo = new Vector2(3f, 3f);
                noNodeBlockLocation = new Vector2(3f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;
        }

        foreach(var block in blocks)
        {
            if(xAxis){
                if(block.Pos.x == blockCoordinate){
                    orderedBlocks.Add(block);
                }
            }

            else if(yAxis){
                if(block.Pos.y == blockCoordinate){
                    orderedBlocks.Add(block);
                }
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }
        cycleMovesLeft--;

        orderedBlocks.OrderBy(b => b.Pos.x ).ThenBy(b => b.Pos.y).ToList();
        if(reverse){
            orderedBlocks.Reverse();
        }

        foreach(var block in orderedBlocks){
            print("ORDER: " + block.Pos);
        }
        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
                var currentNode = GetNodeAtPosition(block.Pos);
                if(!xAxis && block.Pos.x == loopCoordinateCheck)
                {
                    //print("Means you have to cycle");
                    var nodeMovingTo = GetNodeAtPosition(moveTo);
                    //sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                    //var newBlock = SpawnBlockWithNoNode(noNodeBlockLocation, nodeMovingTo, block.Value, block.Obstacle);
                    sequence.Insert(0, block.transform.DOMove(moveTo, travelTime));
                    block.SetBlock(nodeMovingTo);
                    if(block.Obstacle){
                        currentNode.Obstacle = false;
                        nodeMovingTo.Obstacle = true;
                    }
                    //RemoveBlock(block);
                    if(nodeMovingTo.OccupiedBlock == null){
                        nodeMovingTo.OccupiedBlock = block;
                    }
                    continue;
                }

                else if(!yAxis && block.Pos.y == loopCoordinateCheck)
                {
                    //print("Means you have to cycle");
                    var nodeMovingTo = GetNodeAtPosition(moveTo);
                    //sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                    //var newBlock = SpawnBlockWithNoNode(noNodeBlockLocation, nodeMovingTo, block.Value, block.Obstacle);
                    sequence.Insert(0, block.transform.DOMove(moveTo, travelTime));
                    block.SetBlock(nodeMovingTo);
                    if(block.Obstacle){
                        currentNode.Obstacle = false;
                        nodeMovingTo.Obstacle = true;
                    }
                    //RemoveBlock(block);
                    if(nodeMovingTo.OccupiedBlock == null){
                        nodeMovingTo.OccupiedBlock = block;
                    }
                    continue;
                }

            var possibleNode = GetNodeAtPosition(block.Pos + directionOfCycle);
            var movePoint = possibleNode.Pos;
            
            print("Block: " + block.Pos +"," + "Node: " + possibleNode.Pos);
            block.SetBlock(possibleNode);
            if(block.Obstacle){
                currentNode.Obstacle = false;
                possibleNode.Obstacle = true;
            }
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));

        }

            audioSource.PlayOneShot(swipe, 0.2f);

        //print("OrderedBlocks:" + orderedBlocks.Count());
    }

    bool GameOverCheck(Vector2 dir)
    {
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) 
            //print("right or up");
            orderedBlocks.Reverse();

        

        foreach (var block in orderedBlocks)
        {
            if(block.Obstacle){
                continue;
            }
            var next = block.Node;
            do {
                //block.SetBlock(next);
                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if(possibleNode != null)
                {
                    if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        // print("First");
                        // print("Block Pos: " + block.Pos);
                        // print("Value: " + block.Value);
                        // print("Not Null: " + possibleNode.OccupiedBlock != null);
                        // print("Can Merge: " + possibleNode.OccupiedBlock.CanMerge(block.Value));
                        return true;
                    }

                    else if(possibleNode.OccupiedBlock == null){
                        return true;
                    } 
                }
            } while (next != block.Node);
        }

        return false;
    }

    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        score += baseBlock.Value * 2;

        if (baseBlock.Value * 2 > currentHighestValue) UpdateBrickValue();

        scoreText.text = score.ToString();
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {   
        if(block.Obstacle){
            SpawnBlock(block.Node, block.Value);
            obstacleCount--;
        }
        blocks.Remove(block);
        Destroy(block.gameObject);
    }


    Node GetNodeAtPosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        if(possibleHighScore > (PlayerPrefs.GetInt("myHighScore")))
        {
            PlayerPrefs.SetInt("myHighScore", possibleHighScore);
        }
        
    }

    public void ContinueGame()
    {
        winScreen.SetActive(false);
        ChangeState(GameState.WaitingInput);
    }

    public void ToggleCycle()
    {
        if(cycleUI.activeSelf)
        {
            cycleUI.SetActive(false);
        }

        else
        {
            cycleUI.SetActive(true);
        }
    }

    public void ToggleAudio()
    {
        if(!muted){
            audioListener.enabled = false;
            muted = true;
        }

        else if(muted){
            audioListener.enabled = true;
            muted = false;
        }
    }
}

[Serializable]
public struct BlockType 
{
    public int Value;
    public Color Color;
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
