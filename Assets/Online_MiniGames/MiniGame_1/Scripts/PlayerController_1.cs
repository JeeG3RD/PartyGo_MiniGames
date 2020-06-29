using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PlayerController_1 : MonoBehaviour
{
    public Text txtPlayerScore, txtOpponentScore, txtTimer, txtStartGame;
    public GameObject panelStart, panelReady;
    public Button btnReady;
    private int choiceNeed = 0, choiceIndex;
    public Sprite bBlue, bRed, rBlue, rRed, scoredIndicator, failedIndicator;
    public Image imgChoice, imgSuccesIndicator;
    private int[] tabChoices;
    private float timer = -1.0f;
    public bool timerStarted;
    private int score = 0;
    private PhotonView view;
    private PhotonPlayer otherPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        //PhotonNetwork.OnEventCall -= this.OnPhotonEvent;

        view = GetComponent<PhotonView>();
        timer = 30.0f;
        timerStarted = false;
        tabChoices = new int[150];
        imgSuccesIndicator.enabled = false;
        
        //Si la photonView "m'appartient"
        if(view.isMine)
        {
            //Récupère le photonPlayer de l'adversaire dans une variable
            foreach(PhotonPlayer player in PhotonNetwork.playerList)
            {
                if(player.ID != PhotonNetwork.player.ID)
                {
                    otherPlayer = player;
                    break;
                }
            }

            //Affichge du score du joueur, de l'adversaire et du timer
            txtPlayerScore.text = PhotonNetwork.player.GetScore().ToString();
            txtOpponentScore.text = otherPlayer.NickName + " : " + otherPlayer.GetScore().ToString();
            txtTimer.text = timer.ToString();
        
            //Si le joueur éxécutant le code est le "masterclient"
            //Initialisation de 150 "choix" dans un objet envoyé en 
            //paramètre d'un event Photon, destiné à tous les joueurs
            if(PhotonNetwork.player.IsMasterClient)
            {
                object[] datas = new object[150];
                for(int i=0; i<150; i++)
                {
                    //Random de 1(inclus) à 5(exclus)
                    //1 à 4 correspond aux 4 images pouvant etre affichées
                    //aux joueurs 
                    datas[i] = Random.Range(1,5);
                }

                //Définition de l'option de l'event pour que tous les joueurs le reçoivent
                RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.TabChoiceFilling, datas, true, options);
            }
            PhotonNetwork.OnEventCall += this.OnPhotonEvent;
        }

    }


    //Méthode appelée lors d'un event Photon
    //l'eventCode permet d'éxecuter différentes actions
    private void OnPhotonEvent(byte eventCode, object content, int senderID)
    {

        PhotonEventCodes code = (PhotonEventCodes)eventCode;
        //evenement de recuprération de l'array des choix des joueurs
        //dans tabChoices


        if(code == PhotonEventCodes.TabChoiceFilling)
        {
            object[] datas = content as object[];
            if(datas.Length == 150)
            {
                
                for(int i=0; i<150; i++)
                {
                    tabChoices[i] = (int)datas[i];
                }
                //this.StartCoroutine(this.Wait());
            }
        }

        if(code == PhotonEventCodes.StartGame) {
            PhotonNetwork.player.SetScore(0);
            panelReady.SetActive(false);
            this.StartCoroutine(this.Wait());
        }

        //Event lorsqu'un joueur marque un point
        //Actualise le score joueur + adverse
        if(code == PhotonEventCodes.UpdateScores)
        {
            txtPlayerScore.text = PhotonNetwork.player.GetScore().ToString();
            txtOpponentScore.text = otherPlayer.NickName + " : " + otherPlayer.GetScore().ToString();
        }

        if(code == PhotonEventCodes.GameOver)
        {
            PhotonNetwork.LoadLevel("GameOver_1");
        }
    }

    //méthode appelée à chaque frame
    void Update()
    {

        //si le timer est démarré, on le décrémente
        if(timerStarted == true)
        {
            timer -= Time.deltaTime;
        }
        txtTimer.text = System.Math.Round(timer,0).ToString();

        //lorsque le timer atteinds 0 secondes, appel RPC à tous les joueurs
        //de la methode de fin de partie
        if(timer <= 0.0f)
        {
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.GameOver, 0, true, options);
        }
    }

    //Méthode appellée à l'appui de la couleur bleue
    public void OnBlueClick()
    {
        //On vérifie si la vue photonView est bien à moi
        //Permet de s'assurer qu'on évite d'incrémenter le score de l'adversaire
        if(view.isMine)
        {
            //Si le choix est le bon
            if(choiceNeed == 1 || choiceNeed == 2)
            {
                //On appelle la méthode d'ajout d'un point
                this.Scored();
            }
            //sinon le score du joueur est défini à -1
            //et appel rpc de GameOver
            else
            {
                this.Failed();
            }
        }
    }

    //Méthode appellée à l'appui de la couleur rouge
    //Identique à celle de la couleur bleue
    public void OnRedClick()
    {
        if(view.isMine)
        {
            if(choiceNeed == 3 || choiceNeed == 4)
            {
                this.Scored();
            }
            else
            {
                this.Failed();
            }
        }
    }

    //Méthode appelée lorsque le joueur marque un point
    private void Scored()
    {
        
        if(view.isMine)
        {
            //On récupère le score actuel du joueur et on l'incrémente d'un point
            score = PhotonNetwork.player.GetScore();
            PhotonNetwork.player.SetScore(score+1);

            //Appel de l'évent avec un code 2 pour actualiser le score chez tout l'adversaire
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateScores, 0, true, options);
            choiceIndex ++;
            //Récupération du choix suivant
            StopCoroutine(SuccessIndicatorTime());
            imgSuccesIndicator.sprite = scoredIndicator;
            imgSuccesIndicator.enabled = true;
            StartCoroutine(SuccessIndicatorTime());
            GetNeededChoice();
        }
    }    

    private void Failed()
    {
        //Si la view m'appartient et que mon score est suppérieur à 0
        //Enleve -1 au score joueur
        //Appelle le RaiseEvent d'actualisation des scores
        if(view.isMine && PhotonNetwork.player.GetScore() > 0)
        {
            score = PhotonNetwork.player.GetScore();
            PhotonNetwork.player.SetScore(score-1);
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateScores, 0, true, options);
        }
        choiceIndex++;
        StopCoroutine(SuccessIndicatorTime());
        imgSuccesIndicator.sprite = failedIndicator;
        imgSuccesIndicator.enabled = true;
        StartCoroutine(SuccessIndicatorTime());
        GetNeededChoice();
    }


    IEnumerator SuccessIndicatorTime()
    {
        yield return new WaitForSeconds(0.5f);
        imgSuccesIndicator.enabled = false;
    }

    
    private void GetNeededChoice()
    {
        //On récupère dans le tableau le choix suivant le score du joueur
        choiceNeed = tabChoices[choiceIndex];
        
        //En fonction de ce qu'on a reçu, on affiche l'image correspondante au joueur
        switch(choiceNeed)
        {
            case 1:
                this.imgChoice.sprite = bBlue; // "B" écris en bleu
                break;
            case 2:
                this.imgChoice.sprite = bRed; // "b" écris en rouge
                break;
            case 3:
                this.imgChoice.sprite = rBlue; // "R" écris en bleu
                break;
            case 4:
                this.imgChoice.sprite = rRed; // "r" écris en rouge
                break;
        }
    }

    public void OnBtnReady(){
        if (view.isMine == true) {
            PhotonNetwork.player.SetScore(-44);
            btnReady.interactable = false;
            btnReady.GetComponentInChildren<Text>().text = "PRET !";
            view.RPC("Ready", PhotonTargets.MasterClient);
        }
    }

    public IEnumerator Wait()
    {
        txtStartGame.text = "Pret ?";
        yield return new WaitForSeconds(2);
        txtStartGame.text = "3";
        yield return new WaitForSeconds(1);
        txtStartGame.text = "2";
        yield return new WaitForSeconds(1);
        txtStartGame.text = "1";
        yield return new WaitForSeconds(1);
        txtStartGame.text = "";
        choiceIndex = 0;
        this.GetNeededChoice();
        panelStart.SetActive(false);
        timerStarted = true;
    }

    [PunRPC]
    void Ready()
    {
        if(PhotonNetwork.isMasterClient) {
            int count = 0;

            foreach(PhotonPlayer player in PhotonNetwork.playerList) {
                if(player.GetScore() == -44) {
                    count++;
                }
            }

            if(count == 2) {
                RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.StartGame, 0, true, options);
            }
        }
    }
    
}
