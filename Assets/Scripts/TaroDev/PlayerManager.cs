using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("Script Reference To Leaderboard")]
    [SerializeField] public Leaderboard leaderboard;
    [Header("Custom Name InputField")]
    [SerializeField] public TMP_InputField playerNameInputField;
    [Header("Update Player Name Through Settings")]
    [SerializeField] public TMP_InputField updateusername;
    [Header("Length of Input Name")]
    public int lengthofName = 10;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetupRoutine());
    }

    public IEnumerator SetupRoutine()
    {
        playerNameInputField.characterLimit = lengthofName;
        yield return LoginRoutine();
        //yield return leaderboard.FetchTopHighScoresRoutine();
    }

    public IEnumerator LoginRoutine()  //makes sure that we have connection with server
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if(response.success)
            {
                Debug.Log("Player was logged in");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                done = true;
            }else{
                Debug.Log("Could not connect to server");
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public void SetPlayerName()
    {
        LootLockerSDKManager.SetPlayerName(playerNameInputField.text, (response) =>
        {
            if(response.success)
            {
                Debug.Log("Successfully set player name");
            }
            else{
                Debug.Log("Could not set player name");
            }
        });
    }

    public void SetPlayerNameThroughSettings()
    {
        LootLockerSDKManager.SetPlayerName(updateusername.text, (response) =>
        {
            if(response.success)
            {
                Debug.Log("Successfully set player name");
            }
            else{
                Debug.Log("Could not set player name");
            }
        });
    }

    public void UpdateLeaderboards(){
        StartCoroutine(UpdateLeaderboard());
    }

    public IEnumerator UpdateLeaderboard() {
        yield return new WaitForSecondsRealtime(0.5f);
        yield return leaderboard.FetchTopHighScoresRoutine();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
