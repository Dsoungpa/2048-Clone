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
    }

    void Update()
    {
        cycleMoves.text = cycleMovesLeft.ToString();
        possibleHighScore = score;

        if(state != GameState.WaitingInput) return;

        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);

        if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);

        if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }

    private void ChangeState(GameState newState)
    {
        state = newState;

        switch(newState)
        {
            case GameState.GenerateLevel:
                //print("Generating...");
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

    void SpawnBlocks(int amount)
    {
        // Get a list of nodes that are not Occupied by a block from the list of nodes
        var freeNodes = nodes.Where(n=>n.OccupiedBlock == null).OrderBy(b=>Random.value).ToList();

        if(round > 1 && Random.value > 0.9f)
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
                if(cycleMovesLeft == 0)
                    return;
                
                var GameOver = (GameOverCheck(Vector2.left) == false && GameOverCheck(Vector2.right) == false && GameOverCheck(Vector2.up) == false && GameOverCheck(Vector2.down) == false) ? true : false;
                if(GameOver)
                {
                    ChangeState(GameState.Lose);
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

        List<int> fivePercents = new List<int>(new int[] {16, 32, 64}) ;
        List<int> twoPercents = new List<int>(new int[] {128, 256, 512, 1024});
        int value = 0;

        if(Random.value <= 0.4f){
            value = 2;
        }
        else if(Random.value <= 0.6f){
            value = 4;
        }
        else if(Random.value <= 0.75f){
            value = 16;
        }
        else if(Random.value <= 0.90f){
            value = fivePercents[Random.Range(0, 3)];
        }
        else if(Random.value <= 1f){
            value = twoPercents[Random.Range(0, 4)];
        }

        block.Value = value;
        block.text.text = block.Value.ToString();

        // Assign the node to the Block and visa versa
        block.SetBlock(node);

        // Add block to the list
        blocks.Add(block);
    }

    private Block SpawnBlockWithNoNode(Vector2 spawn, Node node, int value)
    {
        // Instantiate a block prefab at the chosen node location
        var block = Instantiate(blockPrefab, spawn, Quaternion.identity);    
        block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.01f).SetEase(Ease.OutBounce);

        if(node.Obstacle)
        {
            block.Obstacle = true;
        }
        else
        {
            // Take the block game object and initialize it as a BlockType
            block.Init(GetBlockTypeByValue(value));
        }
        

        // Assign the node to the Block and visa versa
        block.SetBlock(node);

        // Add block to the list
        blocks.Add(block);
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

                    if(nodeUp && nodeUp.Obstacle && nodeUp.OccupiedBlock.Value == block.Value)
                    {
                        nodeUp.Obstacle = false;
                        RemoveBlock(nodeUp.OccupiedBlock);
                    }

                    if(nodeDown && nodeDown.Obstacle && nodeDown.OccupiedBlock.Value == block.Value)
                    {
                        nodeDown.Obstacle = false;
                        RemoveBlock(nodeDown.OccupiedBlock);
                    }

                    if(nodeLeft && nodeLeft.Obstacle && nodeLeft.OccupiedBlock.Value == block.Value)
                    {
                        nodeLeft.Obstacle = false;
                        RemoveBlock(nodeLeft.OccupiedBlock);
                    }

                    if(nodeRight && nodeRight.Obstacle && nodeRight.OccupiedBlock.Value == block.Value)
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

    // ALL LEFT SHIFT BUTTONS
    // TOP ROW
    public void TopRowLeftShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;
    
        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 3)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }

        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);

        
        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(-1f, 3f);
                var moveTo = new Vector2(3f, 3f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(4f, 3f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.left);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 2ND FROM TOP ROW
    public void SecondRowLeftShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 2)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(-1f, 2f);
                var moveTo = new Vector2(3f, 2f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(4f, 2f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.left);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 3RD FROM TOP ROW
    public void ThirdRowLeftShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 1)
            {
                orderedBlocks.Add(block);
            }
        }
        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }

        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(-1f, 1f);
                var moveTo = new Vector2(3f, 1f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(4f, 1f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.left);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 4TH FROM TOP ROW
    public void FourthRowLeftShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 0)
            {
                orderedBlocks.Add(block);
            }
        }
        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(-1f, 0f);
                var moveTo = new Vector2(3f, 0f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(4f, 0f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.left);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // ALL RIGHT SHIFT BUTTONS
    // TOP ROW
    public void TopRowRightShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 3)
            {
                orderedBlocks.Add(block);
            }
        }
        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(4f, 3f);
                var moveTo = new Vector2(0f, 3f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(-1f, 3f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.right);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 2ND FROM TOP ROW
    public void SecondRowRightShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 2)
            {
                orderedBlocks.Add(block);
            }
        }
        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(4f, 2f);
                var moveTo = new Vector2(0f, 2f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(-1f, 2f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.right);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 3RD FROM TOP ROW
    public void ThirdRowRightShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 1)
            {
                orderedBlocks.Add(block);
            }
        }
        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(4f, 1f);
                var moveTo = new Vector2(0f, 1f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(-1f, 1f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.right);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 4TH FROM TOP ROW
    public void FourthRowRightShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.y == 0)
            {
                orderedBlocks.Add(block);
            }
        }
        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest x
        orderedBlocks.OrderBy(b => b.Pos.x);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.x == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(4f, 0f);
                var moveTo = new Vector2(0f, 0f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(-1f, 0f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.right);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 1ST COLUMN UP
    public void FirstColUpShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 0)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(0f, -1f);
                var moveTo = new Vector2(0f, 0f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(0f, -1f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.up);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 2ND COLUMN UP
    public void SecondColUpShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 1)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(1f, -1f);
                var moveTo = new Vector2(1f, 0f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(1f, -1f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.up);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 3RD COLUMN UP
    public void ThirdColUpShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 2)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(2f, -1f);
                var moveTo = new Vector2(2f, 0f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(2f, -1f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.up);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 4TH COLUMN UP
    public void FourthColUpShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 3)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);
        orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 3)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(3f, -1f);
                var moveTo = new Vector2(3f, 0f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(3f, -1f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.up);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 1St COLUMN DOWN
    public void FirstColDownShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 0)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(0f, 4f);
                var moveTo = new Vector2(0f, 3f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(0f, 4f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.down);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 2ND COLUMN DOWN
    public void SecondColDownShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 1)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(1f, 4f);
                var moveTo = new Vector2(1f, 3f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(1f, 4f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.down);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 3RD COLUMN DOWN
    public void ThirdColDownShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 2)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(2f, 4f);
                var moveTo = new Vector2(2f, 3f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(2f, 4f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.down);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    // 4TH COLUMN DOWN
    public void FourthColDownShiftTest()
    {
        if(cycleMovesLeft == 0)
            return;

        List<Block> orderedBlocks = new List<Block>();

        // fill the list with blocks on the top row
        foreach(var block in blocks)
        {
            if(block.Pos.x == 3)
            {
                orderedBlocks.Add(block);
            }
        }

        if(orderedBlocks.Count() == 0)
        {
            return;
        }

        foreach(var block in orderedBlocks)
        {
            if(block.Obstacle)
                return;
        }


        cycleMovesLeft--;
        // then order them by lowest to highest y
        orderedBlocks.OrderBy(b => b.Pos.y);

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            if(block.Pos.y == 0)
            {
                print("Means you have to cycle");
                var moveDestroyedBlock = new Vector2(3f, 4f);
                var moveTo = new Vector2(3f, 3f);
                var nodeMovingTo = GetNodeAtPosition(moveTo);
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                var newBlock = SpawnBlockWithNoNode(new Vector2(3f, 4f), nodeMovingTo, block.Value);
                sequence.Insert(0, newBlock.transform.DOMove(moveTo, travelTime));
                RemoveBlock(block);
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + Vector2.down);
            var movePoint = possibleNode.Pos;
            block.SetBlock(possibleNode);
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }

            audioSource.PlayOneShot(swipe, 0.2f);

        print("OrderedBlocks:" + orderedBlocks.Count());
    }

    bool GameOverCheck(Vector2 dir)
    {
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do {
                //block.SetBlock(next);
                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if(possibleNode != null)
                {
                    if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
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
        scoreText.text = score.ToString();
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {
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
