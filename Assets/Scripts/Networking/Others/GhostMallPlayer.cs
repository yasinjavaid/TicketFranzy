using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class GhostMallPlayer : MonoBehaviour
{
    
    #region serializeFields

    [SerializeField] protected float speed;
    [SerializeField] protected float accelerationTime;
    [SerializeField] protected float rotationMaxSpeed;
    [SerializeField] protected float rotationSmoothTime;
    [SerializeField] protected float rotationOffset;
    [SerializeField] protected Rigidbody myRigidbody;
    [SerializeField] protected Animator myAnimator;

    #endregion

    #region public fields

    [HideInInspector] public int realPlayerActorNumber;

    #endregion
   
    #region private fields

    private Vector3 playerPos;
    private Quaternion playerRotation;
    private bool isPlayerWalkingAnim;
    
    private float m_Distance;
    private float m_Angle;
    private static readonly int Walking = Animator.StringToHash("Walking");

    #endregion

    #region monobehaviour callbacks

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
    }

   

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerPos, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate)); 
        transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
        myAnimator.SetBool(Walking, isPlayerWalkingAnim);
    }

    #endregion

    #region public methods

   

    #endregion

  

   

    #region private methods

    private void OnRealPlayerDataReceive(object[] data)
    {
        if ((int)data[0] == NetworkManager.Instance.LocalPlayer.ActorNumber) return;
        
        playerPos = (Vector3) data[1];
        playerRotation = (Quaternion) data[2];
        isPlayerWalkingAnim = (bool) data[3];
        m_Distance = Vector3.Distance(transform.position, playerPos);
        m_Angle = Quaternion.Angle(transform.rotation, playerRotation);
    }

    #endregion

    #region EvntsBindings 

    private void NetworkingClientOnEventReceived(EventData obj)
    {
        byte eventCode = obj.Code;
        object[] data = (object[]) obj.CustomData;
        switch (eventCode)
        {
            case NetworkManager.PlayerPositionMallEventCode :
                OnRealPlayerDataReceive(data); 
                break;
            
            default:
                break;
        }
    }

    #endregion
}
