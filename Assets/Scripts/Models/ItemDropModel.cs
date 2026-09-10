namespace Models
{
    public readonly struct ItemDropModel
    {
        public readonly int ItemId;
        public readonly float DropChance;
        public readonly bool Guaranteed;

        public ItemDropModel(
            int itemId,
            float dropChance,
            bool guaranteed = false
        )
        {
            ItemId = itemId;
            DropChance = dropChance;
            Guaranteed = guaranteed;
        }
    }
}