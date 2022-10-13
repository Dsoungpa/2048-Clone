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
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private int winCondition = 2048;
    [SerializeField] private bool gameOver;
    [SerializeField] private int score = 0;
    [SerializeField] private int possibleHighScore;

    [SerializeField] private GameObject winScreen, loseScreen;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highscoreText;

    private List<Node> nodes;
    private List<Block> blocks;
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
                print("WaitingInput...");
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

        // For each of the free nodes get a certain amount of Nodes that you want to use
        // for spawning the blocks
        foreach (var nodes in freeNodes.Take(amount))
        {
            SpawnBlock(nodes, Random.value > 0.8f ? 4 : 2);
        }

        if (freeNodes.Count() == 1) {
            var GameOver = (GameOverCheck(Vector2.left) == false && GameOverCheck(Vector2.right) == false && GameOverCheck(Vector2.up) == false && GameOverCheck(Vector2.down) == false) ? true : false;
            print(GameOver);
            if(GameOver)
            {
                ChangeState(GameState.Lose);
                print("LOSE");
                return;
            }

            else
            {
                ChangeState(GameState.WaitingInput);
                return;
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

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        // Order the block array by x then by y if it is moving left or down
        // Or change it to ordered by y then x if it is moving right or up
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        bool blocksMoved = false;

        // Go through the ordered list of blocks
        foreach (var block in orderedBlocks)
        {
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
                foreach (var block in orderedBlocks.Where(b=>b.MergingBlock != null))
                {
                    MergeBlocks(block.MergingBlock, block);
                }

                ChangeState(GameState.SpawningBlocks);
            });
        }

        ChangeState(GameState.WaitingInput);
    }

bool GameOverCheck(Vector2 dir)
{
    var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList(); 
    if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

    foreach (var block in orderedBlocks)
    {
        var next = block.Node;
        do {
            block.SetBlock(next);
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
