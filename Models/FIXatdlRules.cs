using System.Collections.Generic;

namespace FIXatdlOrderEntry.Models
{
    public enum LogicOperator
    {
        AND,
        OR,
        XOR,
        NOT
    }

    public enum EditOperator
    {
        EQ,
        NE,
        LT,
        LE,
        GT,
        GE,
        EX,
        NX
    }

    public class Edit
    {
        public string Field { get; set; }
        public EditOperator Operator { get; set; }
        public string Value { get; set; }
        public LogicOperator? LogicOperator { get; set; }
        public List<Edit> ChildEdits { get; set; }

        public Edit()
        {
            ChildEdits = new List<Edit>();
        }
    }

    public class StrategyEdit
    {
        public string ErrorMessage { get; set; }
        public Edit RootEdit { get; set; }
    }

    public class AffectedControl
    {
        public string Id { get; set; }
    }

    public class StateRule
    {
        public bool Enabled { get; set; }
        public bool? Visible { get; set; }
        public List<Edit> Conditions { get; set; }
        public List<AffectedControl> AffectedControls { get; set; }

        public StateRule()
        {
            Conditions = new List<Edit>();
            AffectedControls = new List<AffectedControl>();
        }
    }
}
