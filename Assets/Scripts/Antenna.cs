using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antenna : MonoBehaviour
{
    [Header("Объект для поиска")][SerializeField] private Transform _targetTransform;
    private float _targetDistanceSquared;

    public Action<float> NewReadingsReceived;

    private void Update()
    {
        float targetDistanceSquared = (_targetTransform.position - this.transform.position).sqrMagnitude;
        if (targetDistanceSquared != _targetDistanceSquared)
        {
            _targetDistanceSquared = targetDistanceSquared;
            NewReadingsReceived?.Invoke(_targetDistanceSquared);
        }        
    }
}
