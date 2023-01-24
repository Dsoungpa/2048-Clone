using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShake : MonoBehaviour
{
    public float shakeAmount = 5.0f;
    public float shakeDuration = 0.5f;

    private float shakeTimer = 0.0f;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            
            shakeTimer = 0.0f;
            //transform.position = originalPosition;
        }
    }

    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }
}
