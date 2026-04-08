namespace Models
{
    public struct StatData
    {
        public string Name;
        public string Description;
        public string Color;
        
        public StatData(
            string name,
            string description,
            string color
        )
        {
            this.Name = name;
            this.Description = description;
            this.Color = color;
        }
    }
}