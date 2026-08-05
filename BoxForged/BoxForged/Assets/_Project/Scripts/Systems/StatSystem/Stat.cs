using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Tests")]
namespace StatSystem
{
    public sealed partial class Stat
    {
        private const int DEFAULT_LIST_CAPACITY = 4;
        private const int DEFAULT_DIGIT_ACCURACY = 2;
        private const int MAXIMUM_ROUND_DIGITS = 6;

        private float _baseValue;
        private static ModifierOperationsGroups _ModifierOperationsGroups = new();
        private readonly int _digitAccuracy;
        private readonly SortedList<ModifierType, IModifiersOperations> _modifiersOperations = new();
        private float _currentValue;
        private bool _isDirty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Init() => _ModifierOperationsGroups = new ModifierOperationsGroups();

        public Stat(float baseValue, int digitAccuracy, int modsMaxCapacity)
        {
            _baseValue = baseValue;
            _currentValue = baseValue;
            _digitAccuracy = digitAccuracy;

            var modifierOperations = _ModifierOperationsGroups.GetModifierOperations(modsMaxCapacity);
            foreach (var operationType in modifierOperations.Keys)
                _modifiersOperations[operationType] = modifierOperations[operationType]();
        }

        public Stat(float baseValue) : this(baseValue, DEFAULT_DIGIT_ACCURACY, DEFAULT_LIST_CAPACITY) { }
        public Stat(float baseValue, int digitAccuracy) : this(baseValue, digitAccuracy, DEFAULT_LIST_CAPACITY) { }

        public static bool CanAddNewModifierType => !_ModifierOperationsGroups.HasCollectionBeenReturned;

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                _baseValue = value;
                _currentValue = CalculateModifiedValue(_digitAccuracy);
                OnValueChanged();
            }
        }

        public float Value
        {
            get
            {
                if (IsDirty)
                {
                    _currentValue = CalculateModifiedValue(_digitAccuracy);
                    OnValueChanged();
                }
                return _currentValue;
            }
        }

        private bool IsDirty
        {
            get => _isDirty;
            set
            {
                _isDirty = value;
                if (_isDirty) OnModifiersChanged();
            }
        }

        public event Action ValueChanged;
        public event Action ModifiersChanged;

        public static ModifierType NewModifierType(int order, Func<IModifiersOperations> modifierOperationsDelegate)
        {
            if (modifierOperationsDelegate == null)
                throw new ArgumentNullException(nameof(modifierOperationsDelegate));
            if (!CanAddNewModifierType)
                throw new InvalidOperationException("Add any modifier operations before any initialization of the Stat class!");
            return _ModifierOperationsGroups.AddModifierOperation(order, modifierOperationsDelegate);
        }

        public void AddModifier(Modifier modifier)
        {
            IsDirty = true;
            _modifiersOperations[modifier.Type].AddModifier(modifier);
        }

        public void AddModifiers(ReadOnlySpan<Modifier> modifiers)
        {
            IsDirty = true;
            foreach (var modifier in modifiers)
                _modifiersOperations[modifier.Type].AddModifier(modifier);
        }

        public void AddModifiers(params Modifier[] modifiers) => AddModifiers(modifiers.AsSpan());

        public void AddModifiers(IEnumerable<Modifier> modifiers)
        {
            if (modifiers == null) throw new ArgumentNullException(nameof(modifiers));
            IsDirty = true;
            foreach (var modifier in modifiers)
                _modifiersOperations[modifier.Type].AddModifier(modifier);
        }

        public ModifiersCollection GetModifiers()
        {
            var list = new List<Modifier>();
            foreach (var op in _modifiersOperations.Values)
                list.AddRange(op.GetAllModifiers());
            return new ModifiersCollection(list);
        }

        public ModifiersCollection GetModifiers(ModifierType modifierType)
        {
            if (!_modifiersOperations.TryGetValue(modifierType, out _))
                throw new ArgumentOutOfRangeException(nameof(modifierType), $"ModifierType {modifierType} does NOT exist!");
            return _modifiersOperations[modifierType].GetAllModifiers();
        }

        public bool TryRemoveModifier(Modifier modifier)
        {
            if (_modifiersOperations[modifier.Type].TryRemoveModifier(modifier))
            {
                IsDirty = true;
                return true;
            }
            return false;
        }

        public bool TryRemoveAllModifiersOf(object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            bool removed = false;
            for (int i = 0; i < _modifiersOperations.Count; i++)
            {
                if (TryRemoveAllModifiersOfSourceFromList(source, _modifiersOperations.Values[i].GetAllModifiers().GetModifiersList()))
                {
                    removed = true;
                    IsDirty = true;
                }
            }
            return removed;

            static bool TryRemoveAllModifiersOfSourceFromList(object src, IList<Modifier> list)
            {
                bool result = false;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(src, list[i].Source))
                    {
                        list.RemoveAt(i);
                        result = true;
                    }
                }
                return result;
            }
        }

        public void Clear()
        {
            foreach (var key in _modifiersOperations.Keys)
            {
                if (_modifiersOperations[key].GetAllModifiers().Count > 0)
                {
                    _modifiersOperations[key].Clear();
                    IsDirty = true;
                }
            }
        }

        public bool ContainsModifier(Modifier modifier) =>
            _modifiersOperations.ContainsKey(modifier.Type) &&
            _modifiersOperations[modifier.Type].GetAllModifiers().Contains(modifier);

        private float CalculateModifiedValue(int digitAccuracy)
        {
            digitAccuracy = Math.Clamp(digitAccuracy, 0, MAXIMUM_ROUND_DIGITS);
            float finalValue = _baseValue;
            for (int i = 0; i < _modifiersOperations.Count; i++)
                finalValue += _modifiersOperations.Values[i].CalculateModifiersValue(_baseValue, finalValue);
            IsDirty = false;
            return (float)Math.Round(finalValue, digitAccuracy);
        }

        private void OnValueChanged() => ValueChanged?.Invoke();
        private void OnModifiersChanged() => ModifiersChanged?.Invoke();
    }
}
