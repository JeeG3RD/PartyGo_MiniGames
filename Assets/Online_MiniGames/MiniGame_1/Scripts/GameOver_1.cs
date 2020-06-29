using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver_1 : MonoBehaviour
{
    public Text txtWinner, scoreBoard, txtLeaveTimer;
    public GameObject panelScoreBoard;
    private bool drawMatch;
    private PhotonPlayer winner;
    
    void Awake()
    {
        if(PhotonNetwork.player.GetScore() != -5)
        {
            int i = 0;
            drawMatch = false;
            scoreBoard.text = "";
            //On boucle sur tous les joueurs pour dé
            foreach(PhotonPlayer player in PhotonNetwork.playerList)
            {
                //verifie si son score n'est pas
                //a -1 -> s'est trompé de couleur
                //Affichage du nom du joueur + son score OU Perdu dans le
                //scoreboard
                scoreBoard.text += player.NickName + " [" + player.GetScore().ToString() + " points]\n";

                //si c'est la premiere ittération
                //le joueur est défini comme winner
                if(i == 0)
                    winner = player;

                //Si ce joueur a un score suppérieur au winner, 
                //il est assigné à la variable
                if(player.ID != winner.ID && player.GetScore() > winner.GetScore())
                {
                    winner = player;
                    drawMatch = false;
                }

                //Si les deux joueurs ont le meme score, match nul
                if(player.ID != winner.ID && player.GetScore() == winner.GetScore())
                    drawMatch = true;

                i++;
            }

            //Affiche le nom du gagnant, ou le texte de match nul
            if(!drawMatch)
            {
                if(winner.ID == PhotonNetwork.player.ID)
                {
                    txtWinner.text = "VICTOIRE";
                }
                else
                {
                    txtWinner.text = "DÉFAITE";
                }
            }

            else
            {
                txtWinner.text = "MATCH NUL";
            }
        }
        else
        {

            txtWinner.text = "L'adversaire a quitté la partie.";
            panelScoreBoard.SetActive(false); //Retire la panneau des scores

        }
        StartCoroutine(LeaveTimer());
    }

    void Update()
    {
        scoreBoard.text = "";
        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {   
            scoreBoard.text += player.NickName + " [" + player.GetScore().ToString() + " points]\n";
        }
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

    //Quite l'application
    public void Leave()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player.ID);
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
}
