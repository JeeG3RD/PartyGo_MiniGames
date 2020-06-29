using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_4 : MonoBehaviour
{
    public GameObject playerCharacter, panelReady;
    public Button btnReady;
    private PhotonView view;
    public Text txtGameStart;
    private bool isStarted, canjump;
    public float gravForce;
    Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        this.isStarted = false;
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gravForce < 1) gravForce += Time.deltaTime*10;
        if(gravForce > 1) gravForce = 1;
        rb.velocity -= new Vector3(0, gravForce, 0);
        if(rb.velocity.y > 70) rb.velocity = new Vector3(0,70,0);
    }

    private void OnPhotonEvent(byte eventCode, object content, int senderID)
    {

        PhotonEventCodes code = (PhotonEventCodes)eventCode;

        if(code == PhotonEventCodes.StartGame) {
            PhotonNetwork.player.SetScore(0);
            panelReady.SetActive(false);
            StartCoroutine(this.Wait());
        }
    }

    //methode appelee lors de l'appui sur l'ecran pendant la partie
    public void OnScreenTap(){
        if(view.isMine && this.isStarted == true && this.canjump) {
            rb.velocity = new Vector3(0, 25, 0);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "game4JumpZone") {
            this.canjump = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "game4JumpZone") {
            this.canjump = false;
        }
    }

    public void OnBtnReady()
    {
        if(view.isMine) {
            btnReady.interactable = false;
            btnReady.GetComponentInChildren<Text>().text = "Prêt !";
            PhotonNetwork.player.SetScore(-44);
            view.RPC("ReadyGame4", PhotonTargets.MasterClient);
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
        this.isStarted = true;
        this.canjump = true;
        GameObject.Find("Tourelle").GetComponent<ObstacleRotation>().rotate = true;
    }

    public void Loose()
    {
        if(view.isMine) {
            PhotonNetwork.player.SetScore(-10);
            view.RPC("Win_4", PhotonTargets.Others);
            PhotonNetwork.LoadLevel("GameOver_OnlineGame_4");
        }
    }

    [PunRPC]
    void ReadyGame4()
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
    public void Win_4()
    {
        PhotonNetwork.player.SetScore(10);
        PhotonNetwork.LoadLevel("GameOver_OnlineGame_4");
    }
}
