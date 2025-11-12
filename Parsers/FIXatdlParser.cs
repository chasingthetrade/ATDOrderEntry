using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FIXatdlOrderEntry.Models;

namespace FIXatdlOrderEntry.Parsers
{
    public class FIXatdlParser
    {
        private static readonly XNamespace CoreNs = "http://www.fixprotocol.org/FIXatdl-1-2/Core";
        private static readonly XNamespace ValNs = "http://www.fixprotocol.org/FIXatdl-1-2/Validation";
        private static readonly XNamespace LayNs = "http://www.fixprotocol.org/FIXatdl-1-2/Layout";
        private static readonly XNamespace FlowNs = "http://www.fixprotocol.org/FIXatdl-1-2/Flow";
        private static readonly XNamespace XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

        public List<FIXatdlStrategy> ParseFile(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);
            return ParseStrategies(doc);
        }

        public List<FIXatdlStrategy> ParseStrategies(XDocument doc)
        {
            var strategies = new List<FIXatdlStrategy>();
            
            var strategyElements = doc.Descendants(CoreNs + "Strategy");
            
            foreach (var strategyElement in strategyElements)
            {
                var strategy = ParseStrategy(strategyElement);
                strategies.Add(strategy);
            }
            
            return strategies;
        }

        private FIXatdlStrategy ParseStrategy(XElement strategyElement)
        {
            var strategy = new FIXatdlStrategy
            {
                Name = strategyElement.Attribute("name")?.Value,
                UiRep = strategyElement.Attribute("uiRep")?.Value,
                WireValue = strategyElement.Attribute("wireValue")?.Value,
                Version = strategyElement.Attribute("version")?.Value,
                FixMsgType = strategyElement.Attribute("fixMsgType")?.Value,
                ProviderID = strategyElement.Attribute("providerID")?.Value,
                Description = strategyElement.Element(CoreNs + "Description")?.Value
            };

            strategy.Parameters = ParseParameters(strategyElement);
            strategy.Layout = ParseLayout(strategyElement);
            strategy.ValidationRules = ParseValidationRules(strategyElement);
            strategy.StateRules = ParseStateRules(strategyElement);

            return strategy;
        }

        private List<FIXatdlParameter> ParseParameters(XElement strategyElement)
        {
            var parameters = new List<FIXatdlParameter>();
            
            var parameterElements = strategyElement.Elements(CoreNs + "Parameter");
            
            foreach (var paramElement in parameterElements)
            {
                var parameter = new FIXatdlParameter
                {
                    Name = paramElement.Attribute("name")?.Value,
                    Description = paramElement.Element(CoreNs + "Description")?.Value
                };

                var typeAttr = paramElement.Attribute(XsiNs + "type")?.Value;
                if (typeAttr != null && Enum.TryParse(typeAttr, out ParameterType paramType))
                {
                    parameter.Type = paramType;
                }

                var fixTagAttr = paramElement.Attribute("fixTag")?.Value;
                if (fixTagAttr != null && int.TryParse(fixTagAttr, out int fixTag))
                {
                    parameter.FixTag = fixTag;
                }

                var useAttr = paramElement.Attribute("use")?.Value;
                if (useAttr != null && Enum.TryParse(useAttr, out UseType useType))
                {
                    parameter.Use = useType;
                }

                var minValueAttr = paramElement.Attribute("minValue")?.Value;
                if (minValueAttr != null && decimal.TryParse(minValueAttr, out decimal minValue))
                {
                    parameter.MinValue = minValue;
                }

                var maxValueAttr = paramElement.Attribute("maxValue")?.Value;
                if (maxValueAttr != null && decimal.TryParse(maxValueAttr, out decimal maxValue))
                {
                    parameter.MaxValue = maxValue;
                }

                var incrementAttr = paramElement.Attribute("increment")?.Value;
                if (incrementAttr != null && decimal.TryParse(incrementAttr, out decimal increment))
                {
                    parameter.Increment = increment;
                }

                parameter.TrueWireValue = paramElement.Attribute("trueWireValue")?.Value;
                parameter.FalseWireValue = paramElement.Attribute("falseWireValue")?.Value;

                var enumPairElements = paramElement.Elements(CoreNs + "EnumPair");
                foreach (var enumPairElement in enumPairElements)
                {
                    var enumPair = new EnumPair
                    {
                        EnumID = enumPairElement.Attribute("enumID")?.Value,
                        WireValue = enumPairElement.Attribute("wireValue")?.Value,
                        DisplayValue = enumPairElement.Value
                    };
                    parameter.EnumPairs.Add(enumPair);
                }

                parameters.Add(parameter);
            }
            
            return parameters;
        }

        private StrategyLayout ParseLayout(XElement strategyElement)
        {
            var layout = new StrategyLayout();
            
            var layoutElement = strategyElement.Element(LayNs + "StrategyLayout");
            if (layoutElement == null) return layout;

            var panelElements = layoutElement.Elements(LayNs + "StrategyPanel");
            
            foreach (var panelElement in panelElements)
            {
                var panel = new StrategyPanel
                {
                    Title = panelElement.Attribute("title")?.Value
                };

                var orientationAttr = panelElement.Attribute("orientation")?.Value;
                if (orientationAttr != null && Enum.TryParse(orientationAttr, out PanelOrientation orientation))
                {
                    panel.Orientation = orientation;
                }

                var controlElements = panelElement.Elements(LayNs + "Control");
                foreach (var controlElement in controlElements)
                {
                    var control = ParseControl(controlElement);
                    if (control != null)
                    {
                        panel.Controls.Add(control);
                    }
                }

                layout.Panels.Add(panel);
            }
            
            return layout;
        }

        private FIXatdlControl ParseControl(XElement controlElement)
        {
            var control = new FIXatdlControl
            {
                ID = controlElement.Attribute("ID")?.Value,
                Label = controlElement.Attribute("label")?.Value,
                ParameterRef = controlElement.Attribute("parameterRef")?.Value,
                Tooltip = controlElement.Attribute("tooltip")?.Value,
                CheckedEnumRef = controlElement.Attribute("checkedEnumRef")?.Value,
                UncheckedEnumRef = controlElement.Attribute("uncheckedEnumRef")?.Value
            };

            var typeAttr = controlElement.Attribute(XsiNs + "type")?.Value;
            if (typeAttr != null)
            {
                var typeName = typeAttr.Replace("lay:", "");
                if (Enum.TryParse(typeName, out ControlType controlType))
                {
                    control.Type = controlType;
                }
            }

            var incrementAttr = controlElement.Attribute("increment")?.Value;
            if (incrementAttr != null && decimal.TryParse(incrementAttr, out decimal increment))
            {
                control.Increment = increment;
            }

            var innerIncrementAttr = controlElement.Attribute("innerIncrement")?.Value;
            if (innerIncrementAttr != null && decimal.TryParse(innerIncrementAttr, out decimal innerIncrement))
            {
                control.InnerIncrement = innerIncrement;
            }

            var listItemElements = controlElement.Elements(LayNs + "ListItem");
            foreach (var listItemElement in listItemElements)
            {
                var listItem = new ListItem
                {
                    EnumID = listItemElement.Attribute("enumID")?.Value,
                    UiRep = listItemElement.Attribute("uiRep")?.Value
                };
                control.ListItems.Add(listItem);
            }

            return control;
        }

        private List<StrategyEdit> ParseValidationRules(XElement strategyElement)
        {
            var validationRules = new List<StrategyEdit>();
            
            var strategyEditElements = strategyElement.Elements(ValNs + "StrategyEdit");
            
            foreach (var editElement in strategyEditElements)
            {
                var strategyEdit = new StrategyEdit
                {
                    ErrorMessage = editElement.Attribute("errorMessage")?.Value
                };

                var rootEditElement = editElement.Element(ValNs + "Edit");
                if (rootEditElement != null)
                {
                    strategyEdit.RootEdit = ParseEdit(rootEditElement);
                }

                validationRules.Add(strategyEdit);
            }
            
            return validationRules;
        }

        private Edit ParseEdit(XElement editElement)
        {
            var edit = new Edit
            {
                Field = editElement.Attribute("field")?.Value,
                Value = editElement.Attribute("value")?.Value
            };

            var operatorAttr = editElement.Attribute("operator")?.Value;
            if (operatorAttr != null && Enum.TryParse(operatorAttr, out EditOperator editOperator))
            {
                edit.Operator = editOperator;
            }

            var logicOperatorAttr = editElement.Attribute("logicOperator")?.Value;
            if (logicOperatorAttr != null && Enum.TryParse(logicOperatorAttr, out LogicOperator logicOperator))
            {
                edit.LogicOperator = logicOperator;
            }

            var childEditElements = editElement.Elements(ValNs + "Edit");
            foreach (var childEditElement in childEditElements)
            {
                var childEdit = ParseEdit(childEditElement);
                edit.ChildEdits.Add(childEdit);
            }

            return edit;
        }

        private List<StateRule> ParseStateRules(XElement strategyElement)
        {
            var stateRules = new List<StateRule>();
            
            var stateRuleElements = strategyElement.Elements(FlowNs + "StateRule");
            
            foreach (var ruleElement in stateRuleElements)
            {
                var stateRule = new StateRule();

                var enabledAttr = ruleElement.Attribute("enabled")?.Value;
                if (enabledAttr != null && bool.TryParse(enabledAttr, out bool enabled))
                {
                    stateRule.Enabled = enabled;
                }

                var visibleAttr = ruleElement.Attribute("visible")?.Value;
                if (visibleAttr != null && bool.TryParse(visibleAttr, out bool visible))
                {
                    stateRule.Visible = visible;
                }

                var editElements = ruleElement.Elements(ValNs + "Edit");
                foreach (var editElement in editElements)
                {
                    var edit = ParseEdit(editElement);
                    stateRule.Conditions.Add(edit);
                }

                var affectedControlElements = ruleElement.Elements(FlowNs + "AffectedControl");
                foreach (var affectedElement in affectedControlElements)
                {
                    var affectedControl = new AffectedControl
                    {
                        Id = affectedElement.Attribute("id")?.Value
                    };
                    stateRule.AffectedControls.Add(affectedControl);
                }

                stateRules.Add(stateRule);
            }
            
            return stateRules;
        }
    }
}
