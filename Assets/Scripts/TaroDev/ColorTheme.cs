using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ColorArray
{
    public string themeName;
    public Color[] colors;
}

public class ColorTheme : MonoBehaviour
{
    [Header("Themes")]
    [SerializeField] private ColorArray[] colorThemes;

    [Header("Color Separations")]
    [SerializeField] private Image[] primaryColors;
    [SerializeField] private Image[] secondaryColors;
    [SerializeField] private Image[] tertiaryColors;
    [SerializeField] private Image[] buttons;
    [SerializeField] private Camera cam;

    [Header("Theme Art")]
    [SerializeField] private GameObject[] themeArtPrefabs;

    [Header("General")]
    [SerializeField] private TMP_Dropdown themeOptions;
    [SerializeField] private GameManager GMScript;

    [HideInInspector] public SpriteRenderer gameBoard;
    [HideInInspector] public bool boardReady = false;

    private ColorArray initialTheme;
    private List<string> themeOptionNames = new List<string>();

    void Start()
    {
        int prefThemeValue = PlayerPrefs.GetInt("SelectedTheme", 0);
        initialTheme = colorThemes[prefThemeValue];
        StartCoroutine(SetBoard(initialTheme, 0));

        SetDropdownOptions(prefThemeValue);
    }

    void Update()
    {

    }

    private IEnumerator SetBoard(ColorArray initialTheme, int themeIndex) {
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
        ColorArray dropdownTheme = colorThemes[themeOptions.value];
        ChangeTheme(dropdownTheme, themeOptions.value);
    }

    void SetDropdownOptions(int currentOption) {
        foreach(ColorArray theme in colorThemes) {
            themeOptionNames.Add(theme.themeName);
        }
        themeOptions.AddOptions(themeOptionNames);
        themeOptions.value = currentOption;
    }

    public void ChangeTheme(ColorArray theme, int themeIndex) {
        print("Test");
        PlayerPrefs.SetInt("SelectedTheme", themeIndex);

        foreach (Image image in primaryColors) { image.color = theme.colors[0]; }
        foreach (Image image in secondaryColors) { image.color = theme.colors[1]; }
        foreach (Image image in tertiaryColors) { image.color = theme.colors[2]; }
        foreach (Image button in buttons) { button.color = theme.colors[0]; }
        
        cam.backgroundColor = theme.colors[2];

        ToggleThemeArt(themeIndex);
    }
}