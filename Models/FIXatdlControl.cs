using System.Collections.Generic;

namespace FIXatdlOrderEntry.Models
{
    public enum ControlType
    {
        TextField_t,
        DropDownList_t,
        SingleSpinner_t,
        CheckBox_t,
        Clock_t,
        RadioButton_t,
        Label_t,
        Slider_t
    }

    public class ListItem
    {
        public string EnumID { get; set; }
        public string UiRep { get; set; }
    }

    public class FIXatdlControl
    {
        public string ID { get; set; }
        public ControlType Type { get; set; }
        public string Label { get; set; }
        public string ParameterRef { get; set; }
        public decimal? Increment { get; set; }
        public decimal? InnerIncrement { get; set; }
        public string CheckedEnumRef { get; set; }
        public string UncheckedEnumRef { get; set; }
        public List<ListItem> ListItems { get; set; }
        public string Tooltip { get; set; }

        public FIXatdlControl()
        {
            ListItems = new List<ListItem>();
        }
    }
}
