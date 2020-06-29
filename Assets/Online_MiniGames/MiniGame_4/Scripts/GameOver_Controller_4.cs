using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver_Controller_4 : MonoBehaviour
{
    public Text txtResult, txtLeaveTimer;

    // Start is called before the first frame update
    void Awake()
    {
        txtLeaveTimer.text = "";
        if (PhotonNetwork.player.GetScore() == -10) {
            txtResult.text = "DÉFAITE !";
        } else if (PhotonNetwork.player.GetScore() == 10){
            txtResult.text = "VICTOIRE !";
        } else if (PhotonNetwork.player.GetScore() == -5) {
            txtResult.text = "L'adversaire a quitté.";
        }
        this.StartCoroutine(this.LeaveTimer());
    }

    IEnumerator LeaveTimer()
    { 
        for(int i=10; i>=0; i--)
        {
            yield return new WaitForSeconds(1);
            txtLeaveTimer.text = "Déconnexion dans " + i + "s";
        }
        this.Leave();
    }

    public void Leave()
    {
        Application.Quit();
    }
}
