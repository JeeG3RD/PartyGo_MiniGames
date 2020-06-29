using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObstacleRotation : MonoBehaviour
{
    public bool rotate = false;
    public Text txt;

    public float rotationSpeed = 1.00f;
    // Start is called before the first frame update
    void Awake()
    {
        this.rotate = false;
        txt.text = rotationSpeed.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.rotate == true) {
            this.gameObject.GetComponent<Transform>().Rotate(0.0f, rotationSpeed, 0.0f);
        }
    }
}
