﻿namespace ProjectH.Features.Levels
{
    using UnityEngine;
    using ProjectH.Features.Levels.Data;
    using System.Collections.Generic;
    using ProjectH.Features.Grid.Pieces;
    using VUDK.Features.Main.EventSystem;
    using ProjectH.Constants;
    using VUDK.Patterns.Initialization.Interfaces;
    using System;
    using ProjectH.Managers.Main;

    public class LevelManager : MonoBehaviour, IInit<GameManager>
    {
        [Header("Levels")]
        [SerializeField]
        private LevelsMapData _levelsMapData;

        private GameManager _gameManager;

        public bool IsLoadingLevel => _gameManager.VictoryAnimationController.CameraFade.IsFading;

        public int CurrentLevelIndex { get; private set; }
        public List<Piece> CurrentPieces { get; private set; }
        public int PiecesCount => CurrentPieces.Count;

        private void Awake()
        {
            CurrentPieces = new List<Piece>();
        }

        private void OnEnable()
        {
            EventManager.Ins.AddListener(EventKeys.GameEvents.OnGameBegin, ClearPieces);
        }

        private void OnDisable()
        {
            EventManager.Ins.RemoveListener(EventKeys.GameEvents.OnGameBegin, ClearPieces);
        }

        public void Init(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public bool Check()
        {
            return _gameManager != null;
        }

        public LevelData GetCurrentLevelData()
        {
            return _levelsMapData.Levels[CurrentLevelIndex];
        }

        public void SetCurrentLevelIndexToNextLevel()
        {
            if (CurrentLevelIndex >= _levelsMapData.Levels.Count - 1)
            {
                CurrentLevelIndex = 0;
                return;
            }

            CurrentLevelIndex++;
        }

        public void AddPiece(Piece piece)
        {
            CurrentPieces.Add(piece);
        }

        public void RemovePiece(Piece piece)
        {
            CurrentPieces.Remove(piece);
        }

        public void ClearPieces()
        {
            foreach (Piece piece in CurrentPieces)
                piece.Dispose();

            CurrentPieces.Clear();
        }
    }
}