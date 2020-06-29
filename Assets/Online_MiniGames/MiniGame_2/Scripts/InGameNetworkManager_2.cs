using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameNetworkManager_2 : MonoBehaviour
{
    public Text txtWaitPlayers;
    private bool isStarted; 
    private int masterClientRole;


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
            PhotonNetwork.LoadLevel("GameOver_OnlineGame_2");
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
        PhotonNetwork.LoadLevel("GameOver_OnlineGame_2");
    }

    private void CheckWaitForPlayers() 
    {
        // Tant que les 2 joueurs ne sont pas connectés, on affiche un message d'attente
        if (PhotonNetwork.room.PlayerCount < 2) {
            PhotonNetwork.SetMasterClient(PhotonNetwork.player);
            txtWaitPlayers.text = "Attente des joueurs ... [" + PhotonNetwork.room.PlayerCount + "/" + PhotonNetwork.room.MaxPlayers + "]";
            return;
        } else  {
            //Le joueur étant le client maitre tire aléatoirement
            //son role entre 1 & 2 et l'envoi en parametre dans un RaiseEvent
            if (PhotonNetwork.player.IsMasterClient) {
                object[] datas = new object[1];
                datas[0] = Random.Range(1, 3);
                txtWaitPlayers.text = "";
                RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RoleDefine, datas, true, options);
            }
            PhotonNetwork.OnEventCall += this.OnPhotonEvent;
            txtWaitPlayers.text = "";
            isStarted = true;
        }
    }

    private void OnPhotonEvent(byte eventCode, object content, int senderID)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;

        if (code == PhotonEventCodes.RoleDefine) {
            object[] datas = content as object[];
            int data = (int)datas[0];
            var roleName = "";
            var sp = new Vector3(0,0,0);

            //En fonction du role et du fait que le joueur soit masterclient ou non
            //on défini le role name et la vector correspondants au GameObject
            //de son role
            if (PhotonNetwork.player.IsMasterClient) {
                if (data == 1) {
                    roleName = "AttackPlayer_2";
                    sp = new Vector3(0, 0, 120);
                } else if (data == 2) {
                    roleName = "DefensePlayer_2";
                    sp = new Vector3(0, 0, 95.2f);
                }
            } else {
                if (data == 1) {
                    roleName = "DefensePlayer_2";
                    sp = new Vector3(0, 0, 95.2f);
                } else if (data == 2) {
                    roleName = "AttackPlayer_2";
                    sp = new Vector3(0, 0, 120);
                }
            }

            //Puis instantiation du GameObject et activation de ses composants
            GameObject MyPlayer;
            MyPlayer = PhotonNetwork.Instantiate(
                roleName,
                sp,
                Quaternion.identity,
                0
            );
            MyPlayer.GetComponent<PlayerController_2>().enabled = true;
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
