using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StickyGrabbable))]
public class GasAnalyzer : MonoBehaviour
{
    [Header("Время зажатия кнопки")] [SerializeField] private float _buttonHoldTime;
    [Header("Кнопка для включения")] [SerializeField] private ColliderButtonEventData.InputButton _inputButton;
    [SerializeField] private DisplayAnalyzer _displayAnalizer;
    [SerializeField] private Antenna _antenna;  

    private Coroutine _holdingTimeCounter;
    private StickyGrabbable _basicGrabbable;

    private void Awake()
    {
        _basicGrabbable = GetComponent<StickyGrabbable>();
        _antenna.NewReadingsReceived += _displayAnalizer.UpdateValuesOnDisplay;
    }

    private void Start()
    {
        TurnOff();
    }
    
    public void Grabble()
    {
        if (_displayAnalizer.enabled == false)
        {
            _holdingTimeCounter = StartCoroutine(WaitingButtonPress(_buttonHoldTime));
        }               
    }

    public void Release()
    {
        if (_displayAnalizer.enabled == false)
        {
            StopCoroutine(_holdingTimeCounter);
        }        
    }

    private void TurnOn()
    {
        _displayAnalizer.enabled = true;
        _antenna.enabled = true;
    }

    private void TurnOff()
    {
        _displayAnalizer.enabled = false;
        _antenna.enabled = false;
    }

    private IEnumerator WaitingButtonPress(float buttonHoldTime)
    {
        while (true)
        {
            if (_basicGrabbable.IsSecondaryGrabButtonOn(_inputButton))
            {
                buttonHoldTime -= Time.deltaTime;
                if (buttonHoldTime <= 0)
                {
                    TurnOn();
                    StopCoroutine(_holdingTimeCounter);                    
                }
            }
            else
            {
                buttonHoldTime = _buttonHoldTime;
            }           
            yield return null;
        }        
    }
}
