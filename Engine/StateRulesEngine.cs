using System;
using System.Collections.Generic;
using System.Linq;
using FIXatdlOrderEntry.Models;

namespace FIXatdlOrderEntry.Engine
{
    public class ControlState
    {
        public string ControlId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsVisible { get; set; }

        public ControlState(string controlId)
        {
            ControlId = controlId;
            IsEnabled = true;
            IsVisible = true;
        }
    }

    public class StateRulesEngine
    {
        private FIXatdlStrategy strategy;
        private Dictionary<string, object> parameterValues;
        private ValidationEngine validationEngine;

        public StateRulesEngine(FIXatdlStrategy strategy)
        {
            this.strategy = strategy;
            this.parameterValues = new Dictionary<string, object>();
            this.validationEngine = new ValidationEngine(strategy);
        }

        public void SetParameterValue(string parameterName, object value)
        {
            parameterValues[parameterName] = value;
            validationEngine.SetParameterValue(parameterName, value);
        }

        public Dictionary<string, ControlState> EvaluateAllRules()
        {
            var controlStates = new Dictionary<string, ControlState>();

            foreach (var rule in strategy.StateRules)
            {
                bool conditionsMet = EvaluateRuleConditions(rule);

                foreach (var affectedControl in rule.AffectedControls)
                {
                    if (!controlStates.ContainsKey(affectedControl.Id))
                    {
                        controlStates[affectedControl.Id] = new ControlState(affectedControl.Id);
                    }

                    var state = controlStates[affectedControl.Id];

                    if (conditionsMet)
                    {
                        state.IsEnabled = rule.Enabled;
                        if (rule.Visible.HasValue)
                        {
                            state.IsVisible = rule.Visible.Value;
                        }
                    }
                }
            }

            return controlStates;
        }

        private bool EvaluateRuleConditions(StateRule rule)
        {
            if (rule.Conditions.Count == 0)
                return true;

            var results = rule.Conditions.Select(condition => validationEngine.EvaluateEdit(condition)).ToList();

            return results.All(r => r);
        }

        public ControlState GetControlState(string controlId)
        {
            var allStates = EvaluateAllRules();
            
            if (allStates.ContainsKey(controlId))
            {
                return allStates[controlId];
            }

            return new ControlState(controlId);
        }
    }
}
