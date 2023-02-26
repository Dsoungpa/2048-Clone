 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    public static Leaderboard instance;

    int leaderboardID = 8851;
    public TMP_Text playerNames;
    public TMP_Text playerScores;
    public Leaderboard leaderboardPlayerPrefab;
    public GameObject leaderboardUI;
    public Transform leaderboardPanel;
    public Vector2 leaderboardUITransform;
    [SerializeField] private int displayAmount;

    public IEnumerator SubmitScoreRoutine(int scoreToUpload) {
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        print(playerID);
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardID, (response) => 
        {
            if (response.success) {
                Debug.Log("Successfully uploaded score");
                done = true;
            }else {
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public IEnumerator FetchTopHighScoresRoutine() {
        bool done = false;
        LootLockerSDKManager.GetScoreList(leaderboardID, displayAmount, 0, (response) =>
        {
            if(response.success) {
                ClearLeaderboardPanel();
                
                string tempPlayerNames = "";
                //string tempPlayerScores = "PPS\n";

                LootLockerLeaderboardMember[] members = response.items;

                for(int i = 0; i < members.Length; i++) {
                    if(members[i].player.name != "") {
                        tempPlayerNames = members[i].rank + ". " + members[i].player.name;
                    }else {
                        tempPlayerNames = members[i].rank + ". " + members[i].player.id.ToString();
                    }
                    Leaderboard player = Instantiate(leaderboardPlayerPrefab, leaderboardPanel);
                    player.playerNames.text = tempPlayerNames;
                    player.playerScores.text = members[i].score.ToString();
                }
                done = true;
            }else {
                Debug.Log("Failed: " + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    
    public void ClearLeaderboardPanel() {
        int children = leaderboardPanel.childCount;
        for(int i = 1; i < children; i++) {
            Destroy(leaderboardPanel.GetChild(i).gameObject);
        }
    }
}
