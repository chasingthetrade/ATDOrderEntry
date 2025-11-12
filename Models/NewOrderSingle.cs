using System;
using System.Collections.Generic;
using System.Text;

namespace FIXatdlOrderEntry.Models
{
    public class StrategyParameter
    {
        public string Name { get; set; }
        public int? FixTag { get; set; }
        public string Value { get; set; }
        public string WireValue { get; set; }
    }

    public class NewOrderSingle
    {
        public string ClOrdID { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public decimal? OrderQty { get; set; }
        public string OrdType { get; set; }
        public decimal? Price { get; set; }
        public decimal? StopPx { get; set; }
        public string TimeInForce { get; set; }
        public string Account { get; set; }
        public DateTime TransactTime { get; set; }
        public string HandlInst { get; set; }
        
        public string StrategyName { get; set; }
        public List<StrategyParameter> StrategyParameters { get; set; }

        public NewOrderSingle()
        {
            ClOrdID = GenerateClOrdID();
            TransactTime = DateTime.UtcNow;
            HandlInst = "1";
            StrategyParameters = new List<StrategyParameter>();
        }

        private string GenerateClOrdID()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}{new Random().Next(1000, 9999)}";
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== FIX NewOrderSingle (35=D) ===");
            sb.AppendLine();
            sb.AppendLine("=== Standard FIX Fields ===");
            sb.AppendLine($"ClOrdID (11): {ClOrdID}");
            sb.AppendLine($"Symbol (55): {Symbol}");
            sb.AppendLine($"Side (54): {Side}");
            sb.AppendLine($"OrderQty (38): {OrderQty}");
            sb.AppendLine($"OrdType (40): {OrdType}");
            
            if (Price.HasValue)
                sb.AppendLine($"Price (44): {Price:F2}");
            
            if (StopPx.HasValue)
                sb.AppendLine($"StopPx (99): {StopPx:F2}");
            
            sb.AppendLine($"TimeInForce (59): {TimeInForce}");
            
            if (!string.IsNullOrEmpty(Account))
                sb.AppendLine($"Account (1): {Account}");
            
            sb.AppendLine($"HandlInst (21): {HandlInst}");
            sb.AppendLine($"TransactTime (60): {TransactTime:yyyyMMdd-HH:mm:ss.fff}");
            
            if (!string.IsNullOrEmpty(StrategyName))
            {
                sb.AppendLine();
                sb.AppendLine($"=== Strategy: {StrategyName} ===");
            }
            
            if (StrategyParameters.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("=== Strategy Parameters ===");
                foreach (var param in StrategyParameters)
                {
                    var tagInfo = param.FixTag.HasValue ? $" ({param.FixTag})" : "";
                    var wireInfo = !string.IsNullOrEmpty(param.WireValue) && param.WireValue != param.Value 
                        ? $" [wire: {param.WireValue}]" 
                        : "";
                    sb.AppendLine($"{param.Name}{tagInfo}: {param.Value}{wireInfo}");
                }
            }
            
            return sb.ToString();
        }

        public Dictionary<int, string> ToFIXTagValuePairs()
        {
            var fixFields = new Dictionary<int, string>();
            
            fixFields[11] = ClOrdID;
            fixFields[55] = Symbol;
            fixFields[54] = Side;
            fixFields[38] = OrderQty?.ToString();
            fixFields[40] = OrdType;
            
            if (Price.HasValue)
                fixFields[44] = Price.Value.ToString("F2");
            
            if (StopPx.HasValue)
                fixFields[99] = StopPx.Value.ToString("F2");
            
            fixFields[59] = TimeInForce;
            
            if (!string.IsNullOrEmpty(Account))
                fixFields[1] = Account;
            
            fixFields[21] = HandlInst;
            fixFields[60] = TransactTime.ToString("yyyyMMdd-HH:mm:ss.fff");
            
            foreach (var param in StrategyParameters)
            {
                if (param.FixTag.HasValue && !string.IsNullOrEmpty(param.WireValue))
                {
                    fixFields[param.FixTag.Value] = param.WireValue;
                }
            }
            
            return fixFields;
        }
    }
}
