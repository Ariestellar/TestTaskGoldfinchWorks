using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using System;
using UnityEngine;

public class RotateGrabbable : BasicGrabbable
        , IColliderEventDragStartHandler
        , IColliderEventDragFixedUpdateHandler
        , IColliderEventDragUpdateHandler
        , IColliderEventDragEndHandler
{

    [SerializeField] [HideInInspector] private ColliderButtonEventData.InputButton _grabButton = ColliderButtonEventData.InputButton.Trigger;    
    [SerializeField] private bool _allowMultipleGrabbers = true;
    private bool _moveByVelocity { get { return !unblockableGrab && grabRigidbody != null && !grabRigidbody.isKinematic; } }
    private IndexedTable<ColliderButtonEventData, Grabber> _eventGrabberSet;
    private float _currentAngleRotation;
    private float _totalSwingAngle;
    private Quaternion _startingPositionRotationValve;
    private RigidPose _grabStart;

    public Action<float> UpdateProgressTurn;
    [Header("Максимальный поворот (в градусах)")] public float MaximumTurn;

    protected override void Awake()
    {
        base.Awake();

        RestoreObsoleteGrabButton();
        _startingPositionRotationValve = transform.localRotation;
    }

    private void RestoreObsoleteGrabButton()
    {
        if (_grabButton == ColliderButtonEventData.InputButton.Trigger) { return; }
        ClearSecondaryGrabButton();
        SetSecondaryGrabButton(_grabButton, true);
        _grabButton = ColliderButtonEventData.InputButton.Trigger;
    }

    private void ClearEventGrabberSet()
    {
        if (_eventGrabberSet == null) { return; }

        for (int i = _eventGrabberSet.Count - 1; i >= 0; --i)
        {
            Grabber.Release(_eventGrabberSet.GetValueByIndex(i));
        }

        _eventGrabberSet.Clear();
    }

    public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (!IsValidGrabButton(eventData)) { return; }

        if (!_allowMultipleGrabbers)
        {
            ClearGrabbers(false);
            ClearEventGrabberSet();
        }

        var grabber = Grabber.Get(eventData);
        var offset = RigidPose.FromToPose(grabber.GrabberOrigin, new RigidPose(transform));
        if (alignPosition) { offset.pos = alignPositionOffset; }
        if (alignRotation) { offset.rot = Quaternion.Euler(alignRotationOffset); }
        grabber.GrabOffset = offset;

        if (_eventGrabberSet == null) { _eventGrabberSet = new IndexedTable<ColliderButtonEventData, Grabber>(); }
        _eventGrabberSet.Add(eventData, grabber);

        AddGrabber(grabber);
        _grabStart = currentGrabber.GrabberOrigin;
    }

    public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        _currentAngleRotation = GetCurrenAngleRotation(_grabStart.pos, currentGrabber.GrabberOrigin.pos, grabRigidbody.transform.position);

        if (_totalSwingAngle <= 0)
        {
            transform.localRotation = GetNewRotationPositionInTheRange(0, 90);
        }
        else if (_totalSwingAngle <= 45 && _totalSwingAngle > 0)
        {
            transform.localRotation = GetNewRotationPositionInTheRange(-_totalSwingAngle, 90 - _totalSwingAngle);
        }
        else if (_totalSwingAngle > MaximumTurn - 90)
        {
            transform.localRotation = GetNewRotationPositionInTheRange((MaximumTurn - _totalSwingAngle) - 90, MaximumTurn - _totalSwingAngle);
        }
        else if (_totalSwingAngle >= MaximumTurn)
        {
            transform.localRotation = GetNewRotationPositionInTheRange(-90, 0);
        }
        else
        {
            transform.localRotation = GetNewRotationPositionInTheRange(-45, 45);
        }

        if (isGrabbed && !_moveByVelocity && currentGrabber.eventData == eventData)
        {
            RecordLatestPosesForDrop(Time.time, 0.05f);
            OnGrabTransform();
        }
    }

    public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        if (_currentAngleRotation > 180)
        {
            _currentAngleRotation = 360 - _currentAngleRotation;
            _totalSwingAngle -= _currentAngleRotation;
        }
        else if (_currentAngleRotation < 0)
        {
            _totalSwingAngle = 0;
        }
        else
        {
            _totalSwingAngle += _currentAngleRotation;
        }

        _startingPositionRotationValve = transform.localRotation;

        float currentProgressTurn = _totalSwingAngle * 100 / MaximumTurn;
        UpdateProgressTurn?.Invoke(currentProgressTurn);

        if (_eventGrabberSet == null) { return; }

        Grabber grabber;
        if (!_eventGrabberSet.TryGetValue(eventData, out grabber)) { return; }

        RemoveGrabber(grabber);
        _eventGrabberSet.Remove(eventData);
        Grabber.Release(grabber);
    }

    private float GetCurrenAngleRotation(Vector3 startPosition, Vector3 currentPosition, Vector3 axisRotate)
    {
        float currentAngleRotation = Vector3.Angle(startPosition - axisRotate, currentPosition - axisRotate);
        currentAngleRotation *= (Vector3.Dot(axisRotate, Vector3.Cross(startPosition - axisRotate, currentPosition - axisRotate)) < 0 ? -1 : 1);
        return currentAngleRotation;
    }

    private Quaternion GetNewRotationPositionInTheRange(float minRange, float maxRange)
    {
        _currentAngleRotation = Mathf.Clamp(_currentAngleRotation, minRange, maxRange);
        if (_currentAngleRotation < 0)
        {
            _currentAngleRotation = 360 + _currentAngleRotation;
        }
        Quaternion quaternion = Quaternion.Euler(0, _currentAngleRotation, 0);
        return _startingPositionRotationValve * quaternion;
    }
}
