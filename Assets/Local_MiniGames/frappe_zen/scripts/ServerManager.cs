using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Net;
using System.Net.Sockets; 

public class ServerManager : NetworkBehaviour
{
    // Start is called before the first frame update
    GameObject netManager;
    public Text ipText, waitingText;
    public InputField ipField_1, ipField_2, ipField_3, ipField_4;

    public Canvas serverConnect, clientConnect;

    public GameObject playerPrefab, panelResults;

    private GameObject atkPlayer, defPlayer;

    private bool isMyServer, isPlayersSpawned, gameStarted;

    private int countReadyPlayers;
    private bool waitingReadyState;

    private bool atkValid, defValid;
    public int maxPlayers;

    public GameObject[] players, results1, results2, results3, results4, results5, response1, response2, response3, response4, response5, okTicks, wrongTicks;

    private bool txtPlayersConnected, txtPlayersPreparation; 

    //private NetworkPlayer[] playersList;
    
    void Awake()
    {
        isMyServer = (GameObject.Find("Master(Clone)")) ? true : false;
        isPlayersSpawned = false;
        waitingReadyState = false;
        gameStarted = false;
        txtPlayersConnected = false;
        txtPlayersPreparation = false;
        this.countReadyPlayers = 0;
        this.atkValid=false;
        this.defValid =false;
        ipText.text = "";
        if (isMyServer) {
            gameObject.GetComponent<NetworkManager>().networkAddress = this.getIP();
            gameObject.GetComponent<NetworkManager>().StartServer();
            ipText.text = this.getIP();
            waitingText.text = "EN ATTENTE DES JOUEURS... (0/"+maxPlayers+")";
            serverConnect.enabled = true;
            clientConnect.enabled = false;
        } else {
            serverConnect.enabled = false;
            clientConnect.enabled = true;
        }
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {  
        if (isMyServer && NetworkServer.connections.Count < maxPlayers) {
            //Si tous les joueurs ne sont pas connectés, on affiche un message d'attente
            waitingText.text = "EN ATTENTE DES JOUEURS... (" + NetworkServer.connections.Count.ToString()+"/"+maxPlayers+")";
        } else if (NetworkServer.connections.Count >= maxPlayers && waitingReadyState == false) {
            //Sinon on lance l'attente des joueurs prêts 

            //On récupère tous les GameObject des joueurs
            players = GameObject.FindGameObjectsWithTag("Player");

            //On appelle la méthode affichant le panneau avec le bouton "pret"
            foreach (GameObject playerObj in players) {
                playerObj.GetComponent<PlayerManager>().RpcactiveReadyPanel();
            }
            
            if (!txtPlayersConnected) {
                waitingText.text = "TOUS LES JOUEURS SONT CONNECTÉS !";
                txtPlayersConnected = true;
            }
            ipText.text = "";

            if (!txtPlayersPreparation) {
                StartCoroutine(printAfterDelay(waitingText, "Préparation des joueurs", 3.0f));
                txtPlayersPreparation = true;
            }

            //Définition de la valeur waitingReadyState à true pour lancer l'attente des joueurs
            StartCoroutine(setWaitingReadyStateAfterDelay(true, 9.0f));
        }

        
        if (waitingReadyState == true && countReadyPlayers < maxPlayers) {
            foreach (GameObject playerObj in players) {
                if (playerObj.GetComponent<PlayerManager>().getReady() == true) {
                    countReadyPlayers++;
                    continue;
                } else if (playerObj.GetComponent<PlayerManager>().getReady() == false && countReadyPlayers > 0) {
                    countReadyPlayers--;
                    continue;
                }

            }
            waitingText.text = "Joueurs prêts : " + countReadyPlayers + " / " + maxPlayers;

        } else if (waitingReadyState == true && countReadyPlayers >= maxPlayers) {
           
            if (this.gameStarted == false) {
                waitingText.text = "Joueurs prêts !";

                int startId = randomPlayerStart();

                for (int i=0; i<maxPlayers; i++) {
                    if (i == startId) {
                        atkPlayer = players[i];
                        atkPlayer.GetComponent<PlayerManager>().RpcSetRole(1);
                        atkPlayer.GetComponent<PlayerManager>().RpcStartCodeInput();
                    } else {
                        defPlayer = players[i];
                        defPlayer.GetComponent<PlayerManager>().RpcSetRole(2);
                    }
                }

                StartCoroutine(printAfterDelay(waitingText, atkPlayer.GetComponent<PlayerManager>().getPlayerName() + " commence !", 2.5f));
                StartCoroutine(printAfterDelay(waitingText, "", 1.5f));
            }

            this.gameStarted = true;
        }

        if (this.gameStarted == true) {
            if (atkPlayer.GetComponent<PlayerManager>().getCodeConfirmed() == true && atkValid == false) {
                atkValid = true;
                atkPlayer.GetComponent<PlayerManager>().RpcSetCodeConfirmed(false);
                displayAttackCode();
            }

            if (defPlayer.GetComponent<PlayerManager>().getCodeConfirmed() == true && defValid == false) {
                defValid = true;
                defPlayer.GetComponent<PlayerManager>().RpcSetCodeConfirmed(false);
                displayDefResult();
            }
        }
    }

    /**
    * Méthode displayAttackCode
    *
    * Méthode lançant l'affichage du code du joueur "meneur"
    */
    private void displayAttackCode()
    {
        StartCoroutine(printAfterDelay(waitingText, "Code à reproduire : ", 1.0f));
        StartCoroutine(printAfterDelay(waitingText, "", 4.0f));
    
        int[] values = new int[5];

        for (int i=1; i<=5; i++) {
            values[i-1] = atkPlayer.GetComponent<PlayerManager>().getCodeValues(i);
        }

        StartCoroutine(DisplayFullCodeDelay(4.5f, 3.0f, values));
        StartCoroutine(changeRolesDelay(2, 1, 8.0f));
    }

    /**
    * Méthode displayDefResult
    *
    * Méthode lançant l'affichage de la réponse du joueur "suiveur"
    */
    private void displayDefResult()
    {
        Debug.Log("afficher les résultats");
        int[] values = new int[5];
        int[] responses = new int[5];

        for (int i=1; i<=5; i++) {
            values[i-1] = atkPlayer.GetComponent<PlayerManager>().getCodeValues(i);
            responses[i-1] = defPlayer.GetComponent<PlayerManager>().getCodeValues(i);
        }

        StartCoroutine(DisplayFullCodeDelay(0.5f, 0.0f, values));
        StartCoroutine(DisplayResponses(values, responses));
    }

    /**
    * Méthode getIP
    *
    * Méthode permettant à l'hôte de connaître son adresse Ip sur le réseau
    *
    */
    private string getIP()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

    public void OnBtnConnect()
    {
        string url = ipField_1.text + "." + ipField_2.text + "." + ipField_3.text + "." + ipField_4.text;
        gameObject.GetComponent<NetworkManager>().networkAddress = url;
        gameObject.GetComponent<NetworkManager>().StartClient();
    }


    public void OnServerConnect() {
        Debug.Log("JOUEUR CONNECTÉ");
    }

    private IEnumerator<WaitForSeconds> printAfterDelay(Text txt, string textToPrint, float delay)
    {
        yield return new WaitForSeconds(delay);
        txt.text = textToPrint;
    }

    /**
    * Méthode setWaitingReadyStateAfterDelay
    *
    * Méthode définissant une nouvelle valeur à waitingReadyState après un délai défini
    *
    * @param newValue : [bool] Nouvelle valeur à définir
    * 
    * @param delay : [float] delay d'attente avant de changer la valeur 
    *
    */
    private IEnumerator<WaitForSeconds> setWaitingReadyStateAfterDelay(bool newValue, float delay)
    {
        yield return new WaitForSeconds(delay);
        this.waitingReadyState = newValue;
    }

    private IEnumerator<WaitForSeconds> DisplayFullCodeDelay(float beforDelay, float afterDelay, int[] tabValues)
    {
        yield return new WaitForSeconds(beforDelay);
        this.results1[tabValues[0]-1].active = true;
        this.results2[tabValues[1]-1].active = true;
        this.results3[tabValues[2]-1].active = true;
        this.results4[tabValues[3]-1].active = true;
        this.results5[tabValues[4]-1].active = true;
        if (afterDelay > 0.0f) {
            yield return new WaitForSeconds(afterDelay);
            this.results1[tabValues[0]-1].active = false;
            this.results2[tabValues[1]-1].active = false;
            this.results3[tabValues[2]-1].active = false;
            this.results4[tabValues[3]-1].active = false;
            this.results5[tabValues[4]-1].active = false;
        }
    }

    /**
    * Méthode d'affichage des réponse du joueur "suiveur"
    */
    private IEnumerator<WaitForSeconds> DisplayResponses(int[] tabValues, int[] tabResponses)
    {
        int countErrors = 0;
        
        //On commence par afficher un text
        yield return new WaitForSeconds(0.5f);
        this.waitingText.text = "Résultats !";
        yield return new WaitForSeconds(1.5f);
        this.waitingText.text = "";


        this.response1[tabResponses[0]-1].active = true;

        if (tabValues[0] != tabResponses[0]) {
            countErrors++;
            wrongTicks[0].active = true;
        } else {
            okTicks[0].active = true;
        }

        yield return new WaitForSeconds(1.5f);

        this.response2[tabResponses[1]-1].active = true;

        if (tabValues[1] != tabResponses[1]) {
            wrongTicks[1].active = true;
            countErrors++;
        } else {
            okTicks[1].active = true;
        }

        yield return new WaitForSeconds(1.5f);

        this.response3[tabResponses[2]-1].active = true;

        if (tabValues[2] != tabResponses[2]) {
            wrongTicks[2].active = true;
            countErrors++;
        } else {
            okTicks[2].active = true;
            okTicks[2].active = true;
        }

        yield return new WaitForSeconds(1.5f);

        this.response4[tabResponses[3]-1].active = true;

        if (tabValues[3] != tabResponses[3]) {
            wrongTicks[3].active = true;
            countErrors++;
        } else {
            okTicks[3].active = true;
        }

        yield return new WaitForSeconds(1.5f);

        this.response5[tabResponses[4]-1].active = true;

        if (tabValues[4] != tabResponses[4]) {
            wrongTicks[4].active = true;
            countErrors++;
        } else {
            okTicks[4].active = true;
        }

        yield return new WaitForSeconds (2.0f);
        this.results1[tabValues[0]-1].active = false;
        this.response1[tabResponses[0]-1].active = false;
        this.results2[tabValues[1]-1].active = false;
        this.response2[tabResponses[1]-1].active = false;
        this.results3[tabValues[2]-1].active = false;
        this.response3[tabResponses[2]-1].active = false;
        this.results4[tabValues[3]-1].active = false;
        this.response4[tabResponses[3]-1].active = false;
        this.results5[tabValues[4]-1].active = false;
        this.response5[tabResponses[4]-1].active = false;
        
        for (int i =0; i<5 ;i++)
        {
                okTicks[i].active = false;
                wrongTicks[i].active = false;
        }

        yield return new WaitForSeconds (1.5f);

        if (countErrors > 0) {
            this.waitingText.text = defPlayer.GetComponent<PlayerManager>().getPlayerName() + " perd un point de vie !";
            defPlayer.GetComponent<PlayerManager>().RpcRemoveLifePoint(1);
        } else {
            this.waitingText.text = "Correct !";
        }



        if (defPlayer.GetComponent<PlayerManager>().getLife() <= 0) {
            //ToDo : gérer la mort
            Debug.Log("mort");
        } else {
            GameObject tmpDef = defPlayer;
            this.defPlayer = atkPlayer;
            this.atkPlayer = tmpDef;

            yield return new WaitForSeconds (3.0f);

            this.waitingText.text = "Au tour de " + atkPlayer.GetComponent<PlayerManager>().getPlayerName() + " !";

            yield return new WaitForSeconds (2.25f);

            this.waitingText.text = "";

            atkPlayer.GetComponent<PlayerManager>().RpcResetPanels();
            defPlayer.GetComponent<PlayerManager>().RpcResetPanels();
            atkPlayer.GetComponent<PlayerManager>().RpcSetRole(1);
            defPlayer.GetComponent<PlayerManager>().RpcSetRole(2);
            atkPlayer.GetComponent<PlayerManager>().RpcStartCodeInput();
            atkPlayer.GetComponent<PlayerManager>().RpcSetCodeConfirmed(false);
            defPlayer.GetComponent<PlayerManager>().RpcSetCodeConfirmed(false);
            atkValid = false;
            defValid = false;
        }


    }

    private IEnumerator<WaitForSeconds> changeRolesDelay(int newRoleAtk, int newRoleDef, float delay)
    {
        
        yield return new WaitForSeconds(delay);
        waitingText.text = "";
        
        atkPlayer.GetComponent<PlayerManager>().RpcSetRole(newRoleAtk);

        if (newRoleAtk == 1) {
            atkPlayer.GetComponent<PlayerManager>().RpcStartCodeInput();
        }

        defPlayer.GetComponent<PlayerManager>().RpcSetRole(newRoleDef);
        
        if (newRoleDef == 1) {
            defPlayer.GetComponent<PlayerManager>().RpcStartCodeInput();
        }
    }

    /**
    * Méthode randomPlayerStart
    *
    * Méthode retournant un nombre aléatoire permettant de définir quel joueur commence à mener
    *
    */
    private int randomPlayerStart()
    {
        if (isMyServer) {
            return Random.Range(0, maxPlayers);
        } else {
            return -1;
        }
    }

}
