using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{
    /* ~~~COMMENT THINGS LIKE THIS~~~
    IEnumerator SubmitScore(int score){
        yield return leaderboard.SubmitScoreRoutine(score);
        yield return leaderboard.FetchTopHighScoresRoutine();
    }
    */

    #region VARIABLES

    #region  GENERAL VARIABLES
    [Header("General")]
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private int winCondition = 2048;
    [SerializeField] private int score = 0;
    [SerializeField] private int movementtracker;
    [SerializeField] private int possibleHighScore;
    [SerializeField] private int cycleMovesLeft = 5;
    [SerializeField] private float cycleDelay = 0.25f;
    [SerializeField] private int countUntilObstacle = 5;
    [SerializeField] private int obstacleCount;
    [SerializeField] private int obstacleLimit;
    [SerializeField] private float obstacleSpawnChance = 0.1f;

    public static GameManager instance;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Block obstaclePrefab;
    [SerializeField] private Transform boardParent;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;

    // Booleans
    [SerializeField] private bool gameOver;
    [SerializeField] private bool cycling = false;
    [SerializeField] public bool inCycle = false;

    public bool cyclesMode;
    public Block clickedBlock;
    private float cycleTimer = 0;

    [Header("Brick Values")]
    [SerializeField] private int[] brickValues = new int[0];
    [SerializeField] private int[] weightedBrickValues;
    [SerializeField] private int currentHighestValue;
    #endregion

    #region UI VARIABLES
    [Header("UI")] [Space(10)]
    [Header("UI Main")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject worldLoseScreen;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameOverScore;
    [SerializeField] private TMP_Text highscoreText;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private TMP_Text cycleMoves;
    [SerializeField] private TMP_Text leaderboardHighScore;
    [SerializeField] private GameObject cycleUI;
    [SerializeField] private GameObject noCycleIndicator;
    [SerializeField] private GameObject newHighScoreObject;

    [Header("UI Misc")]
    [SerializeField] private GameObject backgroundDarken;
    [SerializeField] private GameObject backgroundDarken2;
    [SerializeField] private GameObject cyclesDarken;
    [SerializeField] private GameObject[] panelDarken;
    [SerializeField] private GameObject mask1;
    [SerializeField] private GameObject mask2;
    [SerializeField] private GameObject mask3;

    [Header("UI Settings")]
    [SerializeField] private bool settingsActive = true;
    [SerializeField] private GameObject audioOnIcon;
    [SerializeField] private GameObject audioOffIcon;
    [SerializeField] private GameObject audioSetting;
    [SerializeField] private GameObject tutorialSetting;
    [SerializeField] private GameObject usernameSetting;
    [SerializeField] private GameObject audioSettingInfo;
    [SerializeField] private GameObject tutorialSettingInfo;
    [SerializeField] private GameObject usernameSettingInfo;
    [SerializeField] private GameObject updateUsername;
    [SerializeField] private float offsetDuration;
    [SerializeField] public bool disableControl = false;

    [Header("Tutorial")]
    [SerializeField] private GameObject skipTutorial;
    [SerializeField] private GameObject devsIcons;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField updateusername;
    [SerializeField] private int postTutorialMoveLimit;
    [SerializeField] public int phase;
    [SerializeField] private GameObject phase0, phase1, phase2, phase3, phase4, phase5, phase6;
    [SerializeField] private int tutorialRestartPhase;
    public int postTutorialMoveCounter = 0;

    [Header("High Score Alert")]
    [SerializeField] private float newHighScoreDuration = 2f;
    [SerializeField] private Vector3 newHighScorePos;
    [SerializeField] private Vector3 startScale;
    [SerializeField] private Vector3 endScale;
    [SerializeField] private float scaleTime;
    [SerializeField] private bool gotNewHighScore = false;
    #endregion

    #region OTHER VARIALBES
    [Header("Audio")]
    [SerializeField] private List<AudioClip> merges = new List<AudioClip>();
    [SerializeField] private AudioClip blockBreak;
    [SerializeField] private AudioClip buttonPress;
    [SerializeField] private AudioClip noCycleError;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool muted = false;
    [SerializeField] private AudioListener audioListener;

    [Header("Mobile")]
    private Vector3 startTouchPosition;
    private Vector3 currentPosition;
    private Vector3 endTouchPosition;
    [SerializeField] private bool stopTouch = false;
    [SerializeField] private float swipeRange;
    private bool hasSwiped;
    private bool hasSwipedCycle;

    [Header("Script Reference")]
    [SerializeField] private ColorTheme colorThemeScript;
    [SerializeField] public Leaderboard leaderboard;
    private SpriteRenderer gameBoard;

    // Node and Block management lists
    private List<Node> nodes;
    private List<Block> blocks;
    private List<Block> obstacles;
    private GameState state;
    private int round;
    #endregion

    #endregion

    // Get a Blocktype by the int value you pass
    // It takes from the list of types that already has a color attached to the value
    private BlockType GetBlockTypeByValue(int value) => types.First(t=>t.Value == value);

    #region STARTUP METHODS
    void Awake() {
        instance = this;

        if (PlayerPrefs.GetInt("phase", -1) == -1) {
            phase = -1;
        }else if (PlayerPrefs.GetInt("phase", - 1) > 5) {
            phase = 7;
        }else {
            phase = 0;
        }

        InitializeAudioState();
    }

    void Start() {
        highscoreText.text = PlayerPrefs.HasKey("myHighScore") ? PlayerPrefs.GetInt("myHighScore").ToString() : "0";
        ChangeState(GameState.GenerateLevel);
        weightedBrickValues = brickValues;
        currentHighestValue = brickValues[brickValues.Length - 1];
        cyclesMode = false;
    }
    #endregion
   
    #region THEME UPDATE METHODS
    void UpdateBlockColors() {
        for (int i = 0; i < types.Count(); i++) {
            if (colorThemeScript.colorRange.ContainsKey(types[i].Value)) {
                types[i].Color = colorThemeScript.colorRange[types[i].Value];
            }
        }
    }

    void NewUpdateBlockColors() { //this is just so we can see the color in the inspector
        for (int i = 0; i < types.Count(); i++) {
            types[i].Color = colorThemeScript.colorValueDisplay[i];
        }
    }

    public void SetBlockColors() {
        NewUpdateBlockColors();
        foreach(KeyValuePair<int, Color> pair in colorThemeScript.colorRange) {
            foreach (var block in blocks) {
                if (block.Value == pair.Key && block.Obstacle != true) {
                    block.renderer.color = pair.Value;
                    UpdateHighlightColor(block);
                }
            }
        }
    }

    public void SetObstacleHighlightColor() {
        foreach (var obstacle in obstacles) {
            UpdateHighlightColor(obstacle);
        }
    }

    void UpdateHighlightColor(Block block) {
        int themeIndex = colorThemeScript.currentThemeIndex;
        block.gameObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = colorThemeScript.highlightColors[themeIndex];
    }

    public void UpdateHighScoreColor() {
        GameObject highScoreInstance = GameObject.Find("NewHighScore");
        if(highScoreInstance != null) {
            Color newColor = colorThemeScript.GetCurrentHighScoreColor();
            highScoreInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = newColor;
            highScoreInstance.transform.GetChild(1).GetComponent<Image>().color = newColor;
        }
    }
    #endregion

    #region CYCLE MANAGEMENT
    public void SetCycleTrue(){
        cyclesMode = true;
    }

    public void SetCycleFalse(){
        cyclesMode = false;
    }

    public void ResetClickedPosition(){
        clickedBlock = null;
    }

    public void clearClickedIndicator(){
        foreach(var block in blocks){
            if(block.clicked){
                block.clickedIndicator.SetActive(false);
                block.clicked = false;
            }
        }  
        SetCycleFalse();
        ResetClickedPosition();
    }

    IEnumerator CheckCyclesModeDelay(){
        yield return new WaitForSeconds(.1f);
        if(Input.GetMouseButtonDown(0)){
            clearClickedIndicator();
        }
    }
    #endregion

    void Update()
    {
        if (disableControl) return; // disable control while updating username;

        cycleMoves.text = cycleMovesLeft.ToString();
        possibleHighScore = score;

        if(state != GameState.WaitingInput) return;

        #region TUTORIAL
        if(phase == -1){
            devsIcons.SetActive(true);
            phase0.SetActive(true);
            backgroundDarken.SetActive(true);
            OverlayCanvasDarkenOn();
        }

        if(phase == 0){
            phase1.SetActive(true);
            phase0.SetActive(false);
            backgroundDarken.SetActive(false);
            OverlayCanvasDarkenOff();
            skipTutorial.SetActive(true);
        }

        if(phase == 2){
            phase1.SetActive(false);
            phase2.SetActive(true);
            backgroundDarken2.SetActive(true);
            OverlayCanvasDarkenOn();
            mask1.SetActive(true);
            mask3.SetActive(true);
        }

        if(phase == 3){
            phase2.SetActive(false);
            phase3.SetActive(true);
            mask3.SetActive(false);
        }

        if(phase == 4){
            phase3.SetActive(false);
            phase4.SetActive(true);
            mask1.SetActive(false);
            mask2.SetActive(true);
            cyclesDarken.SetActive(false);
        }

        if(phase == 5){
            phase4.SetActive(false);
            backgroundDarken2.SetActive(false);
            OverlayCanvasDarkenOff();
            mask2.SetActive(false);
            phase5.SetActive(true);
        }

        if(phase == 6){
            phase5.SetActive(false);
            if (postTutorialMoveCounter < postTutorialMoveLimit) phase6.SetActive(true);
            skipTutorial.SetActive(false);
        }

        void OverlayCanvasDarkenOn() {
            foreach(var panel in panelDarken) {
                panel.SetActive(true);
            }
        }

        void OverlayCanvasDarkenOff() {
            foreach(var panel in panelDarken) {
                panel.SetActive(false);
            }
        }
        #endregion

        #region INPUT

            #region KEYBOARD INPUT
        // Keyboard Input
        if(!cyclesMode){
            if (phase != -1) {
                if(phase != 0){
                    if( phase != 3 && phase != 4 ){
                        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)){
                            PostTutorialMoveLimiter();
                            Shift(Vector2.left);
                            return;
                        }
                        
                    }

                    if(phase != 1){
                        if(phase != 3){
                            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)){
                                PostTutorialMoveLimiter();
                                Shift(Vector2.up);
                                return;
                            }
                        }

                        if(phase != 4){
                            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                            {
                                PostTutorialMoveLimiter();
                                Shift(Vector2.down);
                                return;
                            }
                        }
                        
                    }
                
                }
            
                if(phase != 1){
                    if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)){
                        PostTutorialMoveLimiter();
                        Shift(Vector2.right);
                        return;
                    } 
                }
            }       
        }

        // Cycle Instead of Shift
        
        if(cyclesMode){
             
            //StartCoroutine(CheckCyclesModeDelay());

            if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) 
            {
                PostTutorialMoveLimiter();
                print(clickedBlock.Pos.y);
                if(clickedBlock.Pos.y == 0){
                    Cycle("FourthRowLeft");
                }

                else if(clickedBlock.Pos.y == 1){
                    Cycle("ThirdRowLeft");
                }

                else if(clickedBlock.Pos.y == 2){
                    Cycle("SecRowLeft");
                }

                else if(clickedBlock.Pos.y == 3){
                    Cycle("FirstRowLeft");
                }
            }

            if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) 
            {
                PostTutorialMoveLimiter();
                //print(clickedBlock.Pos.y);
                if(clickedBlock.Pos.y == 0){
                    Cycle("FourthRowRight");
                }

                else if(clickedBlock.Pos.y == 1){
                    Cycle("ThirdRowRight");
                }

                else if(clickedBlock.Pos.y == 2){
                    Cycle("SecRowRight");
                }

                else if(clickedBlock.Pos.y == 3){
                    Cycle("FirstRowRight");
                }
            }

            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                PostTutorialMoveLimiter();
                //print(clickedBlock.Pos.x);
                if(clickedBlock.Pos.x == 0){
                    Cycle("FirstColUp");
                }

                else if(clickedBlock.Pos.x == 1){
                    Cycle("SecColUp");
                }

                else if(clickedBlock.Pos.x == 2){
                    Cycle("ThirdColUp");
                }

                else if(clickedBlock.Pos.x == 3){
                    Cycle("FourthColUp");
                }
            }

            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                PostTutorialMoveLimiter();
                //print(clickedBlock.Pos.x);
                if(clickedBlock.Pos.x == 0){
                    Cycle("FirstColDown");
                }

                else if(clickedBlock.Pos.x == 1){
                    Cycle("SecColDown");
                }

                else if(clickedBlock.Pos.x == 2){
                    Cycle("ThirdColDown");
                }

                else if(clickedBlock.Pos.x == 3){
                    Cycle("FourthColDown");
                }
            }
        }
        
        if(cyclesMode){
            //StartCoroutine(CheckCyclesModeDelay());

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && inCycle){
                debugText.text = "TOUCH CYCLE";
                movementtracker = 0;
                print("began touch in cycles");
                startTouchPosition = Input.GetTouch(0).position;
                //StartCoroutine(WaitingToClear()); // sets inCycle to True
                hasSwipedCycle = false;
            }

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved && movementtracker == 0){
                debugText.text = "SWIPE CYCLE";

                PostTutorialMoveLimiter();
                print("Moved Finger In Cycle");
                currentPosition = Input.GetTouch(0).position;
                Vector3 Distance = currentPosition - startTouchPosition;
                print("X" + Distance.x);
                print("Y" + Distance.y);

                if(Mathf.Abs(Distance.x) > Mathf.Abs(Distance.y)){
                    Distance.y = 0;
                }
                
                else{
                    Distance.x = 0;
                }

                if(Distance.x < -swipeRange) 
                {     
                    print(clickedBlock.Pos.y);
                    if(clickedBlock.Pos.y == 0){
                        movementtracker++;
                        Cycle("FourthRowLeft");
                    }

                    else if(clickedBlock.Pos.y == 1){
                        movementtracker++;
                        Cycle("ThirdRowLeft");
                    }

                    else if(clickedBlock.Pos.y == 2){
                        movementtracker++;
                        Cycle("SecRowLeft");
                    }

                    else if(clickedBlock.Pos.y == 3){
                        movementtracker++;
                        Cycle("FirstRowLeft");
                    }

                    hasSwipedCycle = true;
                }

                else if (Distance.x > swipeRange)
                {
                    print(clickedBlock.Pos.y);
                    if(clickedBlock.Pos.y == 0){
                        print("going into cycle");
                        movementtracker++;
                        Cycle("FourthRowRight");
                    }

                    else if(clickedBlock.Pos.y == 1){
                        print("going into cycle");
                        movementtracker++;
                        Cycle("ThirdRowRight");
                    }

                    else if(clickedBlock.Pos.y == 2){
                        print("going into cycle");
                        movementtracker++;
                        Cycle("SecRowRight");
                    }

                    else if(clickedBlock.Pos.y == 3){
                        print("going into cycle");
                        movementtracker++;
                        Cycle("FirstRowRight");
                    }

                    hasSwipedCycle = true;
                }

                else if(Distance.y > swipeRange)
                {
                    print(clickedBlock.Pos.x);
                    if(clickedBlock.Pos.x == 0){
                        movementtracker++;
                        Cycle("FirstColUp");
                    }

                    else if(clickedBlock.Pos.x == 1){
                        movementtracker++;
                        Cycle("SecColUp");
                    }

                    else if(clickedBlock.Pos.x == 2){
                        movementtracker++;
                        Cycle("ThirdColUp");
                    }

                    else if(clickedBlock.Pos.x == 3){
                        movementtracker++;
                        Cycle("FourthColUp");
                    }

                    hasSwipedCycle = true;
                }

                else if(Distance.y < -swipeRange)
                {
                    print(clickedBlock.Pos.x);
                    if(clickedBlock.Pos.x == 0){
                        movementtracker++;
                        Cycle("FirstColDown");
                    }

                    else if(clickedBlock.Pos.x == 1){
                        movementtracker++;
                        Cycle("SecColDown");
                    }

                    else if(clickedBlock.Pos.x == 2){
                        movementtracker++;
                        Cycle("ThirdColDown");
                    }

                    else if(clickedBlock.Pos.x == 3){
                        movementtracker++;
                        Cycle("FourthColDown");
                    }

                    hasSwipedCycle = true;
                }

                else{
                    hasSwipedCycle = false;
                }
            }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && !hasSwipedCycle){
                debugText.text = "DESELECTED";
                clearClickedIndicator();
                //StartCoroutine(CycleCoolDownFalse());
                print("cleared");
            }

        }
        #endregion

            #region TOUCH INPUT
        // Touch Input
        else if(!cyclesMode){

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
                debugText.text = "TOUCH";
                startTouchPosition = Input.GetTouch(0).position;
                movementtracker = 0;
                hasSwiped = false;
                print("began touch");
            }

            else if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved && movementtracker == 0 && !hasSwiped){
                PostTutorialMoveLimiter();
                debugText.text = "SWIPE";
                print("Moved finger");
                currentPosition = Input.GetTouch(0).position;
                Vector3 Distance = currentPosition - startTouchPosition;
                print("X" + Distance.x);
                print("Y" + Distance.y);
                
                if(Mathf.Abs(Distance.x) > Mathf.Abs(Distance.y)){
                    Distance.y = 0;
                }

                else{
                    Distance.x = 0;
                }

                if(phase != -1){
                    if(phase != 0){
                        if(phase != 1){
                            //if (Input.GetKeyDown(KeyCode.W)) {
                            if(phase != 3 && Distance.y > swipeRange){
                                movementtracker++;
                                print("Up");
                                Shift(Vector2.up);
                                stopTouch = true;
                                hasSwiped = true;
                                return;
                            } 

                            if (phase != 4 && Distance.y < -swipeRange){
                            //else if (Input.GetKeyDown(KeyCode.S)) {
                                movementtracker++;
                                print("Down");
                                Shift(Vector2.down);
                                stopTouch = true;
                                hasSwiped = true;
                                return;
                            }
                        }

                        //if (Input.GetKeyDown(KeyCode.A)) {
                        if(phase != 3 && phase != 4 && Distance.x < -swipeRange){
                            movementtracker++;
                            print("Left");
                            Shift(Vector2.left);
                            stopTouch = true;
                            hasSwiped = true;
                            return;
                        } 
                    }
                
                
                    if (phase != 1 && Distance.x > swipeRange){
                    //else if (Input.GetKeyDown(KeyCode.D)) {
                        movementtracker++;
                        print("Right");
                        Shift(Vector2.right);
                        stopTouch = true;
                        hasSwiped = true;
                        return;
                    }
                }
                hasSwiped = false;
            }

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && !hasSwiped){
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector2.zero);                

                if(hit.collider != null){
                    debugText.text = "NOT";
                    print(hit.collider.gameObject);
                    Block selectedBlock = hit.collider.gameObject.GetComponent<Block>();
                    if (disableControl) return; // disable control while updating username

                    if(phase > 4){
                        if(!cyclesMode){
                        selectedBlock.clickedIndicator.SetActive(true);
                        selectedBlock.clicked = true;
                        // StartCoroutine(CycleCoolDown());
                        inCycle = true;
                        clickedBlock = selectedBlock;
                        SetCycleTrue();
                        print("block selected");
                        Debug.Log(this.gameObject.name + " clicked!");
                        }
                    }
                }

                else{
                    debugText.text = "NULL";
                }

            }
        }
        #endregion

        #endregion
    }

    #region MAIN

        #region GAME LOOP

    private void ChangeState(GameState newState)
    {
        state = newState;
        switch(newState)
        {
            case GameState.GenerateLevel:
                //print("Generating...");
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                //print("Spawning..");
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                //print("WaitingInput...");
                break;
            case GameState.Moving:
                //print("Moving...");
                break;
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                loseScreen.SetActive(true);
                worldLoseScreen.SetActive(true);
                break; 
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);      
        }
    }

    void GenerateGrid() {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();
        obstacles = new List<Block>();

        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                var node = Instantiate(nodePrefab, new Vector2(x,y), Quaternion.identity);
                nodes.Add(node);
                node.transform.SetParent(boardParent, true);
            }
        }

        var center = new Vector2((float) width / 2 - 0.5f, (float) height / 2 - 0.5f);

        gameBoard = Instantiate(boardPrefab, center, Quaternion.identity);
        gameBoard.transform.SetParent(boardParent, true);

        gameBoard.size = new Vector2(width, height);
        colorThemeScript.boardReady = true;

        NewUpdateBlockColors();

        ChangeState(GameState.SpawningBlocks);
    }

    private void GameOverCase() {
        var GameOver = (GameOverCheck(Vector2.left) == false && GameOverCheck(Vector2.right) == false && GameOverCheck(Vector2.up) == false && GameOverCheck(Vector2.down) == false) ? true : false;
        if(GameOver && cycleMovesLeft <= 0) {
            ChangeState(GameState.Lose);
            SetHighScore();
            leaderboardHighScore.text = PlayerPrefs.GetInt("myHighScore").ToString();
            gameOverScore.text = score.ToString(); // moved from merge function
            StartCoroutine(SubmitScore(score));
            return;
        }else {    
            ChangeState(GameState.WaitingInput);
            return;
        }
    }

    private bool GameOverCheck(Vector2 dir) {
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks) {
            if(block.Obstacle) continue;
            var next = block.Node;
            do {
                //block.SetBlock(next);
                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                if(possibleNode != null) {
                    if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value)) {
                        return true;
                    }else if (possibleNode.OccupiedBlock == null) {
                        return true;
                    } 
                }
            } while (next != block.Node);
        }
        return false;
    }

    IEnumerator SubmitScore(int score){
        yield return leaderboard.SubmitScoreRoutine(score);
        yield return leaderboard.FetchTopHighScoresRoutine();
    }
    #endregion

        #region BLOCK MANAGEMENT

    void MergeBlocks(Block baseBlock, Block mergingBlock) {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        score += baseBlock.Value * 2;

        if (baseBlock.Value * 2 > currentHighestValue) UpdateBrickValue();

        scoreText.text = score.ToString();
        if (!gotNewHighScore && score >= PlayerPrefs.GetInt("myHighScore", score) && PlayerPrefs.HasKey("myHghScore")) {
            gotNewHighScore = true;
            possibleHighScore = score;
            SetHighScore();
            NewHighScore();
        }
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block) {   
        if(block.Obstacle){
            audioSource.PlayOneShot(blockBreak, 0.2f);
            SpawnBlock(block.Node, block.Value);
            obstacleCount--;
        }
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    void RemoveObstacle(Block block) {   
        if(block.Obstacle){
            audioSource.PlayOneShot(blockBreak, 0.2f);
            SpawnBlock(block.Node, block.Value);
            obstacleCount--;
        }
        blocks.Remove(block);
        Destroy(block.gameObject, .4f);

        foreach (Transform child in block.gameObject.transform) {
            if (child.gameObject.name == "ObstacleBreak") {
                child.gameObject.SetActive(true);
            }
        }
    }

    Node GetNodeAtPosition(Vector2 pos) {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

    public SpriteRenderer GetBoard() {
        if (gameBoard != null) {
            return gameBoard;
        }else {
            return null;
        }
    }

    public List<Node> GetNodes() {
        return nodes;
    }

    public List<Block> GetBlocks() {
        return blocks;
    }
    #endregion

        #region GAME STATE MANAGEMENT

    public void RestartGame() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        SetHighScore();
        PlayerPrefs.SetInt("phase", phase);
        SceneManager.LoadScene(0);
    }

    public void ContinueGame() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        winScreen.SetActive(false);
        ChangeState(GameState.WaitingInput);
    }

    public void RestartTutorial() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        PlayerPrefs.SetInt("phase", tutorialRestartPhase);
        SetHighScore();
        devsIcons.SetActive(true);
        SceneManager.LoadScene(0);
    }

    public void SkipTutorial() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        PlayerPrefs.SetInt("phase", 7);
        SceneManager.LoadScene(0);
    }
    #endregion

    #endregion

    #region BRICK VALUE MANAGEMENT

    void UpdateBrickValue() {
        int[] newBrickValues = new int[brickValues.Length + 1];
        currentHighestValue *= 2; // double current highest value
        brickValues.CopyTo(newBrickValues, 0);
        newBrickValues[newBrickValues.Length - 1] = currentHighestValue;

        brickValues = newBrickValues;
    }

    void UpdateWeightedBrickValues(int[] unweightedBrickValues) {
        int[] tempWeightedBrickValues = new int[AdditionFactorial(unweightedBrickValues.Length)];
        int index = 0;

        int counter = unweightedBrickValues.Length;
        int tempCounter = counter;
        for (int i = 0; i < counter; i++) {
            for (int k = tempCounter; k > 0; k--) {
                tempWeightedBrickValues[index++] = unweightedBrickValues[i];
            }
            tempCounter--;
        }

        weightedBrickValues = tempWeightedBrickValues;
    }

    private int AdditionFactorial(int startNum) {
        int total = 0;
        for (int i = 1; i <= startNum; i++) {
            total += i;
        }
        return total;
    }
    #endregion

    #region SPAWN MANAGEMENT
    
    void SpawnBlocks(int amount)
    { 
        if (phase == -1) {
            ChangeState(blocks.Any(b=>b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
        }else if (phase == 0){
            SpawnBlock(GetNodeAtPosition(new Vector2(0,0)), 2);
            SpawnBlock(GetNodeAtPosition(new Vector2(3,0)), 2);
        }else if (phase == 1){
            SpawnObstacle(GetNodeAtPosition(new Vector2(3,1)));
        }else if (phase == 3){
            SpawnBlock(GetNodeAtPosition(new Vector2(3,0)), 4);
        }else {
            // Get a list of nodes that are not Occupied by a block from the list of nodes
            var freeNodes = nodes.Where(n=>n.OccupiedBlock == null).OrderBy(b=>Random.value).ToList();

            if(round > 1 && Random.value > (1 - obstacleSpawnChance) && obstacleCount < obstacleLimit) {
                foreach (var nodes in freeNodes.Take(amount)) SpawnObstacle(nodes);
                if (freeNodes.Count() <= 1) GameOverCase();
            }else {
                // For each of the free nodes get a certain amount of Nodes that you want to use for spawning the blocks
                foreach (var nodes in freeNodes.Take(amount)) SpawnBlock(nodes, Random.value > 0.8f ? 4 : 2);
                if (freeNodes.Count() <= 1) GameOverCase();    
            }
        }
        ChangeState(blocks.Any(b=>b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value)
    {
        // Instantiate a block prefab at the chosen node location
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        block.transform.SetParent(boardParent, true);
        
        //Set block highlight color
        UpdateHighlightColor(block);
        
        block.transform.DOScale(new Vector3(1f, 1f, 0), 0.5f).SetEase(Ease.OutBounce);

        // Take the block game object and initialize it as a BlockType
        block.Init(GetBlockTypeByValue(value));
        
        // Assign the node to the Block and visa versa
        block.SetBlock(node);

        // Add block to the list
        blocks.Add(block);
    }

    void SpawnObstacle(Node node)
    {       
            if(phase == 1) phase++;
            // Instantiate a block prefab at the chosen node location
            var block = Instantiate(obstaclePrefab, node.Pos, Quaternion.identity);
            block.transform.SetParent(boardParent, true);
            UpdateHighlightColor(block);
            block.transform.DOScale(new Vector3(1f, 1f, 0), 0.5f).SetEase(Ease.OutBounce);
            node.Obstacle = true;
            block.Obstacle = true;

            // List<int> fivePercents = new List<int>(new int[] {16, 32, 64}) ;
            // List<int> twoPercents = new List<int>(new int[] {128, 256, 512, 1024});
            UpdateWeightedBrickValues(brickValues);
            int value;
            if(phase < 3) {
                value = 8;
            }else{
                value = weightedBrickValues[Random.Range(0, weightedBrickValues.Length)];
            }

            block.Value = value;
            block.text.text = block.Value.ToString();

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
            obstacleCount++;
    }

    private Block SpawnBlockWithNoNode(Vector2 spawn, Node node, int value, bool obstacle) {
        Block block;
        // Instantiate a block prefab at the chosen node location
        if(obstacle) {
            block = Instantiate(obstaclePrefab, spawn, Quaternion.identity);    
            block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.01f).SetEase(Ease.OutBounce);
            block.Obstacle = true;
            node.Obstacle = true;

            block.Value = value;
            block.text.text = block.Value.ToString();

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
        }else {
            // Take the block game object and initialize it as a BlockType
            block = Instantiate(blockPrefab, spawn, Quaternion.identity);    
            block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.01f).SetEase(Ease.OutBounce);
            block.Init(GetBlockTypeByValue(value));

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
        }
        return block;
    }

    #endregion

    #region  MOVEMENT

        #region SHIFTING

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);
        // Order the block array by x then by y if it is moving left or down
        // Or change it to ordered by y then x if it is moving right or up
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x ).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();
        bool blocksMoved = false;

        // Go through the ordered list of blocks
        foreach (var block in orderedBlocks) {
            if(block.Obstacle) continue;
            
            // I think of "next" like current node that the block is on
            var next = block.Node;
            do {
                // Make sure the current block sets this as the node
                block.SetBlock(next);

                // Get the next Node in the direction you want to move
                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                //If there is a possible node in that direction
                if(possibleNode != null) {
                    if(possibleNode.Obstacle == false) {
                        // We know a node is present
                        // If its possible to merge, set merge
                        if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value)) {
                            block.MergeBlock(possibleNode.OccupiedBlock);
                            blocksMoved = true;
                        }else if(possibleNode.OccupiedBlock == null){ // Otherwise can we move to this spot?
                            next = possibleNode;
                            blocksMoved = true;
                        }
                    }
                } // None hit? End do while loop
            } while (next != block.Node);     
        }

        if(blocksMoved) {
            if(phase == 0){
                phase++;
            }else if(phase == 2){
                phase++;
            }else if(phase == 3){
                phase++;
            }else if(phase == 4){
                phase++;
            }

            var sequence = DOTween.Sequence();

            foreach(var block in orderedBlocks) {
                var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;
                sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
            }

            sequence.OnComplete(() => {
                var mergeBlocks = orderedBlocks.Where(b => b.MergingBlock != null).ToList();

                foreach (var block in mergeBlocks) {
                    var nodeUp = GetNodeAtPosition(block.Pos + Vector2.up);
                    var nodeDown = GetNodeAtPosition(block.Pos + Vector2.down);
                    var nodeLeft = GetNodeAtPosition(block.Pos + Vector2.left);
                    var nodeRight = GetNodeAtPosition(block.Pos + Vector2.right);

                    if(nodeUp && nodeUp.Obstacle && nodeUp.OccupiedBlock.Value == block.Value * 2 ) {
                        nodeUp.Obstacle = false;
                        RemoveObstacle(nodeUp.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    if(nodeDown && nodeDown.Obstacle && nodeDown.OccupiedBlock.Value == block.Value * 2 ) {
                        nodeDown.Obstacle = false;
                        RemoveObstacle(nodeDown.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    if(nodeLeft && nodeLeft.Obstacle && nodeLeft.OccupiedBlock.Value == block.Value * 2 ) {
                        nodeLeft.Obstacle = false;
                        RemoveObstacle(nodeLeft.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    if(nodeRight && nodeRight.Obstacle && nodeRight.OccupiedBlock.Value == block.Value * 2 ) {
                        nodeRight.Obstacle = false;
                        RemoveObstacle(nodeRight.OccupiedBlock);
                        cycleMovesLeft++;
                    }
                    MergeBlocks(block.MergingBlock, block);
                }
                if(mergeBlocks.Any()) audioSource.PlayOneShot(merges[Random.Range(0, merges.Count - 1)], 0.2f);
                ChangeState(GameState.SpawningBlocks);
            });
        }else {
            ChangeState(GameState.WaitingInput);
        }
    }
    #endregion

        #region CYCLING

    public void Cycle(String locationCheck)
    {
        if(cycleMovesLeft > 0) udioSource.PlayOneShot(buttonPress, 0.2f);
        if (Time.time - cycleTimer < cycleDelay) return;

        cycleTimer = Time.time;
        
        if(cycleMovesLeft == 0) { // toggle "No Cycle" animation on
            audioSource.PlayOneShot(noCycleError, 0.2f);
            noCycleIndicator.SetActive(true);
            return; 
        }

        if(phase == 5) phase++;

        int blockCoordinate = 0;
        int loopCoordinateCheck = 0;

        Vector2 moveDestroyedBlock = new Vector2(0f, 0f);
        Vector2 moveTo = new Vector2(0f, 0f);
        Vector2 noNodeBlockLocation = new Vector2(0f, 0f);
        Vector2 directionOfCycle = new Vector2(0f, 0f);
    
        Block tempBlock = null;

        bool xAxis = false;
        bool yAxis = false;
        bool reverse = false;

        List<Block> orderedBlocks = new List<Block>();
        //var tag = locationCheck.tag;
        switch(locationCheck){
            case "FirstRowLeft":
                blockCoordinate = 3;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 3f);
                moveTo = new Vector2(3f, 3f);
                noNodeBlockLocation = new Vector2(4f, 3f);
                directionOfCycle = Vector2.left;

                yAxis = true;
                break;

            case "FirstRowRight":
                blockCoordinate = 3;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 3f);
                moveTo = new Vector2(0f, 3f);
                noNodeBlockLocation = new Vector2(-1f, 3f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "SecRowLeft":
                blockCoordinate = 2;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 2f);
                moveTo = new Vector2(3f, 2f);
                noNodeBlockLocation = new Vector2(4f, 2f);
                directionOfCycle = Vector2.left;
                
                yAxis = true;
                break;

            case "SecRowRight":
                blockCoordinate = 2;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 2f);
                moveTo = new Vector2(0f, 2f);
                noNodeBlockLocation = new Vector2(-1f, 2f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "ThirdRowLeft":
                blockCoordinate = 1;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 1f);
                moveTo = new Vector2(3f, 1f);
                noNodeBlockLocation = new Vector2(4f, 1f);
                directionOfCycle = Vector2.left;

                yAxis = true;
                break;

            case "ThirdRowRight":
                blockCoordinate = 1;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 1f);
                moveTo = new Vector2(0f, 1f);
                noNodeBlockLocation = new Vector2(-1f, 1f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "FourthRowLeft":
                blockCoordinate = 0;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(-1f, 0f);
                moveTo = new Vector2(3f, 0f);
                noNodeBlockLocation = new Vector2(4f, 0f);
                directionOfCycle = Vector2.left;

                yAxis = true;
                break;

            case "FourthRowRight":
                blockCoordinate = 0;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(4f, 0f);
                moveTo = new Vector2(0f, 0f);
                noNodeBlockLocation = new Vector2(-1f, 0f);
                directionOfCycle = Vector2.right;

                yAxis = true;
                reverse = true;
                break;

            case "FirstColUp":
                blockCoordinate = 0;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(0f, -1f);
                moveTo = new Vector2(0f, 0f);
                noNodeBlockLocation = new Vector2(0f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;

            case "FirstColDown":
                blockCoordinate = 0;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(0f, 4f);
                moveTo = new Vector2(0f, 3f);
                noNodeBlockLocation = new Vector2(0f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;

            case "SecColUp":
                blockCoordinate = 1;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(1f, -1f);
                moveTo = new Vector2(1f, 0f);
                noNodeBlockLocation = new Vector2(1f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;

            case "SecColDown":
                blockCoordinate = 1;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(1f, 4f);
                moveTo = new Vector2(1f, 3f);
                noNodeBlockLocation = new Vector2(1f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;

            case "ThirdColUp":
                blockCoordinate = 2;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(2f, -1f);
                moveTo = new Vector2(2f, 0f);
                noNodeBlockLocation = new Vector2(2f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;
            case "ThirdColDown":
                blockCoordinate = 2;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(2f, 4f);
                moveTo = new Vector2(2f, 3f);
                noNodeBlockLocation = new Vector2(2f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;
            case "FourthColUp":
                blockCoordinate = 3;

                loopCoordinateCheck = 3;
                moveDestroyedBlock = new Vector2(3f, -1f);
                moveTo = new Vector2(3f, 0f);
                noNodeBlockLocation = new Vector2(3f, -1f);
                directionOfCycle = Vector2.up;

                xAxis = true;
                reverse = true;
                break;
            case "FourthColDown":
                blockCoordinate = 3;

                loopCoordinateCheck = 0;
                moveDestroyedBlock = new Vector2(3f, 4f);
                moveTo = new Vector2(3f, 3f);
                noNodeBlockLocation = new Vector2(3f, 4f);
                directionOfCycle = Vector2.down;

                xAxis = true;
                break;
        }

        foreach(var node in nodes) {
            if (xAxis) {
                if (node.Pos.x == blockCoordinate && node.OccupiedBlock != null) {
                    orderedBlocks.Add(node.OccupiedBlock);
                }
            }else if (yAxis) {
                if (node.Pos.y == blockCoordinate && node.OccupiedBlock != null) {
                    orderedBlocks.Add(node.OccupiedBlock);
                }
            }
        }

        if(orderedBlocks.Count() == 0) return;
        cycleMovesLeft--;

        orderedBlocks.OrderBy(b => (xAxis ? b.Pos.x : b.Pos.y)).ToList();
        foreach(var block in orderedBlocks) {
            print("Pos: " + block.Pos);
        }

        if (reverse) orderedBlocks.Reverse();

        // Create and Play the animation
        var sequence = DOTween.Sequence();
        var nodeMovingTo = GetNodeAtPosition(moveTo);
        foreach(var block in orderedBlocks) {
            print("Block Pos: " + block.Pos);
            if(!xAxis && block.Pos.x == loopCoordinateCheck) {
                //print("Means you have to cycle");
                tempBlock = block;
                block.ClearBlock();
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                sequence.Insert(0, tempBlock.transform.DOMove(nodeMovingTo.Pos, travelTime));
                // RemoveBlock(block);
                continue;
            }else if (!yAxis && block.Pos.y == loopCoordinateCheck) {
                //print("Means you have to cycle");
                tempBlock = block;
                block.ClearBlock();
                sequence.Insert(0, block.transform.DOMove(moveDestroyedBlock, travelTime));
                sequence.Insert(0, tempBlock.transform.DOMove(nodeMovingTo.Pos, travelTime));
                continue;
            }

            var possibleNode = GetNodeAtPosition(block.Pos + directionOfCycle);
            var movePoint = possibleNode.Pos;
            var currentNode = GetNodeAtPosition(block.Pos);
            
            block.SetBlock(possibleNode);
            if(block.Obstacle) {
                currentNode.Obstacle = false;
                possibleNode.Obstacle = true;
            }
            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
        }
        if (tempBlock != null) tempBlock.SetBlock(nodeMovingTo);
        if (cycleMovesLeft == 0) GameOverCase();
    }

    IEnumerator cyclings(){
        yield return new WaitForSeconds(.05f);
        cycling = false;
    }
    #endregion
    #endregion

    #region MOBILE
    
    IEnumerator HasSwipedFalse(){
        yield return new WaitForSeconds(.05f);
        hasSwiped = false;
    }

    IEnumerator HasSwipedTrue(){
        yield return new WaitForSeconds(.05f);
        hasSwiped = true;
    }
    
    IEnumerator CycleCoolDown(){
        yield return new WaitForSeconds(.05f);
        inCycle = true;
    }

    IEnumerator CycleCoolDownFalse(){
        yield return new WaitForSeconds(.05f);
        inCycle = false;
    }

    IEnumerator WaitingToClear(){
            yield return new WaitForSeconds(.05f);
            inCycle = true;
    }
    #endregion

    #region UI

        #region UI GENERAL

        public void ToggleCycle() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        if(cycleUI.activeSelf) {
            cycleUI.SetActive(false);
        }else {
            cycleUI.SetActive(true);
        }
    }

    public void ToggleSettings() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        if (!settingsActive) {
            settingsActive = true;
            OffsetIcons(usernameSetting, 1);
            OffsetIcons(tutorialSetting, 2);
            OffsetIcons(audioSetting, 3);
            Invoke("ToggleSettingsInfo", offsetDuration * 0.25f);
        }else {
            settingsActive = false;
            OffsetIcons(usernameSetting, 1, true);
            OffsetIcons(tutorialSetting, 2, true);
            OffsetIcons(audioSetting, 3, true);
            Invoke("ToggleSettingsInfo", offsetDuration * 0.25f);
            backgroundDarken.SetActive(false);
        }
    }

    public void ToggleSettingsInfo() {
        if (settingsActive) {
            usernameSettingInfo.SetActive(true);
            tutorialSettingInfo.SetActive(true);
            audioSettingInfo.SetActive(true);
        }else {
            usernameSettingInfo.SetActive(false);
            tutorialSettingInfo.SetActive(false);
            audioSettingInfo.SetActive(false);
        }
    }

    public void OffsetIcons(GameObject icon, int offsetIndex, bool reverse = false) {
        float baseOffset = 125;
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        if (reverse) { // baseOffset *= -1
            iconRect.transform.DOLocalMoveY(0, offsetDuration);
        }else {
            iconRect.transform.DOLocalMoveY(baseOffset * offsetIndex, offsetDuration);
        }
    }
    #endregion

        #region USERNAME

    public void SubmitUsername(){
        if(username.text.Length > 0){
            phase = 0;
            ChangeState(GameState.SpawningBlocks);
            PlayerPrefs.SetString("PlayerID", username.text);
            print(username.text);
        }
    }

    public void UpdateUsername(){
        if(username.text.Length > 0){
            PlayerPrefs.SetString("PlayerID", username.text);
            print(username.text);
        }
    }

    public void UpdateUsernameOnSettings(){
        if(username.text.Length > 0){
            PlayerPrefs.SetString("PlayerID", updateusername.text);
            print(updateusername.text);
        }
        disableControl = false;
    }

    public void ToggleUsernameUpdate() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        if (updateUsername.activeInHierarchy) {
            updateUsername.SetActive(false);
            disableControl = false;
        }else {
            updateUsername.SetActive(true);
            disableControl = true;
        }
        backgroundDarken.SetActive(!backgroundDarken.activeInHierarchy);
    }

    public void ForceCloseUsernameUpdate() {
        if (updateUsername.activeInHierarchy) ToggleUsernameUpdate();
        backgroundDarken.SetActive(false);
    }

        #endregion

        #region HIGH SCORE ALERT
        
    void NewHighScore() {
        print("NEW HIGH SCORE ACHIEVED!");
        GameObject highScoreInstance = Instantiate(newHighScoreObject, Vector3.zero, Quaternion.identity);
        highScoreInstance.name = "NewHighScore";
        highScoreInstance.GetComponent<RectTransform>().localPosition = newHighScorePos;
        highScoreInstance.transform.SetParent(GameObject.Find("World Canvas").transform, false);
        highScoreInstance.transform.localScale = startScale;

        //adjust color
        Color newColor = colorThemeScript.GetCurrentHighScoreColor();
        highScoreInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = newColor;
        highScoreInstance.transform.GetChild(1).GetComponent<Image>().color = newColor;

        //animation;
        highScoreInstance.transform.DOScale(endScale, scaleTime).SetEase(Ease.OutBounce);

        Destroy(highScoreInstance, newHighScoreDuration);
    }

    void SetHighScore() {
        if(possibleHighScore > (PlayerPrefs.GetInt("myHighScore"))) {
            PlayerPrefs.SetInt("myHighScore", possibleHighScore);
        }
    }
    #endregion

        #region UI TUTORIAL

    void PostTutorialMoveLimiter() {
        if (phase >= 6) {
            if (postTutorialMoveCounter < postTutorialMoveLimit) {
                print("Incrementing Counter: " + postTutorialMoveCounter);
                postTutorialMoveCounter++;
            }else {
                phase6.SetActive(false);
                devsIcons.SetActive(false);
                PlayerPrefs.SetInt("phase", phase); // might night be needed in 'game end/restart' methods
            }
        } // Can be optimized
    }
    #endregion

    #endregion

    #region AUDIO

    public void ToggleAudio() {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        if(!muted){
            audioListener.enabled = false;
            audioOnIcon.SetActive(false);
            audioOffIcon.SetActive(true);
            muted = true;
            PlayerPrefs.SetInt("audioToggle", muted ? 0 : 1);
        }else if(muted){
            audioListener.enabled = true;
            audioOnIcon.SetActive(true);
            audioOffIcon.SetActive(false);
            muted = false;
            PlayerPrefs.SetInt("audioToggle", muted ? 0 : 1);
        }
    }

    public void InitializeAudioState() {
        if (PlayerPrefs.GetInt("audioToggle", 1) == 1) {
            audioListener.enabled = true;
            audioOnIcon.SetActive(true);
            audioOffIcon.SetActive(false);
            muted = false;
        }else if (PlayerPrefs.GetInt("audioToggle", 1) == 0) {
            audioListener.enabled = false;
            audioOnIcon.SetActive(false);
            audioOffIcon.SetActive(true);
            muted = true;
        }
    }

    #endregion

}

[Serializable]
public class BlockType 
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
