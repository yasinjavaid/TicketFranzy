using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DirectionalArrow : MonoBehaviour
{
    [SerializeField] private float time = 0;
    [SerializeField][Range(10,300)] private float angle = 16;
    [SerializeField] private Ease ease = Ease.Flash;
   // [SerializeField] private GameObject plane;
    [SerializeField] private Renderer plane;
    [SerializeField] private bool isPlane = true;
    
    float scrollSpeed = 0.5f;
    private Tween tween;

    public ParticleSystem GetParticleSystem => _particleSystem ? _particleSystem : (_particleSystem = GetComponent<ParticleSystem>());
    protected ParticleSystem _particleSystem;

    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
    public Quaternion Rotation
    {
        get => transform.rotation;
        private set => transform.rotation = value;
    }

    public void StarRotation()
    {
        gameObject.SetActive(true);
        GetParticleSystem.Play();
        RotateTo();
    }

    public void StopRotation()
    {
        tween.Kill();
        GetParticleSystem.Stop();
        gameObject.SetActive(false);
    }

    public void Update()
    {
        if (isPlane)
        {
            float offset = Time.time * scrollSpeed;
            plane.material.SetTextureOffset("_BaseMap", new Vector2(0, offset));
        }
    }

    private void RotateTo()
    {
        tween = transform.DOLocalRotate(new Vector3(-25, -angle, 0), time, RotateMode.Fast)
            .SetEase(ease)
            .OnComplete(RotateBackTo);
    }
    private void RotateBackTo()
    {
        tween = transform.DOLocalRotate(new Vector3(-25, angle, 0), time, RotateMode.Fast)
            .SetEase(ease)
            .OnComplete(RotateTo);
    }
}
