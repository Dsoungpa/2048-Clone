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

    public Vector2 Pos => transform.position;
    [SerializeField] private SpriteRenderer renderer;
    public TextMeshPro text;

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
}
