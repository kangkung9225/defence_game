using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefenceGame.Core
{
    /// <summary>
    /// Stage 01 광학 재료·출력기 순환 경제 및 2슬롯 동시 훈련 루프를 관리합니다.
    /// </summary>
    public class Stage01EconomyManager : MonoBehaviour
    {
        private const float DefaultInitialMaterial = 150f;
        private const float DefaultMaterialPerSecond = 5f;
        private const int DefaultInitialEmitters = 10;
        private const int DefaultMaxTrainingSlots = 2;
        private const float DefaultEmitterRecoverSeconds = 5f;
        private const int DefaultEmitterCraftCost = 60;
        private const float DefaultEmitterCraftSeconds = 8f;

        [Serializable]
        private struct TrainingOrder
        {
            public string UnitId;
            public int MaterialCost;
            public int EmitterCost;
            public float RemainingSeconds;
        }

        [Serializable]
        private struct EmitterRecoveryOrder
        {
            public int Count;
            public float RemainingSeconds;
        }

        public float CurrentMaterial => _currentMaterial;
        public int AvailableEmitters => _availableEmitters;
        public int TotalEmitters => _totalEmitters;
        public int ActiveTrainingCount => _activeTrainings.Count;
        public int MaxTrainingSlots => DefaultMaxTrainingSlots;
        public bool IsCraftingEmitter => _isCraftingEmitter;
        public float EmitterCraftRemainingSeconds => _emitterCraftRemainingSeconds;

        public event Action<string> OnUnitTrained;
        public event Action<int> OnEmittersRecovered;
        public event Action<string> OnActionRejected;

        private float _currentMaterial;
        private int _availableEmitters;
        private int _totalEmitters;
        private bool _isPaused;
        private bool _isBattleActive;

        private bool _isCraftingEmitter;
        private float _emitterCraftRemainingSeconds;

        private readonly List<TrainingOrder> _activeTrainings = new List<TrainingOrder>();
        private readonly List<EmitterRecoveryOrder> _pendingRecoveries = new List<EmitterRecoveryOrder>();

        private void Awake()
        {
            ResetEconomyState();
        }

        private void Update()
        {
            if (!_isBattleActive || _isPaused)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _currentMaterial += DefaultMaterialPerSecond * deltaTime;

            TickTrainingOrders(deltaTime);
            TickEmitterCraft(deltaTime);
            TickEmitterRecoveries(deltaTime);
        }

        /// <summary>
        /// 재시작 또는 스테이지 진입 시 자원, 출력기, 훈련 예약을 초기 상태로 리셋합니다.
        /// </summary>
        public void ResetEconomyState()
        {
            _currentMaterial = DefaultInitialMaterial;
            _totalEmitters = DefaultInitialEmitters;
            _availableEmitters = DefaultInitialEmitters;
            _isCraftingEmitter = false;
            _emitterCraftRemainingSeconds = 0f;
            _activeTrainings.Clear();
            _pendingRecoveries.Clear();
            _isPaused = false;
            _isBattleActive = true;
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        public void SetBattleActive(bool active)
        {
            _isBattleActive = active;
        }

        /// <summary>
        /// 병종 훈련을 요청합니다. 동일 병종 연타 시 중복 차감을 차단합니다.
        /// </summary>
        public bool TryStartTraining(string unitId, int materialCost, int emitterCost, float trainingSeconds)
        {
            if (!_isBattleActive || _isPaused)
            {
                return false;
            }

            if (IsUnitTraining(unitId))
            {
                OnActionRejected?.Invoke("이미 훈련 중인 병종입니다.");
                return false;
            }

            if (_activeTrainings.Count >= DefaultMaxTrainingSlots)
            {
                OnActionRejected?.Invoke("훈련 슬롯(2/2)이 가득 찼습니다.");
                return false;
            }

            if (_currentMaterial < materialCost)
            {
                OnActionRejected?.Invoke("광학 재료가 부족합니다.");
                return false;
            }

            if (_availableEmitters < emitterCost)
            {
                OnActionRejected?.Invoke("가용 출력기가 부족합니다.");
                return false;
            }

            _currentMaterial -= materialCost;
            _availableEmitters -= emitterCost;

            _activeTrainings.Add(new TrainingOrder
            {
                UnitId = unitId,
                MaterialCost = materialCost,
                EmitterCost = emitterCost,
                RemainingSeconds = trainingSeconds
            });

            return true;
        }

        /// <summary>
        /// 공방에서 출력기 제작(재료 60 · 8초)을 시작합니다. 병종 훈련과 병행 가능합니다.
        /// </summary>
        public bool TryStartEmitterCraft()
        {
            if (!_isBattleActive || _isPaused || _isCraftingEmitter)
            {
                return false;
            }

            if (_currentMaterial < DefaultEmitterCraftCost)
            {
                OnActionRejected?.Invoke("출력기 제작 재료(60)가 부족합니다.");
                return false;
            }

            _currentMaterial -= DefaultEmitterCraftCost;
            _isCraftingEmitter = true;
            _emitterCraftRemainingSeconds = DefaultEmitterCraftSeconds;
            return true;
        }

        /// <summary>
        /// 아군 유닛 사망 시 장착 중이던 출력기를 5초 후 회수 대기열에 등록합니다.
        /// </summary>
        public void QueueEmitterRecovery(int emitterCount)
        {
            if (emitterCount <= 0)
            {
                return;
            }

            _pendingRecoveries.Add(new EmitterRecoveryOrder
            {
                Count = emitterCount,
                RemainingSeconds = DefaultEmitterRecoverSeconds
            });
        }

        public bool IsUnitTraining(string unitId)
        {
            for (int i = 0; i < _activeTrainings.Count; i++)
            {
                if (_activeTrainings[i].UnitId == unitId)
                {
                    return true;
                }
            }

            return false;
        }

        private void TickTrainingOrders(float deltaTime)
        {
            for (int i = _activeTrainings.Count - 1; i >= 0; i--)
            {
                TrainingOrder order = _activeTrainings[i];
                order.RemainingSeconds -= deltaTime;

                if (order.RemainingSeconds <= 0f)
                {
                    _activeTrainings.RemoveAt(i);
                    OnUnitTrained?.Invoke(order.UnitId);
                }
                else
                {
                    _activeTrainings[i] = order;
                }
            }
        }

        private void TickEmitterCraft(float deltaTime)
        {
            if (!_isCraftingEmitter)
            {
                return;
            }

            _emitterCraftRemainingSeconds -= deltaTime;
            if (_emitterCraftRemainingSeconds <= 0f)
            {
                _isCraftingEmitter = false;
                _emitterCraftRemainingSeconds = 0f;
                _totalEmitters += 1;
                _availableEmitters += 1;
                OnEmittersRecovered?.Invoke(1);
            }
        }

        private void TickEmitterRecoveries(float deltaTime)
        {
            for (int i = _pendingRecoveries.Count - 1; i >= 0; i--)
            {
                EmitterRecoveryOrder recovery = _pendingRecoveries[i];
                recovery.RemainingSeconds -= deltaTime;

                if (recovery.RemainingSeconds <= 0f)
                {
                    _pendingRecoveries.RemoveAt(i);
                    _availableEmitters += recovery.Count;
                    OnEmittersRecovered?.Invoke(recovery.Count);
                }
                else
                {
                    _pendingRecoveries[i] = recovery;
                }
            }
        }
    }
}
