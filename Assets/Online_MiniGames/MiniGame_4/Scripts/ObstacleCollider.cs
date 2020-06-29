using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleCollider : MonoBehaviour
{
    public GameObject obstacle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        switch(other.gameObject.tag.ToString()) {
            case "360Spin" :
                obstacle.GetComponent<ObstacleRotation>().rotationSpeed += 0.125f;
                break;

            case "Player" :
                //int id = other.gameObject.GetPhotonView().viewID;
                other.gameObject.GetComponent<PlayerController_4>().Loose();
                /*PhotonView playerView = other.gameObject.GetComponent<PhotonView>();
                playerView.RPC("Loose_4", PhotonPlayer.Find(id));*/
                //other.gameObject.GetComponent<PhotonView>().RPC("OnWin", PhotonTargets.Others);
                break;

            default :
                break;
        }
    }
}
