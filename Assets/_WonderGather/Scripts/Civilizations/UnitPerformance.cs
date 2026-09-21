using System;
using UnityEngine;
namespace WonderGather
{
    // Temporary outcome controls. Future body/behavior systems can resolve these outcomes.
    [Serializable]
    public struct UnitPerformance
    {
        public int movementPercent,capacity,gatheringPercent,constructionPercent;
        public static UnitPerformance Default=>new UnitPerformance(100,5,100,100);
        public UnitPerformance(int movement,int carry,int gathering,int construction)
        {movementPercent=movement;capacity=carry;gatheringPercent=gathering;constructionPercent=construction;}
        public bool IsValid=>Rate(movementPercent)&&capacity>=1&&capacity<=20&&Rate(gatheringPercent)&&Rate(constructionPercent);
        private static bool Rate(int value)=>value>=25&&value<=200;
        public void Validate(){if(!IsValid) throw new ArgumentOutOfRangeException(nameof(UnitPerformance),"Prototype rates must be 25–200% and carrying capacity 1–20.");}
    }
}
