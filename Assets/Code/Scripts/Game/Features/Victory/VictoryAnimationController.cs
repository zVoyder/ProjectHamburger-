﻿namespace ProjectH.Features.Victory
{
    using ProjectH.Constants;
    using ProjectH.Managers.Main;
    using System;
    using UnityEngine;
    using VUDK.Extensions;
    using VUDK.Features.Main.Camera.CameraModifiers;
    using VUDK.Features.Main.EventSystem;
    using VUDK.Generic.Managers.Main;
    using VUDK.Generic.Managers.Main.Interfaces;
    using VUDK.Generic.Serializable;

    public class VictoryAnimationController : MonoBehaviour, ICastGameManager<GameManager>
    {
        [SerializeField]
        private VictoryPlate _plate;
        [SerializeField]
        private Vector3 _cameraOffsetPosition;
        [SerializeField]
        private Vector3 _plateOffsetPosition;
        [SerializeField]
        private DelayTask _moveTask;

        private Transform _target;
        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;

        public GameManager GameManager => MainManager.Ins.GameManager as GameManager;
        private Camera Camera => MainManager.Ins.GameStats.MainCamera;
        public CameraFade CameraFade { get; private set; }

        private void Awake()
        {
            _originalCameraPosition = Camera.transform.position;
            _originalCameraRotation = Camera.transform.rotation;
            Camera.TryGetComponent(out CameraFade fade);
            CameraFade = fade;
        }

        private void OnEnable()
        {
            EventManager.Ins.AddListener(EventKeys.GameEvents.OnGameVictory, AnimateVictory);
            EventManager.Ins.AddListener(EventKeys.GameEvents.OnNextLevelTriggered, StartCameraFade);
            _moveTask.OnTaskCompleted += OnMoveTaskCompleted;
            CameraFade.OnFadeInEnd += OnFadeInEnd;
        }

        private void OnDisable()
        {
            EventManager.Ins.RemoveListener(EventKeys.GameEvents.OnGameVictory, AnimateVictory);
            EventManager.Ins.RemoveListener(EventKeys.GameEvents.OnNextLevelTriggered, StartCameraFade);
            _moveTask.OnTaskCompleted -= OnMoveTaskCompleted;
            CameraFade.OnFadeInEnd -= OnFadeInEnd;
        }

        private void Update()
        {
            if (!_moveTask.Process()) return;

            _plate.transform.position = Vector3.Lerp(_plate.OriginalPosition, _target.position + _plateOffsetPosition, _moveTask.ElapsedPercentPrecise);
            Camera.transform.position = Vector3.Lerp(_originalCameraPosition, _target.position + _cameraOffsetPosition, _moveTask.ElapsedPercentPrecise);
            Camera.transform.LookAtLerp(_originalCameraRotation, _target, _moveTask.ElapsedPercentNormalized);
        }

        /// <summary>
        /// Starts the victory animation.
        /// </summary>
        private void AnimateVictory()
        {
            _target = GameManager.LevelManager.PiecesOnField[0].CurrentTile.BottomPiece.transform;
            _moveTask.Start();
        }

        /// <summary>
        /// Callback for move task completed.
        /// </summary>
        private void OnMoveTaskCompleted()
        {
            _plate.StartRotating();
            _target.transform.SetParent(_plate.transform);
            EventManager.Ins.TriggerEvent(EventKeys.GameEvents.OnEatPhase);
        }

        /// <summary>
        /// Starts the camera fade.
        /// </summary>
        private void StartCameraFade()
        {
            CameraFade.DoFadeInOut();
        }

        /// <summary>
        /// Callback for fade in end.
        /// </summary>
        private void OnFadeInEnd()
        {
            _plate.StopRotating();
            _plate.ResetPosition();
            Camera.transform.position = _originalCameraPosition;
            Camera.transform.rotation = _originalCameraRotation;
            EventManager.Ins.TriggerEvent(EventKeys.GameEvents.OnEatPhaseFadeInEnd);
        }
    }
}