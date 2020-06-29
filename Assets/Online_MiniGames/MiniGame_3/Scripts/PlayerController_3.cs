using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_3 : MonoBehaviour
{

    public GameObject player;
    public Text txtGameStart;
    public float shootForce = 75.0f;
    public GameObject laserPrefab, panelReady;
    public Button btnReady;

    private PhotonView view;
    private bool isStarted;
    // Start is called before the first frame update
    void Awake()
    {
        this.isStarted = false;
        view = GetComponent<PhotonView>();
        PhotonNetwork.OnEventCall += this.OnPhotonEvent;
        //PhotonNetwork.player.SetScore(0);
    }

    // Update is called once per frame
    void Update()
    {

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

    public void OnClick()
    {
        if(view.isMine && this.isStarted == true) {
            var direction = new Vector3(-1,0,0);
            player.GetComponent<Rigidbody>().AddForce(direction * 2, ForceMode.VelocityChange);
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

        GameObject go;
        var laserSpawn = new Vector3(4.00f, 0.80f, 2.00f);
        var direction = new Vector3(-3.5f,0,0);
        go = Instantiate(laserPrefab, laserSpawn, Quaternion.identity);
        go.GetComponent<Transform>().Rotate(-90f,0,0);
        if(view.isMine) {
            go.GetComponent<Rigidbody>().AddForce(direction * shootForce);
        }
    }

    public void OnBtnReady()
    {
        if(view.isMine) {
            btnReady.interactable = false;
            btnReady.GetComponentInChildren<Text>().text = "Pret !";
            PhotonNetwork.player.SetScore(-44);
            view.RPC("ReadyGame3", PhotonTargets.MasterClient);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.tag);
        if(other.gameObject.tag == "FinishLine" && view.isMine) {
            PhotonNetwork.player.SetScore(10);

            view.RPC("GameOver", PhotonTargets.All);
        } else if (other.gameObject.tag == "Laser" && view.isMine) {
            PhotonNetwork.player.SetScore(-10);
            view.RPC("GameOver", PhotonTargets.All);
        }
    }

    [PunRPC]
    void ReadyGame3()
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
    void GameOver()
    {
        PhotonNetwork.LoadLevel("GameOver_OnlineGame_3");
    }
}
