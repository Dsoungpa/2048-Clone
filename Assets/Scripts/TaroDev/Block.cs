using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int Value;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private TextMeshPro text;

    public void Init(BlockType type)
    {
        Value = type.Value;
        renderer.color = type.Color;
        text.text = type.Value.ToString();
    }
}
