using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorTheme : MonoBehaviour
{
    [Header("Color Themes")]
    // Index 0 = primary
    // Index 1 = secondary
    // Index 2 = background
    // Index 3 = text

    [SerializeField] private Color[] theme1 = new Color[4];
    [SerializeField] private Color[] theme2 = new Color[4];
    [SerializeField] private Color[] theme3 = new Color[4];
    [SerializeField] private Color[][] allThemes = new Color[3][];

    [Header("UI Separations")]
    [SerializeField] private Camera cam;
    [SerializeField] private Image[] buttons = new Image[16];
    // Index 0 = score
    // Index 1 = best
    // Index 2 = cycles left
    [SerializeField] private Image[] textBoards = new Image[3];
    [SerializeField] private Image newGameButton;
    [SerializeField] private Image toggleCycleButton;

    [HideInInspector] public SpriteRenderer gameBoard;
    public Color[] initialTheme;
    [SerializeField] private GameManager GMScript;
    public bool boardReady = false;

    // Start is called before the first frame update
    void Start()
    {
        allThemes[0] = theme1;
        allThemes[1] = theme2;
        allThemes[2] = theme3;

        initialTheme = allThemes[Random.Range(0, allThemes.Length)];

        StartCoroutine(SetBoard(initialTheme));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) ChangeTheme(allThemes[0]);
        if (Input.GetKeyDown(KeyCode.X)) ChangeTheme(allThemes[1]);
        if (Input.GetKeyDown(KeyCode.C)) ChangeTheme(allThemes[2]);
    }

    private IEnumerator SetBoard(Color[] initialTheme) {
        yield return new WaitUntil(() => boardReady == true);
        gameBoard = GMScript.GetBoard();
        ChangeTheme(initialTheme);
    }

    public void ChangeTheme(Color[] theme) {
        cam.backgroundColor = theme[2];
        newGameButton.color = theme[0];
        toggleCycleButton.color = theme[0];
        foreach (var button in buttons) { button.color = theme[0]; }
        foreach (var board in textBoards) { board.color = theme[1]; }
        gameBoard.color = theme[1];
    }
}
