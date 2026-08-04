using System;
using System.Collections.Generic;
using UnityEngine;
using static StatSystem.ModifierType;

namespace StatSystem
{
    public sealed partial class Stat
    {
        private sealed class ModifierOperationsGroups
        {
            private readonly Dictionary<ModifierType, Func<IModifiersOperations>> _modifierOperationsDict = new();

            internal bool HasCollectionBeenReturned { get; private set; }

            internal ModifierType AddModifierOperation(int order, Func<IModifiersOperations> modifierOperationsDelegate)
            {
                if (HasCollectionBeenReturned)
                    throw new InvalidOperationException("Cannot change collection after it has been returned");

                var modifierType = (ModifierType)order;

                if (modifierType is Flat or Additive or Multiplicative)
                    Debug.LogWarning("modifier operations for types flat, additive and multiplicative cannot be changed! Default operations for these types will be used.");

                _modifierOperationsDict[modifierType] = modifierOperationsDelegate;
                return modifierType;
            }

            internal IReadOnlyDictionary<ModifierType, Func<IModifiersOperations>> GetModifierOperations(int capacity)
            {
                _modifierOperationsDict[Flat]           = () => new FlatModifierOperations(capacity);
                _modifierOperationsDict[Additive]       = () => new AdditiveModifierOperations(capacity);
                _modifierOperationsDict[Multiplicative] = () => new MultiplicativeModifierOperations(capacity);
                HasCollectionBeenReturned = true;
                return _modifierOperationsDict;
            }

            private sealed class FlatModifierOperations : ModifierOperationsBase
            {
                internal FlatModifierOperations(int capacity) : base(capacity) { }

                public override float CalculateModifiersValue(float baseValue, float currentValue)
                {
                    float sum = 0f;
                    foreach (var t in Modifiers) sum += t;
                    return sum;
                }
            }

            private sealed class AdditiveModifierOperations : ModifierOperationsBase
            {
                internal AdditiveModifierOperations(int capacity) : base(capacity) { }

                public override float CalculateModifiersValue(float baseValue, float currentValue)
                {
                    float sum = 0f;
                    foreach (var t in Modifiers) sum += t;
                    return baseValue * sum;
                }
            }

            private sealed class MultiplicativeModifierOperations : ModifierOperationsBase
            {
                internal MultiplicativeModifierOperations(int capacity) : base(capacity) { }

                public override float CalculateModifiersValue(float baseValue, float currentValue)
                {
                    float value = currentValue;
                    foreach (var t in Modifiers) value += value * t;
                    return value - currentValue;
                }
            }
        }
    }
}
