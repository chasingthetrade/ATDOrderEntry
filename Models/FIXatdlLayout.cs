using System.Collections.Generic;

namespace FIXatdlOrderEntry.Models
{
    public enum PanelOrientation
    {
        VERTICAL,
        HORIZONTAL
    }

    public class StrategyPanel
    {
        public string Title { get; set; }
        public PanelOrientation Orientation { get; set; }
        public List<FIXatdlControl> Controls { get; set; }

        public StrategyPanel()
        {
            Controls = new List<FIXatdlControl>();
        }
    }

    public class StrategyLayout
    {
        public List<StrategyPanel> Panels { get; set; }

        public StrategyLayout()
        {
            Panels = new List<StrategyPanel>();
        }
    }
}
