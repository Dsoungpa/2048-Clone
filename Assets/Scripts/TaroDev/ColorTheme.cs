using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
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

    // [Header("Theme Art")]
    // [SerializeField] private GameObject[] themeArtPrefabs;

    [Header("Block Colors")]
    public Dictionary<int, Color> colorRange;
    [SerializeField] public Color[] colorValueDisplay; // was private
    [SerializeField] public int[] colorKeyDisplay;
    [SerializeField] private float colorStep;
    [SerializeField] private int numPerColor;
    [Range(0f, 01)] [SerializeField] private float colorVariance; // 1 = no variance
    [Range(0f, 01)] [SerializeField] private float colorShift;
    private Color startColor;
    [SerializeField] int[] intValues = {2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768};

    [SerializeField] public string[] colorHexes = new string[4];
    // Need to optimize this
    private string springHexes = "#59d980 #60edc0 #83c958 #c1ee23 #e1e40d #f8b82e #eda86a #f69fc2 #c166cc #6397d7 #c2db7b #68ab66 #52afa1 #296b8d #3651ab";
    private string summerHexes = "#237762 #41b48a #60bec4 #6bafe9 #7e76f0 #ffd556 #ffae5e #ef9bc0 #e371e2 #a169cc #7fcdd1 #58b1e9 #56b9a0 #529672 #80a356";
    private string fallHexes = "#4b6522 #397b2e #9fbf20 #c5be0c #e1b815 #dc812f #d56044 #cc7c7c #ce7cc2 #cea7f9 #92ab7a #a5a66b #69a687 #4897a3 #5374ab";
    private string winterHexes = "#55a5c2 #2e698b #a38cc9 #da9ce7 #e57695 #3c9eca #598cdf #60ccb6 #71e171 #4da43e #7dc8e7 #459177 #5e863d #c1bd4f #c39650";

    [Header("Background Pattern")]
    [SerializeField] private Transform iconsParent;
    [SerializeField] private GameObject[] backgroundIcons;
    [SerializeField] private GameObject placeholder;
    [SerializeField] private int columnLength, rowLength;
    [SerializeField] private float x_Start, y_Start, x_Space, y_Space;
    private List<GameObject> instantiatedIcons = new List<GameObject>();

    [Header("Devs")]
    [SerializeField] private GameObject[] devs;

    [Header("General")]
    [SerializeField] private TMP_Dropdown themeOptions;
    [SerializeField] private GameManager GMScript;
    private bool initialsetTheme = true;

    public SpriteRenderer gameBoard;
    [HideInInspector] public List<Node> nodes;
    [HideInInspector] public List<Block> blocks;
    [HideInInspector] public bool boardReady = false;

    private ColorArray currentTheme;
    private List<string> themeOptionNames = new List<string>();
    private int prefThemeValue;

    void Awake() {
        colorHexes[0] = springHexes;
        colorHexes[1] = summerHexes;
        colorHexes[2] = fallHexes;
        colorHexes[3] = winterHexes;

        prefThemeValue = PlayerPrefs.GetInt("SelectedTheme", Random.Range(0, colorThemes.Length)); //random default theme
        currentTheme = colorThemes[prefThemeValue];
        TextToColor(prefThemeValue);

        SpawnBackgroundIcons(prefThemeValue);
        ActivateDev(prefThemeValue);
    }

    void Start()
    {
        StartCoroutine(SetBoard(currentTheme, prefThemeValue));
        SetDropdownOptions(prefThemeValue);
    }

    void SpawnBackgroundIcons(int iconIndex) {
        Boolean shift = true;
        for (int i = 0; i < columnLength * rowLength; i++) {
            Vector3 iconPos = new Vector3(x_Start + (x_Space * (i % columnLength)), y_Start + (y_Space * (i / columnLength)));
            GameObject icon = Instantiate(placeholder, iconPos, Quaternion.identity);
            instantiatedIcons.Add(icon);
            icon.transform.SetParent(iconsParent, true);

            if (shift) {
                icon.transform.position += new Vector3(0f, .75f, 0f);
                shift = false;
            }else {
                shift = true;
            }
        }
    }

    void ActivateDev(int devIndex) {
        if (PlayerPrefs.GetInt("phase", 0) < 5) {
            devs[devIndex].SetActive(true);
        }
    }

    void UpdateActiveDevs(int devIndex) {
        if (PlayerPrefs.GetInt("Phase", 0) < 5) {
            foreach(GameObject dev in devs) {
                if (dev.activeInHierarchy) {
                    dev.SetActive(false);
                }
            }
            ActivateDev(devIndex);
        }
    }

    void UpdateBackgrounIcons(int iconIndex) {
        Sprite currentIcon = backgroundIcons[iconIndex].GetComponent<SpriteRenderer>().sprite;
        foreach (GameObject icon in instantiatedIcons) {
            icon.GetComponent<SpriteRenderer>().sprite = currentIcon;
        }
    }

    void TextToColor(int themeValue) {
        //String[] lines = File.ReadAllLines(files[themeValue]);
        String[] lines = colorHexes[themeValue].Split(' ');
        int index = 0;
        int valueTracker = 0;
        colorRange = new Dictionary<int, Color>();
        for(int i = 0; i < lines.Length; i++) {
            if (!string.IsNullOrEmpty(lines[i])) {
                Color color;
                if (ColorUtility.TryParseHtmlString(lines[i], out color)) {
                    valueTracker = intValues[index++];
                    colorRange.Add(valueTracker, color);
                }else {
                    Debug.LogError("Invalid Color String: " + lines);
                }
            }
        }

        int count = 0;
        foreach(KeyValuePair<int, Color> pair in colorRange) {
            colorKeyDisplay[count] = pair.Key;
            colorValueDisplay[count++] = pair.Value;
        }
    }

    void CreateBlockColors() {
        int startKey = 1;
        int colorIndex; 
        int previousStart;
        colorRange = new Dictionary<int, Color>();
        for (int i = 0; i < currentTheme.colors.Length; i++) {
            previousStart = startKey;
            startKey = (int)Mathf.Pow(2, (i + 1) * numPerColor);
            colorIndex = (i + 1) % currentTheme.colors.Length;
            AddDictionary(colorRange, CreateDict(currentTheme.colors[i], currentTheme.colors[colorIndex], startKey, numPerColor, previousStart));
        }

        int count = 0;
        foreach(KeyValuePair<int, Color> pair in colorRange) {
            colorKeyDisplay[count] = pair.Key;
            colorValueDisplay[count++] = pair.Value;
        }
    }

    private Dictionary<int, Color> CreateDict(Color baseColor, Color targetColor, int startKey, int dictSize, int endKey = 2) {
        Dictionary<int, Color> colorDict = new Dictionary<int, Color>();

        int dictKey = startKey;
        int stepMultiplier = 1;
        int count = 0;
        for (int i = dictKey; i > endKey; i /= 2) {
            float t = (float)count / 5;
            Color newColor = Color.Lerp(baseColor, targetColor, t);
            newColor.a = 1f;
            colorDict[i] = newColor;
            stepMultiplier++;
            count++;
        }
        return colorDict;
    }

    void AddDictionary(Dictionary<int, Color> mainDict, Dictionary<int, Color> dictToAdd) {
        foreach(var pair in dictToAdd) {
            mainDict.Add(pair.Key, pair.Value);
        }
    }

    private IEnumerator SetBoard(ColorArray currentTheme, int themeIndex) {
        yield return new WaitUntil(() => boardReady == true);
        gameBoard = GMScript.GetBoard();
        nodes = GMScript.GetNodes();
        blocks = GMScript.GetBlocks();
        ChangeTheme(currentTheme, themeIndex);
    }

    // public void ToggleThemeArt(int themeIndex) {
    //     for (int i = 0; i < themeArtPrefabs.Length; i++) {
    //         if (i == themeIndex) {
    //             themeArtPrefabs[i].SetActive(true);
    //         }else {
    //             themeArtPrefabs[i].SetActive(false);
    //         }
    //     }
    // }

    public void OldUpdateThemeFromDropdown() {
        ColorArray dropdownTheme = colorThemes[themeOptions.value];
        StartCoroutine(DelayedChangeTheme(dropdownTheme, themeOptions.value));
    }

    private IEnumerator DelayedChangeTheme(ColorArray dropdownTheme, int colorThemeIndex) {
        yield return new WaitUntil(() => boardReady == true);
        ChangeTheme(dropdownTheme, colorThemeIndex);
        GMScript.SetBlockColors();
    }

    void SetDropdownOptions(int currentOption) {
        foreach(ColorArray theme in colorThemes) {
            themeOptionNames.Add(theme.themeName.ToUpper()); //upper case options
        }
        themeOptions.AddOptions(themeOptionNames);
        themeOptions.value = currentOption;
    }

    public void ChangeTheme(ColorArray theme, int themeIndex) {
        TextToColor(themeIndex);
        UpdateBackgrounIcons(themeIndex); // background icons art
        UpdateActiveDevs(themeIndex); // dev art

        PlayerPrefs.SetInt("SelectedTheme", themeIndex);
        currentTheme = theme;

        foreach (Image image in primaryColors) { image.color = theme.colors[0]; }
        foreach (Image image in secondaryColors) { image.color = theme.colors[1]; }
        foreach (Image image in tertiaryColors) { image.color = theme.colors[2]; }

        foreach (Image button in buttons) { button.color = theme.colors[0]; }
        foreach (Node node in nodes) { node.visualRenderer.color = ShiftColor(theme.colors[2]); }
        
        cam.backgroundColor = theme.colors[2];
        gameBoard.color = theme.colors[3];

        // ToggleThemeArt(themeIndex);
    }

    public Color ShiftColor(Color color) {
        float r = Mathf.Clamp01(color.r * colorShift);
        float g = Mathf.Clamp01(color.g * colorShift);
        float b = Mathf.Clamp01(color.b * colorShift);

        return new Color(r, g, b);
    }
}