﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace HighwayRacers
{
    [UpdateAfter(typeof(CarMoveSystem))]
    public class ColorSystem : JobComponentSystem
    {
        struct ColorSystemJob : IJobForEach<CarState, CarSettings, CarColor>
        {
            public Color defaultColor;
            public Color maxSpeedColor;
            public Color  minSpeedColor;
            public void Execute(
                [ReadOnly] ref CarState state,
                [ReadOnly] ref CarSettings settings,
                ref CarColor color)
            {
                if (state.FwdSpeed > settings.DefaultSpeed)
                {
                    color.Value = Color.Lerp (defaultColor, maxSpeedColor, (state.FwdSpeed - settings.DefaultSpeed) / (settings.DefaultSpeed * settings.OvertakePercent - settings.DefaultSpeed));
                }
                else if (state.FwdSpeed < settings.DefaultSpeed)
                {
                    color.Value = Color.Lerp (minSpeedColor, defaultColor, state.FwdSpeed / settings.DefaultSpeed);
                }
                else
                {
                    color.Value = defaultColor;
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new ColorSystemJob
            {
                defaultColor = Game.instance.defaultColor,
                maxSpeedColor = Game.instance.maxSpeedColor,
                minSpeedColor = Game.instance.minSpeedColor
            };
            return job.Schedule(this, inputDeps);
        }
    }
}
