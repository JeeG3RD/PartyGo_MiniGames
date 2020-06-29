using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InGameNetworkManager_3 : MonoBehaviour
{
    public Text txtWaitPlayers;
    private bool isStarted; 


    // au démarrage de la partie
    void Start() 
    {
        // On appelle la méthode permettant de vérifier si nos 2 joueurs sont connectés
        CheckWaitForPlayers();
    }

    void Update() 
    {
        if (isStarted && PhotonNetwork.room.PlayerCount < 2)
        {
            PhotonNetwork.player.SetScore(-5);
            PhotonNetwork.LoadLevel("GameOver_OnlineGame_3");
        }
    }

    // Lorsqu'un joueur se connecte, on rappelle la methode permettant de vérifier la présence de 2 joueurs
    void OnPhotonPlayerConnected() 
    {
        CheckWaitForPlayers();
    }

    void OnPhotonPlayerDisctonnected() 
    {
        PhotonNetwork.player.SetScore(-5);
        PhotonNetwork.LoadLevel("GameOver_OnlineGame_3");
    }

    private void CheckWaitForPlayers() 
    {
        // Tant que les 2 joueurs ne sont pas connectés, on affiche un message d'attente
        if (PhotonNetwork.room.PlayerCount < 2) {
            PhotonNetwork.SetMasterClient(PhotonNetwork.player);
            txtWaitPlayers.text = "Attente des joueurs ... [" + PhotonNetwork.room.PlayerCount + "/" + PhotonNetwork.room.MaxPlayers + "]";
            return;
        } else  {
            txtWaitPlayers.text = "";
            isStarted = true;

            var sp = new Vector3(0,0,0);
            if(PhotonNetwork.isMasterClient) {
                sp = new Vector3(0.0f, 1.33f, 0.70f);
            } else {
                sp = new Vector3(0.0f, 1.33f, 3.30f);
            }

            GameObject MyPlayer;
            MyPlayer = PhotonNetwork.Instantiate(
                "Player_3",
                sp,
                Quaternion.identity,
                0
            );
            MyPlayer.GetComponent<Transform>().Rotate(-90f,0,0);
            MyPlayer.GetComponent<PlayerController_3>().enabled = true;
            MyPlayer.GetComponentInChildren<Camera>().enabled = true;
            MyPlayer.GetComponentInChildren<Canvas>().enabled = true;
        }
    }

    public void OnBtnLeaveClick()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player.ID);
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
}
