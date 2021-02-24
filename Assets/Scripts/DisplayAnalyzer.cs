using System;
using UnityEngine;
using UnityEngine.UI;

public class DisplayAnalyzer : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Text _text;

    public void UpdateValuesOnDisplay(float value)
    {
        _text.text = Convert.ToString(Math.Round(value, 2));
    }

    private void OnEnable()
    {
        _background.color = Color.white;
        _text.enabled = true;
    }

    private void OnDisable()
    {
        _background.color = Color.gray;
        _text.enabled = false;
    }
}
