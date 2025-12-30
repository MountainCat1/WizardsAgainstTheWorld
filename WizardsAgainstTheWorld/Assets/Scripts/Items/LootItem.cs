using UnityEngine;

namespace Items
{
    public class LootItem : ItemBehaviour
    {
        public override bool Stackable => true;
        public override bool AutoSell => autoSell;

        [SerializeField] private bool autoSell = false;
    }
}