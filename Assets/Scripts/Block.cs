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

    public Vector2 Pos => transform.position;
    [SerializeField] private SpriteRenderer rend;
    [SerializeField] private TextMeshPro valueText;

    public void Init(BlockType type) {
        Value = type.Value;
        rend.color = type.Color;
        valueText.text = type.Value.ToString();
    }

    public void SetBlock(Node node) {
        if (Node != null) Node.OccupiedBlock = null;
        Node = node;
        Node.OccupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith) {
        //set the block we are merging with
        MergingBlock = blockToMergeWith;

        //set current node as unoccupied
        Node.OccupiedBlock = null;

        //set the base block as Merging, so it does not get used twice
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;
}
