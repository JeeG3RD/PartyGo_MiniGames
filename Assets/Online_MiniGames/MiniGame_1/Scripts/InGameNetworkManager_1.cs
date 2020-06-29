using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameNetworkManager_1 : MonoBehaviour
{
    public Text txtWaitPlayers;
    private bool isStarted; 


    //au démarrage de la partie
    void Start()
    {
        //On appelle la méthode permettant de vérifier si nos 2 joueurs sont connectés
        CheckWaitForPlayers();
    }

    void Update()
    {
        if(isStarted && PhotonNetwork.room.PlayerCount < 2)
        {
            PhotonNetwork.player.SetScore(-5);
            PhotonNetwork.LoadLevel("GameOver_OnlineGame_1");
        }
    }

    //Lorsqu'un joueur se connecte, on rappelle la methode permettant de vérifier la présence de 2 joueurs
    void OnPhotonPlayerConnected()
    {
        CheckWaitForPlayers();
    }

    void OnPhotonPlayerDisctonnected()
    {
        PhotonNetwork.player.SetScore(-5);
        PhotonNetwork.LoadLevel("GameOver");
    }

    private void CheckWaitForPlayers()
    {
        //Tant que les 2 joueurs ne sont pas connectés, on affiche un message d'attente
        if(PhotonNetwork.room.PlayerCount < 2)
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.player);
            txtWaitPlayers.text = "Attente des joueurs ... [" + PhotonNetwork.room.PlayerCount + "/" + PhotonNetwork.room.MaxPlayers + "]";
            return;
        }
        else
        {
            txtWaitPlayers.text = "";
            isStarted = true;
        }

        //Une fois que nos 2 joueurs sont présent, on les instantie
        Vector3 sp;
        //Point d'apparition dans la scene
        sp = new Vector3(0, 1, -10);

        GameObject MyPlayer;
        MyPlayer = PhotonNetwork.Instantiate
            ("Player_Game1", sp, Quaternion.identity, 0);
        MyPlayer.GetComponent<PlayerController_1>().enabled = true;
        MyPlayer.GetComponentInChildren<Camera>().enabled = true;
        MyPlayer.GetComponentInChildren<Canvas>().enabled = true;
    }

    public void OnBtnLeaveClick()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player.ID);
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
}
