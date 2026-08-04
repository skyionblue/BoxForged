using UnityEngine;
using TMPro;
using Boxhead.Systems;

namespace Boxhead.UI
{
    // Temporary debug display showing material bag and cardboard count.
    // Remove when WeaponHUDSlots is fully wired up.
    public class MaterialBagDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        private WeaponInventory _inventory;
        private CardboardResource _cardboard;

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            _inventory = player.GetComponent<WeaponInventory>();
            _cardboard = player.GetComponent<CardboardResource>();

            if (_inventory != null) _inventory.OnInventoryChanged += Refresh;
            if (_cardboard != null) _cardboard.OnCardboardChanged += OnCardboardChanged;

            Refresh();
        }

        private void OnDestroy()
        {
            if (_inventory != null) _inventory.OnInventoryChanged -= Refresh;
            if (_cardboard != null) _cardboard.OnCardboardChanged -= OnCardboardChanged;
        }

        private void OnCardboardChanged(int _) => Refresh();

        private void Refresh()
        {
            if (_text == null) return;

            var bag = new System.Text.StringBuilder();
            bag.Append("Cardboard: ");
            bag.Append(_cardboard != null ? _cardboard.Current.ToString() : "?");
            bag.Append("  |  Bag: ");

            if (_inventory != null)
            {
                int count = 0;
                for (int i = 0; i < WeaponInventory.MaterialBagCapacity; i++)
                {
                    var item = _inventory.GetMaterialBagItem(i);
                    if (item != null)
                    {
                        if (count > 0) bag.Append(", ");
                        bag.Append(item.rawObjectName);
                        count++;
                    }
                }
                if (count == 0) bag.Append("[empty]");
            }

            _text.SetText(bag);
        }
    }
}
