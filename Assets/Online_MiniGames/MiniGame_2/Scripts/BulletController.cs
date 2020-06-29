using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private float lifeTime = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        //Détruis automatiquement l'object après un temps défini
        Destroy(this.gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) 
    {
        //Si l'objet touché est le joueur de défense
        if(other.gameObject.tag == "Defense") {
            //Appel du Photon Event "Scored"
            RaiseEventOptions options = new RaiseEventOptions(){Receivers = ReceiverGroup.All};
            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.Scored, 0, true, options);
            //Destruction de l'objet bullet
            Destroy(this.gameObject, 0.0f);
        }
    }
}
