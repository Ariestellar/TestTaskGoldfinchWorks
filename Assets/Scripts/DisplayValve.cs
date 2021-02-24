using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayValve : MonoBehaviour
{
    [SerializeField] private Text _progressText;

    private void Start ()
    {
        UpdateProgress(0);
    }

    public void UpdateProgress(float currentProgress)
    {
        _progressText.text = Convert.ToString(Math.Round(currentProgress, 2)) + " %";
    }
}
