using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;

public class ConnectFourHoops : BallShoot
{
  
    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private float ballForce;

    private GameObject ballInstance;

    private Vector3 mouseStart, mouseEnd;

    private float minFlickDistance = 15f;
    private float zDepth = 25f;

    public int score;
    public Text scoreText;

    
    private  Mouse mouse => Mouse.current;
    void Awake () {
      
    }

    void Start () {
        CreateBall ();
    }

   

    void Update () {
        FlickAction ();
        UpdateScore ();
        if (mouse.leftButton.isPressed)
        {
            
        }
    }

    void UpdateScore () {
        scoreText.text = "" + score;
    }

    void CreateBall () {
        ballInstance = Instantiate (ballPrefab, ballPrefab.transform.position, Quaternion.identity);
    }

    void RespawnBall() {
        ballInstance.GetComponent<Rigidbody> ().velocity = Vector3.zero;
        ballInstance.GetComponent<Rigidbody> ().angularVelocity = Vector3.zero;
        ballInstance.transform.rotation = Quaternion.identity;
        Vector3 restart = new Vector3 (0f, 0.1f, 5f);
        ballInstance.transform.position = restart;
    }

    void FlickAction () {
        if (Input.GetMouseButtonDown (0)) {
            mouseStart = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp (0)) {
            mouseEnd = Input.mousePosition;
            if (Vector3.Distance (mouseEnd, mouseStart) > minFlickDistance) {
                // Kick ball
                Vector3 hitPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, zDepth);
                hitPos = Camera.main.ScreenToWorldPoint (hitPos);
                ballInstance.transform.LookAt (hitPos);
                ballInstance.GetComponent<Rigidbody> ().AddRelativeForce (ballInstance.transform.forward * ballForce, ForceMode.Impulse);

                Invoke ("RespawnBall", 3f);
            }
        }
    }
}
