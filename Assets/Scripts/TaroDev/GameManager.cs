using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;


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

    [SerializeField] private GameObject winScreen, loseScreen;

    private List<Node> nodes;
    private List<Block> blocks;
    private GameState state;
    private int round;

    // Get a Blocktype by the int value you pass
    // It takes from the list of types that already has a color attached to the value
    private BlockType GetBlockTypeByValue(int value) => types.First(t=>t.Value == value);

    void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    void Update()
    {

        if(state != GameState.WaitingInput) return;

        if(Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);

        if(Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);

        if(Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);

        if(Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }

    private void ChangeState(GameState newState)
    {
        state = newState;

        switch(newState)
        {
            case GameState.GenerateLevel:
                print("Generating...");
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                print("Spawning..");
                SpawnBlocks(round++ == 0 ? 16 : 1);
                break;
            case GameState.WaitingInput:
                print("WaitingInput...");
                break;
            case GameState.Moving:
                print("Moving...");
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

        // if(freeNodes.Count() == 1)
        // {
        //     print("Free Nodes:" + freeNodes.Count());

        //     foreach(var block in blocks)
        //     {
        //         print("IN HERE STILL");
        //         var next = block.Node;

        //         var possibleLeftNode = GetNodeAtPosition(next.Pos + Vector2.left);
        //         var possibleRightNode = GetNodeAtPosition(next.Pos + Vector2.right);
        //         var possibleUpNode = GetNodeAtPosition(next.Pos + Vector2.up);
        //         var possibleDownNode = GetNodeAtPosition(next.Pos + Vector2.down);

        //         var notNull = (possibleLeftNode != null && possibleRightNode  != null && possibleUpNode != null && possibleDownNode  != null) ? true : false;
        //         print(block.Pos);
        //         print(block.Value);

        //         if(notNull)
        //         {
        //             print("Left Occupied Blocks's Value:" + possibleLeftNode.OccupiedBlock.Value);
        //             print("Right Occupied Blocks's Value:" + possibleRightNode.OccupiedBlock.Value);
        //             print("Up Occupied Blocks's Value:" + possibleUpNode.OccupiedBlock.Value);
        //             print("Down Occupied Blocks's Value:" + possibleDownNode.OccupiedBlock.Value);

        //             if(possibleLeftNode.OccupiedBlock.Value == block.Value)
        //                 print("MATCH IN L");
        //                 //ChangeState(GameState.WaitingInput);
        //                 return;
        //             if(possibleRightNode.OccupiedBlock.Value == block.Value)
        //                 print("MATCH IN R");
        //                 //ChangeState(GameState.WaitingInput);
        //                 return;
        //             if(possibleUpNode.OccupiedBlock.Value == block.Value)
        //                 print("MATCH IN U");
        //                 //ChangeState(GameState.WaitingInput);
        //                 return;
        //             if(possibleDownNode.OccupiedBlock.Value == block.Value)
        //                 print("MATCH IN D");
        //                 //ChangeState(GameState.WaitingInput);
        //                 return;
        //             print("Did not go into any ifs");
        //         }
        //     }

        //     print("Out of the foreach");
        //     ChangeState(GameState.Lose);
        //     return;
        // }

        if (freeNodes.Count() == 1) {
            ChangeState(GameState.Lose);
            return;
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
                    }

                    // Otherwise can we move to this spot?
                    else if(possibleNode.OccupiedBlock == null) next = possibleNode;

                    // None hit? End do while loop
                }
            } while (next != block.Node);
               
        }

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

void MergeBlocks(Block baseBlock, Block mergingBlock)
{
    SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
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
