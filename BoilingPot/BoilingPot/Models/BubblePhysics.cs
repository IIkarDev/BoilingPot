// Models/BubblePhysics.cs
using System;
using BoilingPot.ViewModels.Components; // Для BubbleViewModel
using Avalonia; // Для Point

namespace BoilingPot.Models
{
    // Класс, инкапсулирующий логику движения ОДНОГО пузырька.
    public class BubblePhysics
    {
        private static Random _globalRandom = new Random();
        private Random _localRandom = new Random(_globalRandom.Next());

        private double _aquariumWidth;
        private double _aquariumHeight;
        private double _currentSpeedFactor;
        private double _currentHeatFactor;
        
        // --- Индивидуальные параметры пузырька ---
        private double _targetYLevelUp;
        private double _targetYLevelDown;
        private double _targetXCenterLane;
        private double _targetXEdgeLane;
        private double _inherentBuoyancy;
        private MovementDirection _currentDirection;
        private bool _isMovingToLeftEdgeInitially;

        // --- НОВОЕ: Объявление пропущенного поля ---
        private double _edgeLaneWidthRatio = 0.25; // Ширина боковой полосы как доля от общей ширины аквариума

        private enum MovementDirection { UpEdge, ToCenterAtTop, DownCenter, ToEdgeAtBottom }

        public BubblePhysics(double aquariumWidth, double aquariumHeight, double initialX)
        {
            _aquariumWidth = aquariumWidth;
            _aquariumHeight = aquariumHeight;

            // Инициализация индивидуальных параметров
            _targetYLevelUp = _aquariumHeight * (0.1 + _localRandom.NextDouble() * 0.3);
            _targetYLevelDown = _aquariumHeight * (0.7 + _localRandom.NextDouble() * 0.25);

            double centerLaneWidth = aquariumWidth * 0.4; // Ширина центральной полосы
            // Используем объявленное поле _edgeLaneWidthRatio
            double actualEdgeLaneWidth = aquariumWidth * _edgeLaneWidthRatio;

            _targetXCenterLane = (aquariumWidth - centerLaneWidth) / 2 + _localRandom.NextDouble() * centerLaneWidth;

            _isMovingToLeftEdgeInitially = (initialX < aquariumWidth / 2);
            if (_isMovingToLeftEdgeInitially)
            {
                _targetXEdgeLane = _localRandom.NextDouble() * actualEdgeLaneWidth;
            }
            else
            {
                _targetXEdgeLane = aquariumWidth - actualEdgeLaneWidth + _localRandom.NextDouble() * actualEdgeLaneWidth;
            }

            _inherentBuoyancy = 0.8 + _localRandom.NextDouble() * 0.4;
            _currentDirection = MovementDirection.UpEdge;
        }

        public void UpdateSimulationParameters(double speedFactor, double heatFactor)
        {
            _currentSpeedFactor = Math.Clamp(speedFactor, 0.1, 5.0);
            _currentHeatFactor = Math.Clamp(heatFactor, 0.0, 1.0);
        }

        public void UpdateBubblePosition(BubbleViewModelBase bubble)
        {
            if (bubble == null || _currentSpeedFactor <= 0) return;

            double baseSpeed = 0.5;
            double heatEffect = 5.0 * _currentHeatFactor;

            double verticalSpeed = (baseSpeed + heatEffect) * _currentSpeedFactor * _inherentBuoyancy;
            double horizontalSpeed = (baseSpeed * 0.5 + heatEffect * 0.3) * _currentSpeedFactor * _inherentBuoyancy;
            double randomDrift = 0.5 * _currentSpeedFactor;

            double deltaX = (_localRandom.NextDouble() - 0.5) * randomDrift;
            double deltaY = 0;

            double bubbleCenterX = bubble.X + bubble.Size / 2;
            // double bubbleCenterY = bubble.Y + bubble.Size / 2; // Не используется напрямую в switch

            double centerX = _aquariumWidth / 2.0;

            switch (_currentDirection)
            {
                case MovementDirection.UpEdge:
                    deltaY = -verticalSpeed;
                    deltaX += Math.Sign(_targetXEdgeLane - bubbleCenterX) * horizontalSpeed * 0.3;
                    if (bubble.Y + deltaY <= _targetYLevelUp)
                    {
                        bubble.Y = _targetYLevelUp;
                        _currentDirection = MovementDirection.ToCenterAtTop;
                    }
                    break;

                case MovementDirection.ToCenterAtTop:
                    deltaY = (_localRandom.NextDouble() - 0.5) * verticalSpeed * 0.1;
                    deltaX += Math.Sign(_targetXCenterLane - bubbleCenterX) * horizontalSpeed;
                    if (Math.Abs(_targetXCenterLane - bubbleCenterX) < bubble.Size / 2)
                    {
                        bubble.X = _targetXCenterLane - bubble.Size / 2;
                        _currentDirection = MovementDirection.DownCenter;
                    }
                    break;

                case MovementDirection.DownCenter:
                    deltaY = verticalSpeed * (0.5 + (1 - _currentHeatFactor) * 0.4);
                    deltaX += (_localRandom.NextDouble() - 0.5) * horizontalSpeed * 0.2;
                    if (bubble.Y + bubble.Size + deltaY >= _targetYLevelDown)
                    {
                        bubble.Y = _targetYLevelDown - bubble.Size;
                        _currentDirection = MovementDirection.ToEdgeAtBottom;
                        _isMovingToLeftEdgeInitially = (_localRandom.Next(2) == 0); // Перевыбираем край
                        // Используем объявленное поле _edgeLaneWidthRatio
                        double actualEdgeLaneWidth = _aquariumWidth * _edgeLaneWidthRatio;
                        _targetXEdgeLane = _isMovingToLeftEdgeInitially ?
                                           _localRandom.NextDouble() * actualEdgeLaneWidth :
                                           _aquariumWidth - actualEdgeLaneWidth + _localRandom.NextDouble() * actualEdgeLaneWidth;
                    }
                    break;

                case MovementDirection.ToEdgeAtBottom:
                    deltaY = (_localRandom.NextDouble() - 0.5) * verticalSpeed * 0.1;
                    deltaX += Math.Sign(_targetXEdgeLane - bubbleCenterX) * horizontalSpeed;
                    if (Math.Abs(_targetXEdgeLane - bubbleCenterX) < bubble.Size)
                    {
                        bubble.X = Math.Clamp(_targetXEdgeLane - bubble.Size / 2, 0, _aquariumWidth - bubble.Size);
                        _currentDirection = MovementDirection.UpEdge;
                    }
                    break;
            }

            bubble.X += deltaX;
            bubble.Y += deltaY;

            bubble.X = Math.Clamp(bubble.X, 0, _aquariumWidth - bubble.Size);
            bubble.Y = Math.Clamp(bubble.Y, 0, _aquariumHeight - bubble.Size);
        }
    }
}