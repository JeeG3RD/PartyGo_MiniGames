using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_2 : MonoBehaviour
{
    // Start is called before the first frame update
    private PhotonView view;

    // Role = 1 -> Attaque , Role = 2 -> Défense
    public int role;
    public Text txtTimer, txtScore, txtGameStart;
    public GameObject roleObject, scrollBar, panelReady;
    public Button btnReady;
    public Image[] scoreDisplay;
    public Sprite[] scoreSprites;

    private Scrollbar myScrollBar;
    private bool timerStarted;
    private float timer;
    private int hitedTimes;

    void Awake() 
    {
        timerStarted = false;
        timer = 30.0f;
        hitedTimes = 0;
        view = GetComponent<PhotonView>();
        

        PhotonNetwork.OnEventCall += this.OnPhotonEvent;

        if(view.isMine)
        {
            if (this.gameObject.name == "AttackPlayer_2(Clone)") {
                this.role = 1;
                this.gameObject.transform.Rotate(0, 180, 0);
                PhotonNetwork.player.SetScore(0);
            } else if (this.gameObject.name == "DefensePlayer_2(Clone)") {
                this.role = 2;
                PhotonNetwork.player.SetScore(6);
            }
        }

        //Récupération du composant de la scrollbar, désactive son interractibilité 
        //Active le controller du roleObject (Correspondant a l'objet 3d joueur ou canon)
        //Et appel de sa methode lui définissant ses parametres
        myScrollBar = scrollBar.GetComponent<Scrollbar>();
        myScrollBar.interactable = false;
        var objSp = roleObject.GetComponent<Transform>().position;
        roleObject.GetComponent<RoleObjController>().enabled = true;
        roleObject.GetComponent<RoleObjController>().SetParams(objSp, myScrollBar, role);
    }

    // Update is called once per frame
    void Update() 
    {
        if(view.isMine) {
            var score = PhotonNetwork.player.GetScore();
            if(this.role == 1) {
                switch(score) {
                    case 0:
                        scoreDisplay[0].sprite = scoreSprites[0];
                        scoreDisplay[1].sprite = scoreSprites[0];
                        scoreDisplay[2].sprite = scoreSprites[0];
                        break;

                    case 2:
                        scoreDisplay[0].sprite = scoreSprites[1];
                        scoreDisplay[1].sprite = scoreSprites[0];
                        scoreDisplay[2].sprite = scoreSprites[0];
                        break;

                    case 4:
                        scoreDisplay[0].sprite = scoreSprites[1];
                        scoreDisplay[1].sprite = scoreSprites[1];
                        scoreDisplay[2].sprite = scoreSprites[0];
                        break;

                    case 6:
                        scoreDisplay[0].sprite = scoreSprites[1];
                        scoreDisplay[1].sprite = scoreSprites[1];
                        scoreDisplay[2].sprite = scoreSprites[1];
                        break;
                }
            } else if (this.role == 2) {
                switch(score) {
                    case 0:
                        scoreDisplay[0].sprite = scoreSprites[0];
                        scoreDisplay[1].sprite = scoreSprites[0];
                        scoreDisplay[2].sprite = scoreSprites[0];
                        break;

                    case 2:
                        scoreDisplay[0].sprite = scoreSprites[1];
                        scoreDisplay[1].sprite = scoreSprites[0];
                        scoreDisplay[2].sprite = scoreSprites[0];
                        break;

                    case 4:
                        scoreDisplay[0].sprite = scoreSprites[1];
                        scoreDisplay[1].sprite = scoreSprites[1];
                        scoreDisplay[2].sprite = scoreSprites[0];
                        break;

                    case 6:
                        scoreDisplay[0].sprite = scoreSprites[1];
                        scoreDisplay[1].sprite = scoreSprites[1];
                        scoreDisplay[2].sprite = scoreSprites[1];
                        break;
                }
            }
            
        }
        
        if(timerStarted == true)
        {
            timer -= Time.deltaTime;
        }
        txtTimer.text = System.Math.Round(timer,0).ToString();

        //lorsque le timer atteinds 0 secondes, appel RPC à tous les joueurs
        //de la methode de fin de partie
        if(timer <= 0.0f && hitedTimes < 6)
        {
            if (view.isMine && this.role == 1) {
                PhotonNetwork.player.SetScore(-10);
            } else if (view.isMine && this.role == 2) {
                PhotonNetwork.player.SetScore(10);
            }
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.GameOver, 0, true, options);
        }


        if(timerStarted && hitedTimes >= 6)  {
            if (view.isMine && this.role == 1) {
                PhotonNetwork.player.SetScore(10);
            } else if (view.isMine && this.role == 2) {
                PhotonNetwork.player.SetScore(-10);
            }
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.GameOver, 0, true, options);
        }
    }

    private void OnPhotonEvent(byte eventCode, object content, int senderID)
    {

        PhotonEventCodes code = (PhotonEventCodes)eventCode;
    
        if(code == PhotonEventCodes.Scored) {
            if(this.role == 1 && view.isMine) {
                var score = PhotonNetwork.player.GetScore();
                PhotonNetwork.player.SetScore((score) +1);
            } else if (this.role == 2 && view.isMine) {
                var score = PhotonNetwork.player.GetScore();
                PhotonNetwork.player.SetScore((score) -1);
            }
            hitedTimes++;
        }

        if (code == PhotonEventCodes.GameOver)
        {
            PhotonNetwork.LoadLevel("GameOver_OnlineGame_2");
        }

        if (code == PhotonEventCodes.StartGame) {
            PhotonNetwork.player.SetScore(0);
            panelReady.SetActive(false);
            StartCoroutine(this.Wait());
        }
    }

    public void OnBtnReady()
    {
        if(view.isMine) {
            btnReady.interactable = false;
            btnReady.GetComponentInChildren<Text>().text = "Prêt !";
            PhotonNetwork.player.SetScore(-44);
            view.RPC("ReadyGame2", PhotonTargets.MasterClient);
        }
    }

    public IEnumerator Wait()
    {
        txtGameStart.text = "Pret ?";
        yield return new WaitForSeconds(2);
        txtGameStart.text = "3";
        yield return new WaitForSeconds(1);
        txtGameStart.text = "2";
        yield return new WaitForSeconds(1);
        txtGameStart.text = "1";
        yield return new WaitForSeconds(1);
        txtGameStart.text = "";
        timerStarted = true;
        myScrollBar.interactable = true;
        if(role == 1)
        {
            roleObject.GetComponent<RoleObjController>().StartShoot();
        }
    }

    [PunRPC]
    void ReadyGame2()
    {
        int nbReady = 0;

        foreach (PhotonPlayer player in PhotonNetwork.playerList) {
            if(player.GetScore() == -44) {
                nbReady ++;
            }
        }

        if(nbReady == 2) {
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.StartGame, 0, true, options);
        }
    }
}
