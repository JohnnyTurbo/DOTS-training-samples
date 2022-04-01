using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace AutoFarmers.Authoring
{
    public class DebugDisplay : MonoBehaviour
    {
        public TMP_Text FrameRateDisplay;
        public TMP_Text FarmerCount;
        public TMP_Text DroneCount;
        public TMP_Text HarvestCount;
        public TMP_Text FarmSize;
        
        [SerializeField] private float _fpsPollRate;
        
        private float _timer;
        private float _curFPSAverage;
        private List<float> _fpsThisSecond;
        private Entity _farmEntity;
        private EntityManager _entityManager;
        private void Start()
        {
            _fpsThisSecond = new List<float>();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _farmEntity = Entity.Null;
            GetFarmEntity();
        }

        private void GetFarmEntity()
        {
            var farmQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<FarmData>());
            if(farmQuery.CalculateEntityCount() == 0){return;}
            _farmEntity = farmQuery.GetSingletonEntity();
            var farmData = _entityManager.GetComponentData<FarmData>(_farmEntity);
            FarmSize.text = $"Farm Size: {farmData.FarmSize.ToString()}";
        }
        
        private void Update()
        {
            RunFPSCounter();
            if (_farmEntity == Entity.Null)
            {
                GetFarmEntity();
                if (_farmEntity == Entity.Null)
                {
                    return;
                }
            }
            var farmStats = _entityManager.GetComponentData<StatsData>(_farmEntity);
            FarmerCount.text = $"Farmer Count: {farmStats.FarmerCount}";
            DroneCount.text = $"Drone Count: {farmStats.DroneCount}";
            HarvestCount.text = $"Harvest Count: {farmStats.HarvestCount}";
        }

        private void RunFPSCounter()
        {
            _timer -= Time.unscaledDeltaTime;
            _fpsThisSecond.Add(1f / Time.unscaledDeltaTime);
            if (_timer <= 0f)
            {
                var totalFrameTimes = 0f;
                foreach (var fps in _fpsThisSecond)
                {
                    totalFrameTimes += fps;
                }
                _curFPSAverage = totalFrameTimes / _fpsThisSecond.Count;
                _fpsThisSecond.Clear();
                _timer = _fpsPollRate;
            }

            FrameRateDisplay.text = $"FPS: {_curFPSAverage:N2}";
        }
    }
}