using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Valve : MonoBehaviour
{
    [SerializeField] private DisplayValve _displayValve;
    [SerializeField] private RotateGrabbable _rotateGrabbable;

    private void Awake()
    {
        _rotateGrabbable.UpdateProgressTurn += _displayValve.UpdateProgress;
    }
}
