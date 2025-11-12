using System.Collections.Generic;

namespace FIXatdlOrderEntry.Models
{
    public enum ParameterType
    {
        String_t,
        Char_t,
        Int_t,
        Float_t,
        Qty_t,
        Price_t,
        Amt_t,
        Boolean_t,
        UTCTimestamp_t,
        LocalMktDate_t
    }

    public enum UseType
    {
        required,
        optional
    }

    public class EnumPair
    {
        public string EnumID { get; set; }
        public string WireValue { get; set; }
        public string DisplayValue { get; set; }
    }

    public class FIXatdlParameter
    {
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public int? FixTag { get; set; }
        public UseType Use { get; set; }
        public string Description { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? Increment { get; set; }
        public string TrueWireValue { get; set; }
        public string FalseWireValue { get; set; }
        public List<EnumPair> EnumPairs { get; set; }

        public FIXatdlParameter()
        {
            EnumPairs = new List<EnumPair>();
        }

        public bool HasEnumPairs => EnumPairs != null && EnumPairs.Count > 0;
    }
}
