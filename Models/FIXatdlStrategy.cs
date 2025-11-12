using System.Collections.Generic;

namespace FIXatdlOrderEntry.Models
{
    public class FIXatdlStrategy
    {
        public string Name { get; set; }
        public string UiRep { get; set; }
        public string WireValue { get; set; }
        public string Version { get; set; }
        public string FixMsgType { get; set; }
        public string ProviderID { get; set; }
        public string Description { get; set; }
        
        public List<FIXatdlParameter> Parameters { get; set; }
        public StrategyLayout Layout { get; set; }
        public List<StrategyEdit> ValidationRules { get; set; }
        public List<StateRule> StateRules { get; set; }

        public FIXatdlStrategy()
        {
            Parameters = new List<FIXatdlParameter>();
            ValidationRules = new List<StrategyEdit>();
            StateRules = new List<StateRule>();
        }

        public FIXatdlParameter GetParameter(string name)
        {
            return Parameters.Find(p => p.Name == name);
        }
    }
}
