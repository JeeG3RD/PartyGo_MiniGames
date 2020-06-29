using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_5 : MonoBehaviour
{
    private PhotonView view;
    private bool isStarted, canTap;

    public Text txtStart, player1Name, player2Name, txtPlayerScored;
    public Image imgTapIndicator;
    public Image[] scoreDisplay, opponentScoreDisplay;
    public Sprite[] scoreSprites, tapIndicatorSprites;
    public Button btnTap, btnReady;
    public GameObject panelReady;
    public int score;
    // Start is called before the first frame update
    void Awake()
    {
        this.view = GetComponent<PhotonView>();
        this.isStarted = false;
        this.canTap = false;
        this.txtStart.text = "";
        this.score = 0;
        this.btnTap.interactable = false;
        this.imgTapIndicator.enabled = false;

        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            if(player.ID == PhotonNetwork.player.ID) {
                player1Name.text = player.NickName;
            } else {
                player2Name.text = player.NickName;
            }
        }

        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
        //StartCoroutine(this.StartTimer());
    }

    // Update is called once per frame
    void Update()
    {
        if(PhotonNetwork.player.GetScore() >= 10) {
            this.view.RPC("GameOver_5", PhotonTargets.AllBuffered);
        }

        foreach(PhotonPlayer player in PhotonNetwork.playerList)
        {
            if(isStarted) {
                if (player.ID == PhotonNetwork.player.ID ) {
                    for (int i=0; i<player.GetScore(); i+=2) {
                        int x = i /2;
                        scoreDisplay[x].sprite = scoreSprites[1];
                    }
                } else {
                    for (int i=0; i<player.GetScore(); i+=2) {
                        int x = i /2;
                        opponentScoreDisplay[x].sprite = scoreSprites[1];
                    }
                }
            }
        }

        if(this.btnTap.interactable == false && this.isStarted == true)
        {
            this.imgTapIndicator.transform.Rotate(0,0, -70 * Time.deltaTime);
        }

    }

    private void OnPhotonEvent(byte eventCode, object content, int senderID)
    {
        PhotonEventCodes code = (PhotonEventCodes)eventCode;

         if(code == PhotonEventCodes.StartGame) {
            PhotonNetwork.player.SetScore(0);
            panelReady.SetActive(false);
            StartCoroutine(this.StartTimer());
        }

        if (code == PhotonEventCodes.Game_5_CanTap && view.isMine) {
            this.canTap = true;
            this.btnTap.interactable = true;
            this.imgTapIndicator.transform.rotation = Quaternion.Euler(0,0,0);
            this.imgTapIndicator.sprite = tapIndicatorSprites[1];
        }

        if (code == PhotonEventCodes.Game_5_Tap) {
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Game_5_CantTap, 0, true, options);

            object[] datas = content as object[];
            int playerTapID = (int)datas[0];
            if(PhotonNetwork.isMasterClient) {
                StopCoroutine(this.StartTimer());
                PhotonPlayer playerToScore = PhotonPlayer.Find(playerTapID);
                foreach(PhotonPlayer player in PhotonNetwork.playerList) {
                    if(player.IsMasterClient) {
                        int playerScore = playerToScore.GetScore();
                        playerToScore.SetScore(playerScore + 1);
                        object[] playerName = new object[1];
                        playerName[0] = playerToScore.NickName;
                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Game_5_DisplayPlayerSocred, playerName, true, options);
                    }
                }
            }

            if(PhotonNetwork.isMasterClient) {
                Debug.Log("Timer ouvre toi !");
                StartCoroutine(this.ScoredDelay());
            }
        }

        if (code == PhotonEventCodes.Game_5_CantTap) {
            this.imgTapIndicator.sprite = tapIndicatorSprites[0];
            this.imgTapIndicator.enabled = false;
            this.canTap = false;
            this.btnTap.interactable = false;
        }

        if (code == PhotonEventCodes.Game_5_DisplayPlayerSocred) {
            object[] datas = content as object[];
            string playerScored = (string)datas[0];
            this.txtPlayerScored.text = playerScored + " marque le point !";
        }

        if (code == PhotonEventCodes.Game_5_DisplayWaiting) {
            this.txtPlayerScored.text = "";
            this.imgTapIndicator.enabled = true;
        }
    }

    IEnumerator StartTimer()
    {
        this.txtStart.text = "Pret ?";
        yield return new WaitForSeconds(2.0f);
        this.txtStart.text = "3";
        yield return new WaitForSeconds(1.0f);
        this.txtStart.text = "2";
        yield return new WaitForSeconds(1.0f);
        this.txtStart.text = "1";
        yield return new WaitForSeconds(1.0f);
        this.txtStart.text = " ";
        this.isStarted = true;
        this.imgTapIndicator.enabled = true;
        this.imgTapIndicator.sprite = tapIndicatorSprites[0];

        if (PhotonNetwork.isMasterClient == true && view.isMine == true) {
            StartCoroutine(this.TapDelay());
        } 
    }

    IEnumerator TapDelay()
    {
        Debug.Log("Timer presque ouvert !");
        if(PhotonNetwork.isMasterClient == true && this.view.isMine == true && this.canTap == false) {
            float randomTime = Random.Range(5, 13);
            Debug.Log(randomTime);
            yield return new WaitForSeconds(randomTime);
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Game_5_CanTap, 0, true, options);
        }
    }

    IEnumerator ScoredDelay(){
        if(PhotonNetwork.isMasterClient == true && this.view.isMine == true)
        {
            yield return new WaitForSeconds(3);
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Game_5_DisplayWaiting, 0, true, options);
            StartCoroutine(this.TapDelay());
        }
    }

    public void OnTap()
    {
        //TODO : Action du tap button
        if(this.canTap == true && this.view.isMine && this.isStarted == true) {
            this.canTap = false;
            this.btnTap.interactable = false;
            object[] datas = new object[1];
            datas[0] = PhotonNetwork.player.ID;
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.MasterClient};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Game_5_Tap, datas, true, options);
        }
    }

    public void OnBtnReady()
    {
        if(view.isMine) {
            btnReady.interactable = false;
            btnReady.GetComponentInChildren<Text>().text = "Prêt !";
            PhotonNetwork.player.SetScore(-44);
            view.RPC("ReadyGame5", PhotonTargets.MasterClient);
        }
    }

    [PunRPC]
    void ReadyGame5()
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

    [PunRPC]
    void GameOver_5()
    {
        PhotonNetwork.LoadLevel("GameOver_OnlineGame_5");
    }

    [PunRPC]
    void DisplayWaiting()
    {
        
    }
}
