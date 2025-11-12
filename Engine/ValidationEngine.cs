using System;
using System.Collections.Generic;
using System.Linq;
using FIXatdlOrderEntry.Models;

namespace FIXatdlOrderEntry.Engine
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ErrorMessages { get; set; }

        public ValidationResult()
        {
            IsValid = true;
            ErrorMessages = new List<string>();
        }
    }

    public class ValidationEngine
    {
        private FIXatdlStrategy strategy;
        private Dictionary<string, object> parameterValues;

        public ValidationEngine(FIXatdlStrategy strategy)
        {
            this.strategy = strategy;
            this.parameterValues = new Dictionary<string, object>();
        }

        public void SetParameterValue(string parameterName, object value)
        {
            parameterValues[parameterName] = value;
        }

        public ValidationResult ValidateAll()
        {
            var result = new ValidationResult();

            ValidateRequiredFields(result);
            ValidateRanges(result);
            ValidateStrategyRules(result);

            result.IsValid = result.ErrorMessages.Count == 0;
            return result;
        }

        private void ValidateRequiredFields(ValidationResult result)
        {
            foreach (var parameter in strategy.Parameters)
            {
                if (parameter.Use == UseType.required)
                {
                    if (!parameterValues.ContainsKey(parameter.Name) || 
                        parameterValues[parameter.Name] == null ||
                        (parameterValues[parameter.Name] is string str && string.IsNullOrWhiteSpace(str)))
                    {
                        result.ErrorMessages.Add($"{parameter.Name} is required");
                    }
                }
            }
        }

        private void ValidateRanges(ValidationResult result)
        {
            foreach (var parameter in strategy.Parameters)
            {
                if (!parameterValues.ContainsKey(parameter.Name) || parameterValues[parameter.Name] == null)
                    continue;

                var value = parameterValues[parameter.Name];

                if (parameter.Type == ParameterType.Int_t || 
                    parameter.Type == ParameterType.Qty_t ||
                    parameter.Type == ParameterType.Float_t ||
                    parameter.Type == ParameterType.Price_t ||
                    parameter.Type == ParameterType.Amt_t)
                {
                    decimal numericValue = 0;
                    
                    if (value is decimal decVal)
                        numericValue = decVal;
                    else if (value is int intVal)
                        numericValue = intVal;
                    else if (value is double dblVal)
                        numericValue = (decimal)dblVal;
                    else if (value is string strVal && decimal.TryParse(strVal, out decimal parsedVal))
                        numericValue = parsedVal;

                    if (parameter.MinValue.HasValue && numericValue < parameter.MinValue.Value)
                    {
                        result.ErrorMessages.Add($"{parameter.Name} must be at least {parameter.MinValue.Value}");
                    }

                    if (parameter.MaxValue.HasValue && numericValue > parameter.MaxValue.Value)
                    {
                        result.ErrorMessages.Add($"{parameter.Name} cannot exceed {parameter.MaxValue.Value}");
                    }
                }
            }
        }

        private void ValidateStrategyRules(ValidationResult result)
        {
            foreach (var rule in strategy.ValidationRules)
            {
                if (rule.RootEdit != null)
                {
                    bool ruleResult = EvaluateEdit(rule.RootEdit);
                    if (!ruleResult)
                    {
                        result.ErrorMessages.Add(rule.ErrorMessage ?? "Validation rule failed");
                    }
                }
            }
        }

        public bool EvaluateEdit(Edit edit)
        {
            if (edit.ChildEdits.Count > 0)
            {
                return EvaluateLogicOperator(edit);
            }

            if (string.IsNullOrEmpty(edit.Field))
                return true;

            if (!parameterValues.ContainsKey(edit.Field))
            {
                return edit.Operator == EditOperator.NX;
            }

            var fieldValue = parameterValues[edit.Field];
            
            switch (edit.Operator)
            {
                case EditOperator.EX:
                    return fieldValue != null && 
                           !(fieldValue is string str && string.IsNullOrWhiteSpace(str));
                
                case EditOperator.NX:
                    return fieldValue == null || 
                           (fieldValue is string str && string.IsNullOrWhiteSpace(str));
                
                case EditOperator.EQ:
                    return CompareValues(fieldValue, edit.Value) == 0;
                
                case EditOperator.NE:
                    return CompareValues(fieldValue, edit.Value) != 0;
                
                case EditOperator.LT:
                    return CompareValues(fieldValue, edit.Value) < 0;
                
                case EditOperator.LE:
                    return CompareValues(fieldValue, edit.Value) <= 0;
                
                case EditOperator.GT:
                    return CompareValues(fieldValue, edit.Value) > 0;
                
                case EditOperator.GE:
                    return CompareValues(fieldValue, edit.Value) >= 0;
                
                default:
                    return true;
            }
        }

        private bool EvaluateLogicOperator(Edit edit)
        {
            if (edit.ChildEdits.Count == 0)
                return true;

            var results = edit.ChildEdits.Select(e => EvaluateEdit(e)).ToList();

            switch (edit.LogicOperator)
            {
                case Models.LogicOperator.AND:
                    return results.All(r => r);
                
                case Models.LogicOperator.OR:
                    return results.Any(r => r);
                
                case Models.LogicOperator.XOR:
                    return results.Count(r => r) == 1;
                
                case Models.LogicOperator.NOT:
                    return !results.First();
                
                default:
                    return results.All(r => r);
            }
        }

        private int CompareValues(object fieldValue, string compareValue)
        {
            if (fieldValue == null && compareValue == null)
                return 0;
            if (fieldValue == null)
                return -1;
            if (compareValue == null)
                return 1;

            var fieldStr = fieldValue.ToString();
            
            if (decimal.TryParse(fieldStr, out decimal fieldNum) && 
                decimal.TryParse(compareValue, out decimal compareNum))
            {
                return fieldNum.CompareTo(compareNum);
            }

            return string.Compare(fieldStr, compareValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}
