using System.Collections.Generic;

namespace ATDOrderSystem
{
    public class ATDFieldDefinition
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string ValidationPattern { get; set; }
        public string ValidationMessage { get; set; }
        public string DefaultValue { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int? DecimalPlaces { get; set; }
        public List<string> Options { get; set; }

        public ATDFieldDefinition()
        {
            Options = new List<string>();
        }
    }
}
