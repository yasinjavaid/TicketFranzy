using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcticChompPaddle : MonoBehaviour
{
    public float paddleForce;
    public float liftForce;
    public string ballTag1;
    public string ballTag2;

    public bool applyForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(ballTag1) || collision.gameObject.CompareTag(ballTag2))
        {
            if (applyForce)
            {
                Ball ball = collision.gameObject.GetComponent<Ball>();
                ball.GetRigidbody.AddForce(new Vector3(0, liftForce, paddleForce), ForceMode.Impulse);
            }
            else
            {
                Ball ball = collision.gameObject.GetComponent<Ball>();
                ball.GetRigidbody.AddForce(new Vector3(0, liftForce/4, paddleForce/4), ForceMode.Impulse);
            }
        }
    }
}
