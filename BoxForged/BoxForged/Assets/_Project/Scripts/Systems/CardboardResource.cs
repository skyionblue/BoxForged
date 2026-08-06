using System;
using UnityEngine;

namespace Boxhead.Systems
{
    public class CardboardResource : MonoBehaviour
    {
        public int Current { get; private set; }

        public event Action<int> OnCardboardChanged;

        private void Awake()     => Boxhead.Enemy.EnemyStats.OnEnemyCardboardDrop += Add;
        private void OnDestroy() => Boxhead.Enemy.EnemyStats.OnEnemyCardboardDrop -= Add;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Current += amount;
            OnCardboardChanged?.Invoke(Current);
        }

        public bool CanAfford(int cost)
        {
            return Current >= cost;
        }

        public bool Spend(int cost)
        {
            if (!CanAfford(cost)) return false;
            Current -= cost;
            OnCardboardChanged?.Invoke(Current);
            return true;
        }

        public void ResetForRun()
        {
            Current = 0;
            OnCardboardChanged?.Invoke(Current);
        }

        /// <summary>
        /// Overwrites the current cardboard count and fires OnCardboardChanged so any
        /// live UI refreshes. Used by ProgressionSystem.RestoreRunLoadout on scene load.
        /// </summary>
        public void SetCurrent(int value)
        {
            Current = Mathf.Max(0, value);
            OnCardboardChanged?.Invoke(Current);
        }
    }
}
