namespace AvePoint.RA.Common.JobService
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Constraint { get; set; } = null;

        public ColumnDefinition(string name, string type, string constraint = null)
        {
            Name = name;
            Type = type;
            Constraint = constraint;
        }
    }
}
