using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private int spritesPerFrame = 6;
    [SerializeField] private int loopLimit = 10;
    [SerializeField] private bool loopAnimation = false;
    [SerializeField] private bool destroyOnEnd = false;

    private int index = 0;
    private int frame = 0;
    private int loopCount = 0;
    private Image image;

    void Awake() {
        image = GetComponent<Image>();
    }

    void Update()
    {
        if (!loopAnimation && index == sprites.Length) return;
        frame++;

        if (loopCount >= loopLimit) {
            loopCount = 0;
            gameObject.SetActive(false);
        }

        if (frame < spritesPerFrame) return;
        image.sprite = sprites[index];
        frame = 0;
        index++;
        if (index >= sprites.Length) {
            if (loopAnimation) {
                index = 0;
                loopCount++;
            }
            if (destroyOnEnd) Destroy(gameObject);
        }
    }
}
