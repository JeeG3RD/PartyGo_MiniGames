using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleObjController : MonoBehaviour
{
    private PhotonView view;
    private Scrollbar scrollBar;
    private Vector3 objPosition;
    private Vector3 minPosition, maxPosition;
    private int role;
    private float scrollVal, shotDelay, shootForce;
    private bool isStarted;

    public GameObject bulletPrefab, ejectPos = null;

    // Start is called before the first frame update
    void Awake()
    {
        view = this.gameObject.GetComponent<PhotonView>();
        shotDelay = 2.5f;
        shootForce = 2500f;
    }

    public void SetParams(Vector3 objPos, Scrollbar scrollB, int roleNum)
    {
        this.scrollBar = scrollB;
        this.role = roleNum;

        switch(this.role) {
            case 1:
                minPosition = new Vector3(7.5f, objPos.y, objPos.z);
                maxPosition = new Vector3(-7.5f, objPos.y, objPos.z);
                break;
            
            case 2:
                minPosition = new Vector3(-7.5f, objPos.y, objPos.z);
                maxPosition = new Vector3(7.5f, objPos.y, objPos.z);
                break;
        }

        scrollBar.onValueChanged.AddListener(OnScroll);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnScroll(float value)
    {
        if (scrollBar != null && view.isMine) {
            this.gameObject.transform.position = Vector3.Lerp(minPosition, maxPosition, value);
        }
    }

    public void StartShoot()
    {
        //Si je suis le joueur attaque
        //Appelle de la coroutine Shot
        if(this.role == 1 && view.isMine) {
            isStarted = true;
            this.StartCoroutine(this.Shot());
        }
    }

    public IEnumerator Shot()
    {
        yield return new WaitForSeconds(0.75f);
        view.RPC("Shoot", PhotonTargets.All, ejectPos.transform.position, transform.TransformDirection(Vector3.back));
        while(isStarted == true) {
            if(this.role == 1 && view.isMine) {
                yield return new WaitForSeconds(shotDelay);
                view.RPC(
                    "Shoot",
                    PhotonTargets.All,
                    ejectPos.transform.position,
                    transform.TransformDirection(Vector3.back)
                );
            }
            shotDelay -= 0.07f;
        }
    }
    
    [PunRPC]
    void Shoot(Vector3 pos, Vector3 dir)
    {
        GameObject go;
        go = Instantiate(bulletPrefab, pos, Quaternion.identity);
        go.GetComponent<Rigidbody>().AddForce(dir * shootForce);
    }
}
