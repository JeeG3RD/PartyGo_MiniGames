using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum PhotonEventCodes
{
    StartGame = 0,
    GameOver = 1,
    Scored = 2,
    UpdateScores = 3,
    TabChoiceFilling = 4,
    RoleDefine = 5,
    Game_5_CanTap = 6,
    Game_5_Tap = 7,
    Game_5_CantTap = 8,
    Game_5_DisplayPlayerSocred = 9,
    Game_5_DisplayWaiting = 10,
}

public class LoadingConnect : MonoBehaviour
{
    private string miniGame, randomRoom, playerName;

    private bool isLocalGame;

    public bool isMaster;

    private Dictionary<string, string> gameScenes = new Dictionary<string, string>();

    public GameObject MasterPrefab;

    void Awake()
    {
        /**
        * Définition des noms de scènes a charger pour chaque mini jeu
        */

        gameScenes["OnlineGame1"] = "Game_1";
        gameScenes["OnlineGame2"] = "Game_2";
        gameScenes["OnlineGame3"] = "Game_3";
        gameScenes["OnlineGame4"] = "Game_4";
        gameScenes["OnlineGame5"] = "Game_5";
        gameScenes["FrappeZen"] = "frappe_zen_game";

        //On récupère l'intent reçu dans l'activité, puis on verifie s'il contient les extra nécessaires à la connexion
        /*AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        var hasGameExtra = intent.Call<bool> ("hasExtra", "miniGame");
        var hasRoomExtra = intent.Call<bool> ("hasExtra", "randomRoom");
        var hasUserExtra = intent.Call<bool> ("hasExtra", "username");
        var hasMasterExtra = intent.Call<bool> ("hasExtra", "isMaster");
        var hasLocalExtra = intent.Call<bool> ("hasExtra", "isLocal");

        //Si les extras sont présents dans l'intent
        if(hasGameExtra && hasRoomExtra && hasUserExtra && hasMasterExtra && hasLocalExtra) {
            //On les récupère dans les variables appropriées
            var extras = intent.Call<AndroidJavaObject> ("getExtras");
            miniGame = extras.Call<string> ("getString", "miniGame");
            randomRoom = extras.Call<string>("getString", "randomRoom");
            playerName = extras.Call<string>("getString", "username");
            isLocalGame = extras.Call<bool>("getBool", "isLocal");
            isMaster = extras.Call<bool>("getBool", "isMaster");
        }
        //Sinon on quitte l'application
        else {
            Application.Quit();
        }*/

        isLocalGame = true;

        if (isLocalGame) {
            miniGame = "FrappeZen";
            if (isMaster) {
                Instantiate(MasterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            }
            SceneManager.LoadScene(gameScenes[miniGame]);
        } else {
            if(!PhotonNetwork.connected)
            {
                //Au lancement de la scène, on demande à Photon de se connecter
                PhotonNetwork.ConnectUsingSettings("PartyGoMiniGames");
            }

            PhotonNetwork.JoinLobby();
        }
        
    }

    //Le projet Unity étant configuré en AutoJoinLobby, dès qu'on est connecté, Photon rejoins le lobby
    //et exécute cette méthode une fois rejoins
    void OnJoinedLobby()
    {
        randomRoom = "4004";
        playerName = "Joueur-" + Random.Range(44,69);
        //On définit les options de la room (Partie)
        string roomName = "MniGame_" + randomRoom; //Le nom de la room à rejoindre -> statique pour l'instant
        RoomOptions MyRoomOptions = new RoomOptions();
        MyRoomOptions.MaxPlayers = 2; //On limite la partie à 2 joueurs max;
        MyRoomOptions.CleanupCacheOnLeave = true;
        PhotonNetwork.playerName = playerName; //On définit le nom du joueur -> Aléatoire pour l'instant
        
        //Rejoins la room concernée, ou la crée si elle n'existe pas, avec son nom, ses options, et le
        //type de lobby, laissé par défaut ici
        PhotonNetwork.JoinOrCreateRoom(roomName, MyRoomOptions, TypedLobby.Default);
    }

    //Une fois qu'on à rejoins la room, on peut charger la scène du mini-jeu
    void OnJoinedRoom()
    {
        //En fonction de la valeur reçue dans l'extra "miniGame", on charge la scène du jeu correspondant
        //Ou on quitte l'appli si la valeur ne correspond à aucun
        //miniGame = "OnlineGame3";
        isMaster = true;
        if (isMaster) {
            Instantiate(MasterPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        
        PhotonNetwork.LoadLevel(gameScenes[miniGame]);
    }
}
