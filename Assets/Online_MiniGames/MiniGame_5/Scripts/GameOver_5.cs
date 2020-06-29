using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver_5 : MonoBehaviour
{
    public Text txtLeaveTimer, txtScores, txtResult;
    public GameObject scoreboard;
    // Start is called before the first frame update
    void Awake()
    {
        txtLeaveTimer.text = "";
        txtScores.text = "";

        if(PhotonNetwork.player.GetScore() >= 10) {
            txtResult.text = "VICTOIRE";
        } else if (PhotonNetwork.player.GetScore() >=0 && PhotonNetwork.player.GetScore() < 10) {
            txtResult.text = "DÉFAITE";
        } else if (PhotonNetwork.player.GetScore() == -5) {
            txtResult.text = "L'adversaire a quitté.";
            scoreboard.SetActive(false);
        }

        txtScores.text = "\n";
        foreach(PhotonPlayer player in PhotonNetwork.playerList) {
            int playerScore = player.GetScore() / 2;
            txtScores.text += player.NickName + " [" + playerScore.ToString() + " points]\n";
        }
        StartCoroutine(this.LeaveTimer());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Leave()
    {
        Application.Quit();
    }

    IEnumerator LeaveTimer()
    {
        //txtLeaveTimer.text = "Déconnexion dans 10";

        for(int i=10; i>=0; i--)
        {
            yield return new WaitForSeconds(1.0f);
            txtLeaveTimer.text = "Déconnexion dans " + i;
        }
        this.Leave();
    }
}
