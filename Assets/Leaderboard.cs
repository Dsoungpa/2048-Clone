using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [Header("Leaderboard ID ")]
    [SerializeField] private int leaderBoardID = 8851;    //ID which is found in website(corresponds to specific leaderboard)

    [Header("Display Name and Scores")]
    public TextMeshProUGUI playerNames;
    public TextMeshProUGUI playerScores;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderBoardID, (response) =>
        {
            if(response.success)
            {
                Debug.Log("Successfully uploaded score");
                done = true;
            }
            else{
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done = false);
    }

    public IEnumerator FetchTopHighscoresRoutine()      //grabs names and scores from website
    {
        bool done = false;
        LootLockerSDKManager.GetScoreList(leaderBoardID, 15, 0, (response) =>
        {
            if(response.success)
            {
                string tempPlayerNames = "Names\n\n";
                string tempPlayerScores = "Scores\n\n";

                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    tempPlayerNames += members[i].rank + ".  ";
                    if(members[i].player.name != "")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else{
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score + "\n";
                    tempPlayerNames += "\n";
                }
                done = true;
                playerNames.text = tempPlayerNames;
                playerScores.text = tempPlayerScores;
            }
            else{
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    
}
