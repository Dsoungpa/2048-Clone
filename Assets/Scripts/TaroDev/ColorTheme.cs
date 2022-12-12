using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorTheme : MonoBehaviour
{
    // Index 0 = primary
    // Index 1 = secondary
    // Index 2 = background
    // Index 3 = text primary
    // Index 4 = text secondary

    [Header("Color Themes")]
    [SerializeField] private Color[] theme1 = new Color[3]; // spring
    [SerializeField] private Color[] theme2 = new Color[3]; // summer
    [SerializeField] private Color[] theme3 = new Color[3]; // fall
    [SerializeField] private Color[] theme4 = new Color[3]; // winter
    //[SerializeField] private Color[] theme5 = new Color[3]; // christmas
    [SerializeField] private Color[][] allThemes = new Color[4][];

    [Header("UI Separations")]
    [SerializeField] private Camera cam;
    [SerializeField] private Image[] buttons = new Image[16];

    // Index 0 = score
    // Index 1 = best
    // Index 2 = cycles left
    [SerializeField] private Image[] textBoards = new Image[3];
    [SerializeField] private Image newGameButton;
    [SerializeField] private Image toggleCycleButton;
    [SerializeField] private Image leaderboard;
    [SerializeField] private Image audioButton;
    [SerializeField] private Image themeDropdown;
    [SerializeField] private TMP_Dropdown themeOptions;


    [Header("Theme Art")]
    [SerializeField] private GameObject[] themeArtPrefabs = new GameObject[4];

    [HideInInspector] public SpriteRenderer gameBoard;
    public Color[] initialTheme;
    [SerializeField] private GameManager GMScript;
    public bool boardReady = false;
    private TextMeshPro text1;

    // Start is called before the first frame update
    void Start()
    {
        allThemes[0] = theme1;
        allThemes[1] = theme2;
        allThemes[2] = theme3;
        allThemes[3] = theme4;
        //allThemes[4] = theme5;

        // int randomNum = Random.Range(0, allThemes.Length);
        //initialTheme = allThemes[randomNum];

        initialTheme = allThemes[0];

        StartCoroutine(SetBoard(initialTheme, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) ChangeTheme(allThemes[0], 0);
        if (Input.GetKeyDown(KeyCode.X)) ChangeTheme(allThemes[1], 1);
        if (Input.GetKeyDown(KeyCode.C)) ChangeTheme(allThemes[2], 2);
        if (Input.GetKeyDown(KeyCode.V)) ChangeTheme(allThemes[3], 3);
        //if (Input.GetKeyDown(KeyCode.B)) ChangeTheme(allThemes[4], 4);
    }

    private IEnumerator SetBoard(Color[] initialTheme, int themeIndex) {
        yield return new WaitUntil(() => boardReady == true);
        gameBoard = GMScript.GetBoard();
        ChangeTheme(initialTheme, themeIndex);
    }

    public void ToggleThemeArt(int themeIndex) {
        for (int i = 0; i < themeArtPrefabs.Length; i++) {
            if (i == themeIndex) {
                themeArtPrefabs[i].SetActive(true);
            }else {
                themeArtPrefabs[i].SetActive(false);
            }
        }
    }

    public void UpdateThemeFromDropdown() {
        Color[] theme = allThemes[themeOptions.value];
        ChangeTheme(theme, themeOptions.value);
    }

    public void ChangeTheme(Color[] theme, int themeIndex) {
        ToggleThemeArt(themeIndex);
        cam.backgroundColor = theme[2];
        newGameButton.color = theme[0];
        toggleCycleButton.color = theme[0];
        foreach (var button in buttons) { button.color = theme[0]; }
        foreach (var board in textBoards) { board.color = theme[1]; }
        gameBoard.color = theme[1];
        leaderboard.color = theme[0];
        audioButton.color = theme[1];
        themeDropdown.color = theme[2];
    }
}
