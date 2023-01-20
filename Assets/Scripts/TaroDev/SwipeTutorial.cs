using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using TMPro;


public class SwipeTutorial : MonoBehaviour
{
    public static SwipeTutorial instance;
    // private void Awake() => instance = this;

    [SerializeField] public Leaderboard leaderboard;

    [SerializeField] private int phase = 0;

    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Block obstaclePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float travelTime = 0.2f;
    [SerializeField] private int winCondition = 2048;
    [SerializeField] private bool gameOver;
    [SerializeField] private int score = 0;
    [SerializeField] private int possibleHighScore;
    [SerializeField] private int cycleMovesLeft = 5;
    [SerializeField] private int countUntilObstacle = 5;
    [SerializeField] private int obstacleCount = 0;
    [SerializeField] private float obstacleSpawnChance = 0.1f;
    [SerializeField] private bool cycling = false;
    public bool cyclesMode;
    public Block clickedBlock;

    [Header("Brick Values")]
    [SerializeField] private int[] brickValues = new int[0];
    [SerializeField] private int[] weightedBrickValues;
    [SerializeField] private int currentHighestValue;

    [Header("UI")]
    [SerializeField] private GameObject winScreen, loseScreen;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameoverscore;
    [SerializeField] private TMP_Text highscoreText;
    [SerializeField] private TMP_Text cycleMoves;
    [SerializeField] private GameObject cycleUI;
    [SerializeField] private GameObject audioOnIcon;
    [SerializeField] private GameObject audioOffIcon;
    [SerializeField] public UIShake shakeScript;

    [Header("Audio")]
    [SerializeField] private List<AudioClip> merges = new List<AudioClip>();
    [SerializeField] private AudioClip blockBreak;
    [SerializeField] private AudioClip buttonPress;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool muted = false;
    [SerializeField] private AudioListener audioListener;

    [Header("Mobile")]
    private Vector3 startTouchPosition;
    private Vector3 currentPosition;
    private Vector3 endTouchPosition;
    [SerializeField] private bool stopTouch = false;
    [SerializeField] private float swipeRange;

    [Header("Script Reference")]
    [SerializeField] private ColorTheme colorThemeScript;
    private SpriteRenderer gameBoard;

    private List<Node> nodes;
    private List<Block> blocks;
    private List<Block> obstacles;
    private GameState state;
    private int round;

    [Header("Block Color Manager")]
    public Dictionary<int, Color> blockColors;

    // Get a Blocktype by the int value you pass
    // It takes from the list of types that already has a color attached to the value
    private BlockType GetBlockTypeByValue(int value) => types.First(t=>t.Value == value);

    void Awake() {
        instance = this;
        // NewUpdateBlockColors();
    }

    void Start()
    {
        highscoreText.text = PlayerPrefs.HasKey("myHighScore") ? PlayerPrefs.GetInt("myHighScore").ToString() : "0";
        ChangeState(GameState.GenerateLevel);
        weightedBrickValues = brickValues;
        currentHighestValue = brickValues[brickValues.Length - 1];
        cyclesMode = false;
        // UpdateBlockColors();
    }

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
                }
            }
        }
    }

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

    void Update()
    {
        cycleMoves.text = cycleMovesLeft.ToString();
        possibleHighScore = score;

        if(state != GameState.WaitingInput) return;

        // Keyboard Input
        if(!cyclesMode){
            if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
            if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
        }    
        
        // Touch Input
        if(!cyclesMode){

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
                startTouchPosition = Input.GetTouch(0).position;
            }

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved){
                currentPosition = Input.GetTouch(0).position;
                Vector3 Distance = currentPosition - startTouchPosition;
                //if (Input.GetKeyDown(KeyCode.W)) {
                if(Distance.y > swipeRange){
                    Shift(Vector2.up);
                    stopTouch = true;
                } 

                else if (Distance.y < -swipeRange){
                //else if (Input.GetKeyDown(KeyCode.S)) {
                    print("Down");
                    Shift(Vector2.down);
                    stopTouch = true;
                }

                //if (Input.GetKeyDown(KeyCode.A)) {
                else if(Distance.x < -swipeRange){
                    print("Left");
                    Shift(Vector2.left);
                    stopTouch = true;
                } 
                else if (Distance.x > swipeRange){
                //else if (Input.GetKeyDown(KeyCode.D)) {
                    print("Right");
                    Shift(Vector2.right);
                    stopTouch = true;
                }
            }
        }
    }

    private void ChangeState(GameState newState)
    {
        state = newState;
        switch(newState)
        {
            case GameState.GenerateLevel:
                //dprint("Generating...");
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
                break; 
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);      
        }
    }

    void GenerateGrid()
    {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>();
        obstacles = new List<Block>();

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x,y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var center = new Vector2((float) width / 2 - 0.5f, (float) height / 2 - 0.5f);

        gameBoard = Instantiate(boardPrefab, center, Quaternion.identity);
        gameBoard.size = new Vector2(width, height);
        colorThemeScript.boardReady = true;

        NewUpdateBlockColors();

        ChangeState(GameState.SpawningBlocks);
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


    void SpawnBlocks(int amount)
    {
        // // Get a list of nodes that are not Occupied by a block from the list of nodes
        // var freeNodes = nodes.Where(n=>n.OccupiedBlock == null).OrderBy(b=>Random.value).ToList();

        // if(round > 1 && Random.value > (1 - obstacleSpawnChance) && obstacleCount < 3)
        // {
        //     foreach (var nodes in freeNodes.Take(amount))
        //     {
        //         SpawnObstacle(nodes);
        //     }
        // }

        // else
        // {
        //     // For each of the free nodes get a certain amount of Nodes that you want to use
        //     // for spawning the blocks
        //     foreach (var nodes in freeNodes.Take(amount))
        //     {
        //         SpawnBlock(nodes, Random.value > 0.8f ? 4 : 2);
        //     }

        //     if (freeNodes.Count() == 1) {
        //         //print("FN: " + freeNodes.Count());
        //         // if(cycleMovesLeft == 0)
        //         //     return;
        //         GameOverCase();
        //     }     
        // }
            
        SpawnBlock(GetNodeAtPosition(new Vector2(0,0)), 2);
        SpawnBlock(GetNodeAtPosition(new Vector2(3,0)), 2);
        
        

        // "b=>b" - "Is there any..."
        ChangeState(blocks.Any(b=>b.Value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    IEnumerator SubmitScore(int score){
        yield return leaderboard.SubmitScoreRoutine(score);
    }

    void SpawnBlock(Node node, int value)
    {
        // Instantiate a block prefab at the chosen node location
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        
        block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.5f).SetEase(Ease.OutBounce);

        // Take the block game object and initialize it as a BlockType
        block.Init(GetBlockTypeByValue(value));
        
        // Assign the node to the Block and visa versa
        block.SetBlock(node);

        // Add block to the list
        blocks.Add(block);
    }

    void SpawnObstacle(Node node)
    {   
            // Instantiate a block prefab at the chosen node location
            var block = Instantiate(obstaclePrefab, node.Pos, Quaternion.identity);    
            block.transform.DOScale(new Vector3(0.9f, 0.9f, 0), 0.5f).SetEase(Ease.OutBounce);
            node.Obstacle = true;
            block.Obstacle = true;

            // List<int> fivePercents = new List<int>(new int[] {16, 32, 64}) ;
            // List<int> twoPercents = new List<int>(new int[] {128, 256, 512, 1024});
            int value = weightedBrickValues[Random.Range(0, weightedBrickValues.Length)];

            // if(Random.value <= 0.6f){
            //     value = 4;
            // }
            // else if(Random.value <= 0.75f){
            //     value = 16;
            // }
            // else if(Random.value <= 0.90f){
            //     value = fivePercents[Random.Range(0, 3)];
            // }
            // else if(Random.value <= 1f){
            //     value = twoPercents[Random.Range(0, 4)];
            // }

            block.Value = value;
            block.text.text = block.Value.ToString();

            // Assign the node to the Block and visa versa
            block.SetBlock(node);

            // Add block to the list
            blocks.Add(block);
            obstacleCount++;
    }

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        // Order the block array by x then by y if it is moving left or down
        // Or change it to ordered by y then x if it is moving right or up
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x ).ThenBy(b => b.Pos.y).ToList(); 
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        bool blocksMoved = false;

        // Go through the ordered list of blocks
        foreach (var block in orderedBlocks)
        {
            if(block.Obstacle)
                continue;
            // I think of "next" like current node that the block is on
            var next = block.Node;
            do {
                // Make sure the current block sets this as the node
                block.SetBlock(next);

                // Get the next Node in the direction you want to move
                var possibleNode = GetNodeAtPosition(next.Pos + dir);

                //If there is a possible node in that direction
                if(possibleNode != null)
                {
                    if(possibleNode.Obstacle == false)
                    {
                        // We know a node is present
                        // If its possible to merge, set merge
                        if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                        {
                            block.MergeBlock(possibleNode.OccupiedBlock);
                            blocksMoved = true;
                        }

                        // Otherwise can we move to this spot?
                        else if(possibleNode.OccupiedBlock == null){
                            next = possibleNode;
                            blocksMoved = true;
                        }
                    }
                    
                    // None hit? End do while loop
                }
            } while (next != block.Node);
               
        }

        if(blocksMoved)
        {
            var sequence = DOTween.Sequence();

            foreach(var block in orderedBlocks)
            {
                var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

                sequence.Insert(0, block.transform.DOMove(movePoint, travelTime));
            }

            sequence.OnComplete(() => {
                var mergeBlocks = orderedBlocks.Where(b => b.MergingBlock != null).ToList();

                foreach (var block in mergeBlocks)
                {
                    // foreach(var obstacle in obstacles)
                    //     {
                            
                    //         var nodeUp = GetNodeAtPosition(obstacle.Pos + Vector2.up);
                    //         print(nodeUp.Pos);
                    //         var nodeDown = GetNodeAtPosition(obstacle.Pos + Vector2.down);
                    //         print(nodeDown.Pos);
                    //         var nodeLeft = GetNodeAtPosition(obstacle.Pos + Vector2.left);
                    //         print(nodeLeft.Pos);
                    //         var nodeRight = GetNodeAtPosition(obstacle.Pos + Vector2.right);
                    //         print(nodeRight.Pos);

                    //         if((nodeUp && block.Node == nodeUp) || (nodeDown && block.Node == nodeDown) || (nodeLeft && block.Node == nodeLeft) || (nodeRight && block.Node == nodeRight))
                    //         {
                    //             //obstacles.Remove(obstacle);
                    //             print(obstacle.Pos);
                    //         }
                            
                    //     }
                    var nodeUp = GetNodeAtPosition(block.Pos + Vector2.up);
                    var nodeDown = GetNodeAtPosition(block.Pos + Vector2.down);
                    var nodeLeft = GetNodeAtPosition(block.Pos + Vector2.left);
                    var nodeRight = GetNodeAtPosition(block.Pos + Vector2.right);

                    if(nodeUp && nodeUp.Obstacle && nodeUp.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeUp.Obstacle = false;
                        RemoveBlock(nodeUp.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    if(nodeDown && nodeDown.Obstacle && nodeDown.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeDown.Obstacle = false;
                        RemoveBlock(nodeDown.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    if(nodeLeft && nodeLeft.Obstacle && nodeLeft.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeLeft.Obstacle = false;
                        RemoveBlock(nodeLeft.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    if(nodeRight && nodeRight.Obstacle && nodeRight.OccupiedBlock.Value == block.Value * 2 )
                    {
                        nodeRight.Obstacle = false;
                        RemoveBlock(nodeRight.OccupiedBlock);
                        cycleMovesLeft++;
                    }

                    MergeBlocks(block.MergingBlock, block);
                    
                    
                }

                if(mergeBlocks.Any()) audioSource.PlayOneShot(merges[Random.Range(0, merges.Count - 1)], 0.2f);
                
                ChangeState(GameState.SpawningBlocks);
            });

            //audioSource.PlayOneShot(swipe, 0.2f);
        }

        else
        {
            //var freeNodes = nodes.Where(n=>n.OccupiedBlock == null).OrderBy(b=>Random.value).ToList();
            if (orderedBlocks.Count() == 16) {
                shakeScript.TriggerShake();
            }

            ChangeState(GameState.WaitingInput);
        }
        
    }


    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        score += baseBlock.Value * 2;

        scoreText.text = score.ToString();
        gameoverscore.text = score.ToString();
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {   
        if(block.Obstacle){
            audioSource.PlayOneShot(blockBreak, 0.2f);
            SpawnBlock(block.Node, block.Value);
            obstacleCount--;
        }
        blocks.Remove(block);
        Destroy(block.gameObject);
    }


    Node GetNodeAtPosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

    public void RestartGame()
    {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        SceneManager.LoadScene(0);
        if(possibleHighScore > (PlayerPrefs.GetInt("myHighScore")))
        {
            PlayerPrefs.SetInt("myHighScore", possibleHighScore);
        }
        
    }

    public void ContinueGame()
    {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        winScreen.SetActive(false);
        ChangeState(GameState.WaitingInput);
    }

    public void ToggleCycle()
    {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        if(cycleUI.activeSelf)
        {
            cycleUI.SetActive(false);
        }

        else
        {
            cycleUI.SetActive(true);
        }
    }

    public void ToggleAudio()
    {
        audioSource.PlayOneShot(buttonPress, 0.2f);
        if(!muted){
            audioListener.enabled = false;
            audioOnIcon.SetActive(false);
            audioOffIcon.SetActive(true);
            muted = true;
        }

        else if(muted){
            audioListener.enabled = true;
            audioOnIcon.SetActive(true);
            audioOffIcon.SetActive(false);
            muted = false;
        }
    }
}


