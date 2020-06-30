using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    // Start is called before the first frame update

    public GameObject readyPanel, atkPanel, defPanel;
    public Button readyBtn, btnConfirm;
    public GameObject[] results1, results2, results3, results4, results5;
    public Text txtLife;
    private int[] tempCodeValues = new int[5];
    private int codeIndex;
    private bool codeInputStarted;

    [SyncVar]
    private bool ready, codeConfirmed;

    [SyncVar]
    private int role;

    [SyncVar]
    public int life;

    [SyncVar]
    private int codeVal1, codeVal2, codeVal3, codeVal4, codeVal5;

    [SyncVar]
    private string playerName;


    /**
    * MÉTOHDE START
    *
    * Méthode appelée à l'initialisation du GameObject sur lequel le script est attaché
    */
    void Start()
    {
        //Si le script est éxécuté du côté local et non depuis le serveur ou un client distant
        //On initialise les différentes variables nécessaires
        if (this.isLocalPlayer) {
            this.role = 0;
            this.codeIndex = 0;
            this.codeInputStarted = false;

            //On définit le nombre de PV du joueur à 3
            //Puis on appelle la méthode permettant de synchroniser cette valeur sur l'instance du joueur côté serveur
            this.life = 3;
            CmdSendLife(life);

            //On récupère le pseudo du joueur
            //Puis on appelle la méthode de synchronisation avec l'instance côté serveur 
            this.playerName = "Joueur " + Random.Range(1, 10000).ToString();
            CmdsetPlayerName(this.playerName);
            
            //On s'assure que le bouton de 
            this.btnConfirm.interactable = false;

            //On s'assure que seul le paneau  d'attente est actif
            this.defPanel.SetActive(false);
            this.atkPanel.SetActive(false);
            this.readyPanel.SetActive(true);
            this.readyPanel.GetComponentInChildren<Text>().text = "Prêt ?";

            //Enfin on active la Caméra et le Canva permettant l'affichage de l'écran joueur
            this.GetComponentInChildren<Camera>().enabled = true;
            this.GetComponentInChildren<Canvas>().enabled = true;
        }

        this.txtLife.text = "";
    }

    /**
    * MÉTHODE ON BTN CODE
    *
    * Méthode appelée lors de l'appui sur un bouton de code
    */
    public void onBtnCode(int code)
    {
        //Il faut que la saisie du code soit défini comme "démarrée" et que le script s'exécute sur le joueur local
        if (this.codeInputStarted && this.isLocalPlayer) {
            //En fonction de l'avancement de la saisie du code, on défini la valeur correspondant à l'index
            switch(this.codeIndex) {
                case 1:
                    results1[code-1].SetActive(true);
                    break;

                case 2:
                    results2[code-1].SetActive(true);
                    break;

                case 3:
                    results3[code-1].SetActive(true);
                    break;

                case 4:
                    results4[code-1].SetActive(true);
                    break;

                case 5:
                    results5[code-1].SetActive(true);
                    break;

                default:
                    break;
            }

            //On insère dans un tableau "temporaire" la nouvelle valeur
            //Ce tableau sera utilisé pour synchroniser les valeurs vers l'instance côté serveur
            this.tempCodeValues[this.codeIndex-1] = (int)code;

            //si le code n'est pas encore complet, on continue
            //Sinon on arrete la saisie du code et on rends le bouton de validation cliquable 
            if (this.codeIndex <= 5) {
                this.codeIndex++;
            } else {
                this.btnConfirm.interactable = true;
                this.codeInputStarted = false;
            }
        }
    }

    /**
    * MÉTHODE ON BTN CONFIRM
    *
    * Méthode appelée lors de l'appui sur le bouton de validation du code
    */
    public void onBtnConfirm()
    {
        //On rends le bouton de validation non cliquable
        this.btnConfirm.interactable = false;
        
        //Puis on synchronise le code vers l'instance joueur du côté serveur à l'aide du tableau tempCodeValues
        CmdSendCodeValues(tempCodeValues);
    }


    /**
    * MÉTHODE SET READY
    *
    * Méthode appelée lors de l'appuis sur le bouton ready
    * 
    * Définit une nouvelle valeur à ready et la synchronise avec l'instance du côté serveur
    */
    public void setReady()
    {
        if (this.isLocalPlayer) {
            this.ready = !ready;

            //On appelle la méthode de synchronisation vers le serveur avec en paramètre la valeur inverse de ready
            CmdSyncReady(ready);

            //Puis on met à jour le texte du bouton en fonction du statut ready
            if (ready) {
                this.readyPanel.GetComponentInChildren<Text>().text = "Prêt !";
            } else {
                this.readyPanel.GetComponentInChildren<Text>().text = "Prêt ?";
            }
        }
    }

    /**
    * MÉTHOD GET READY
    *
    * Retourne la valeur de ready
    */
    public bool getReady()
    {
        return ready; 
    }

    private void setPlayerName(string name)
    {
        this.playerName = name;
    }

    public string getPlayerName()
    {
        return this.playerName;
    }

    private void setLife(int pv)
    {
        this.life = pv;
    }

    public int getLife()
    {
        return this.life;
    }

    private void setRole(int idRole)
    {
        this.role = idRole;
    }

    public int getRole()
    {
        return this.role;
    }

    public bool getCodeConfirmed()
    {
        return this.codeConfirmed;
    }

    public int getCodeValues(int index)
    {
        switch (index) {
            case 1:
                return codeVal1;
            
            case 2:
                return codeVal2;

            case 3:
                return codeVal3;

            case 4:
                return codeVal4;

            case 5:
                return codeVal5;

            default:
                return -1;
        }
    }


    [ClientRpc]
    public void RpcSetRole(int newRole)
    {
        this.txtLife.text = "Points de vie : " + life;
        
        this.role = newRole;

        switch (role) {
            case 1:
                this.readyPanel.SetActive(false);
                this.atkPanel.SetActive(true);
                this.defPanel.SetActive(false);
                break;

            case 2:
                this.readyPanel.SetActive(false);
                this.atkPanel.SetActive(false);
                this.defPanel.SetActive(true);
                break;

            default:
                this.readyPanel.SetActive(true);
                this.atkPanel.SetActive(false);
                this.defPanel.SetActive(false);
                break;
        }
    }

    [ClientRpc]
    public void RpcactiveReadyPanel()
    {
        this.readyPanel.SetActive(true);
    }

    [ClientRpc]
    public void RpcSetCodeConfirmed(bool value)
    {
        this.codeConfirmed = value;
        CmdSetCodeConfirmed(value);
    }

    [Command]
    public void CmdSetCodeConfirmed(bool value)
    {
        this.codeConfirmed = value;
    }

    [ClientRpc]
    public void RpcStartCodeInput()
    {
        this.codeIndex = 1;
        this.codeInputStarted = true;
        this.btnConfirm.interactable = false;
        this.codeConfirmed = false;
        
        for (int i = 0; i<5; i++)
        {
            this.tempCodeValues[i] = 0;
        }
    }

    [ClientRpc]
    public void RpcRemoveLifePoint(int pointToRemove)
    {
        this.life -= pointToRemove;
        this.txtLife.text = "Points de vie : " + this.life;
        CmdSendLife(this.life);
    } 
    
    [ClientRpc]
    public void RpcResetPanels()
    {
        results1[tempCodeValues[0]-1].SetActive(false);
        results2[tempCodeValues[1]-1].SetActive(false);
        results3[tempCodeValues[2]-1].SetActive(false);
        results4[tempCodeValues[3]-1].SetActive(false);
        results5[tempCodeValues[4]-1].SetActive(false);
    }


    [Command]
    void CmdsetPlayerName(string name)
    {
        playerName = name;
    }



    /**
    * MÉTHODE CmdSyncReady
    */

    [Command]
    void CmdSyncReady(bool value) {
        ready = value; 
    }

    [Command]
    void CmdSendCodeValues(int[] values)
    {
        codeVal1 = values[0];
        codeVal2 = values[1];
        codeVal3 = values[2];
        codeVal4 = values[3];
        codeVal5 = values[4];
        codeConfirmed = true;
    }

    [Command]
    public void CmdSendLife(int pv)
    {
        life = pv;
    }
}
