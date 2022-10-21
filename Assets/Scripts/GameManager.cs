using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> blockTypes;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private int winCondition = 2048;

    [SerializeField] private GameObject winScreen, loseScreen;

    private List<Node> nodes;
    private List<Block> blocks;
    private GameState state;
    private int round;

    private BlockType GetBlockTypeByValue(int value) => blockTypes.First(t => t.Value == value);

    void Start() {
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState) {
        state = newState;

        switch (newState) {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2: 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
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

    void Update() {
        if (state != GameState.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);

    }

    void GenerateGrid() {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var gridCenter = new Vector2((float)width/2 - 0.5f, (float)height/2 - 0.5f);
        var board = Instantiate(boardPrefab, gridCenter, Quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(gridCenter.x, gridCenter.y, -10);

        ChangeState(GameState.SpawningBlocks);

    }

    void SpawnBlocks(int amount) {

        var freeNodes = nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        foreach(var node in freeNodes.Take(amount)){
            SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
        }

        if (freeNodes.Count() == 1) {
            ChangeState(GameState.Lose);
            return;
        }

        ChangeState(blocks.Any(b=>b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value) {
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(value));
            block.SetBlock(node);
            blocks.Add(block);
    }

    public void ShiftSingle(GameObject button) {
        //dir tells which direction to shift by 1
        //if shiftRow = true, shift the row, else shift the column
        var testDir = Vector2.zero;
        var rowOrColumnNumber = -1;
        
        print(button.name);
        
        switch (button.name) {
            // Left ---------------------------------------------
            case "Left 1":
                testDir = Vector2.left;
                rowOrColumnNumber = 3;
                break;
            case "Left 2":
                testDir = Vector2.left;
                rowOrColumnNumber = 2;
                break;
            case "Left 3":
                testDir = Vector2.left;
                rowOrColumnNumber = 1;
                break;
            case "Left 4":
                testDir = Vector2.left;
                rowOrColumnNumber = 0;
                break;
            // Right --------------------------------------------
            case "Right 1":
                testDir = Vector2.right;
                rowOrColumnNumber = 3;
                break;
            case "Right 2":
                testDir = Vector2.right;
                rowOrColumnNumber = 2;
                break;
            case "Right 3":
                testDir = Vector2.right;
                rowOrColumnNumber = 1;
                break;
            case "Right 4":
                testDir = Vector2.right;
                rowOrColumnNumber = 0;
                break;
            // Down ---------------------------------------------
            case "Down 1":
                testDir = Vector2.down;
                rowOrColumnNumber = 0;
                break;
            case "Down 2":
                testDir = Vector2.down;
                rowOrColumnNumber = 1;
                break;
            case "Down 3":
                testDir = Vector2.down;
                rowOrColumnNumber = 2;
                break;
            case "Down 4":
                testDir = Vector2.down;
                rowOrColumnNumber = 3;
                break;
            // Up -----------------------------------------------
            case "Up 1":
                testDir = Vector2.up;
                rowOrColumnNumber = 0;
                break;
            case "Up 2":
                testDir = Vector2.up;
                rowOrColumnNumber = 1;
                break;
            case "Up 3":
                testDir = Vector2.up;
                rowOrColumnNumber = 2;
                break;
            case "Up 4":
                testDir = Vector2.up;
                rowOrColumnNumber = 3;
                break;
        }

        if (testDir == Vector2.left) {
            var rowBlocks = blocks.Where(n => n.Pos.y == rowOrColumnNumber); //get all blocks in the specified row/column
            var tempSequence = DOTween.Sequence();

            foreach (var block in rowBlocks) {
                if (block.Pos.x == 0) {
                    //Mathf.Abs(rowOrColumnNumber - 3)
                    var targetNode = GetNodeAtPosition(new Vector2(3, rowOrColumnNumber));
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(new Vector2(3, rowOrColumnNumber)));
                }else {
                    var targetNode = GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir);
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir));
                }
            }
        }else if (testDir == Vector2.right) {
            var rowBlocks = blocks.Where(n => n.Pos.y == rowOrColumnNumber); //get all blocks in the specified row/column
            var tempSequence = DOTween.Sequence();

            foreach (var block in rowBlocks) {
                if (block.Pos.x == 3) {
                    //Mathf.Abs(rowOrColumnNumber - 3)
                    var targetNode = GetNodeAtPosition(new Vector2(0, rowOrColumnNumber));
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(new Vector2(0, rowOrColumnNumber)));
                }else {
                    var targetNode = GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir);
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir));
                }
            }
        }else if (testDir == Vector2.down) {
            var rowBlocks = blocks.Where(n => n.Pos.x == rowOrColumnNumber); //get all blocks in the specified row/column
            var tempSequence = DOTween.Sequence();

            foreach (var block in rowBlocks) {
                if (block.Pos.y == 0) {
                    var targetNode = GetNodeAtPosition(new Vector2(rowOrColumnNumber, 3));
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(new Vector2(rowOrColumnNumber, 3)));
                }else {
                    var targetNode = GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir);
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir));
                }
            }
        }else if (testDir == Vector2.up) {
            var rowBlocks = blocks.Where(n => n.Pos.x == rowOrColumnNumber); //get all blocks in the specified row/column
            var tempSequence = DOTween.Sequence();

            foreach (var block in rowBlocks) {
                if (block.Pos.y == 3) {
                    //Mathf.Abs(rowOrColumnNumber - 3)
                    var targetNode = GetNodeAtPosition(new Vector2(rowOrColumnNumber, 0));
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(new Vector2(rowOrColumnNumber, 0)));
                }else {
                    var targetNode = GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir);
                    tempSequence.Insert(0, block.transform.DOMove(targetNode.Pos, travelTime));
                    block.SetBlock(GetNodeAtPosition(GetNodeAtPosition(block.Pos).Pos + testDir));
                }
            }
        }
    }

    void Shift(Vector2 dir) {
        ChangeState(GameState.Moving);

        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y);
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks) {
            var next = block.Node;
            do {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                if (possibleNode != null) {
                    //node is present

                    //if its possible to merge, set merge
                    if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value)) {
                        block.MergeBlock(possibleNode.OccupiedBlock);
                    // otherise, can we move to this spot?
                    }else if(possibleNode.OccupiedBlock == null) next = possibleNode; //next node is available
                }
            } while (next != block.Node);

        }

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks) {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));

        }

        sequence.OnComplete(() => {
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null)) {
                MergeBlocks(block.MergingBlock, block);
            }

            ChangeState(GameState.SpawningBlocks);
        });
    }

    void MergeBlocks(Block baseBlock, Block mergingBlock) {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block) {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Node GetNodeAtPosition(Vector2 pos) {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

}

[Serializable]
public struct BlockType {
    public int Value;
    public Color Color;
}

public enum GameState {
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
