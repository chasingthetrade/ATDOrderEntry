using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FIXatdlOrderEntry.Models;
using FIXatdlOrderEntry.Engine;
using FIXatdlOrderEntry.Parsers;

namespace FIXatdlOrderEntry.UI
{
    public partial class OrderEntryWindow : Window
    {
        private FIXatdlStrategy strategy;
        private Dictionary<string, Control> controlMap;
        private Dictionary<string, FIXatdlParameter> parameterMap;
        private ValidationEngine validationEngine;
        private StateRulesEngine stateRulesEngine;

        public NewOrderSingle CreatedOrder { get; private set; }

        public OrderEntryWindow()
        {
            InitializeComponent();
            controlMap = new Dictionary<string, Control>();
            parameterMap = new Dictionary<string, FIXatdlParameter>();
            LoadStrategy();
        }

        private void LoadStrategy()
        {
            try
            {
                var parser = new FIXatdlParser();
                string xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleStrategy.xml");
                var strategies = parser.ParseFile(xmlPath);

                if (strategies.Count > 0)
                {
                    strategy = strategies[0];
                    StrategyNameTextBlock.Text = $"Strategy: {strategy.UiRep} v{strategy.Version}";
                    
                    validationEngine = new ValidationEngine(strategy);
                    stateRulesEngine = new StateRulesEngine(strategy);
                    
                    foreach (var param in strategy.Parameters)
                    {
                        parameterMap[param.Name] = param;
                    }
                    
                    BuildUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading strategy: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildUI()
        {
            MainPanel.Children.Clear();

            if (strategy.Layout == null || strategy.Layout.Panels.Count == 0)
                return;

            foreach (var panel in strategy.Layout.Panels)
            {
                var groupBox = CreateStrategyPanel(panel);
                MainPanel.Children.Add(groupBox);
            }
        }

        private GroupBox CreateStrategyPanel(StrategyPanel panel)
        {
            var groupBox = new GroupBox
            {
                Header = panel.Title,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var stackPanel = new StackPanel
            {
                Orientation = panel.Orientation == PanelOrientation.VERTICAL 
                    ? Orientation.Vertical 
                    : Orientation.Horizontal
            };

            foreach (var control in panel.Controls)
            {
                var controlElement = CreateControl(control);
                if (controlElement != null)
                {
                    stackPanel.Children.Add(controlElement);
                }
            }

            groupBox.Content = stackPanel;
            return groupBox;
        }

        private UIElement CreateControl(FIXatdlControl controlDef)
        {
            var parameter = parameterMap.ContainsKey(controlDef.ParameterRef) 
                ? parameterMap[controlDef.ParameterRef] 
                : null;

            if (parameter == null)
                return null;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new Label
            {
                Content = controlDef.Label + (parameter.Use == UseType.required ? " *" : ""),
                ToolTip = parameter.Description
            };
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            Control inputControl = null;

            switch (controlDef.Type)
            {
                case ControlType.TextField_t:
                    inputControl = CreateTextField(controlDef, parameter);
                    break;

                case ControlType.DropDownList_t:
                    inputControl = CreateDropDownList(controlDef, parameter);
                    break;

                case ControlType.SingleSpinner_t:
                    inputControl = CreateSpinner(controlDef, parameter);
                    break;

                case ControlType.CheckBox_t:
                    inputControl = CreateCheckBox(controlDef, parameter);
                    break;

                case ControlType.Clock_t:
                    inputControl = CreateClock(controlDef, parameter);
                    break;
            }

            if (inputControl != null)
            {
                inputControl.Tag = new ControlTag 
                { 
                    ControlDef = controlDef, 
                    Parameter = parameter 
                };
                
                controlMap[controlDef.ID] = inputControl;
                Grid.SetColumn(inputControl, 1);
                grid.Children.Add(inputControl);
            }

            return grid;
        }

        private TextBox CreateTextField(FIXatdlControl controlDef, FIXatdlParameter parameter)
        {
            var textBox = new TextBox
            {
                ToolTip = parameter.Description
            };

            textBox.TextChanged += (s, e) => OnValueChanged(controlDef.ParameterRef);

            return textBox;
        }

        private ComboBox CreateDropDownList(FIXatdlControl controlDef, FIXatdlParameter parameter)
        {
            var comboBox = new ComboBox
            {
                ToolTip = parameter.Description
            };

            comboBox.Items.Add(new ComboBoxItem { Content = "", Tag = null });

            foreach (var listItem in controlDef.ListItems)
            {
                var enumPair = parameter.EnumPairs.FirstOrDefault(e => e.EnumID == listItem.EnumID);
                if (enumPair != null)
                {
                    var item = new ComboBoxItem
                    {
                        Content = listItem.UiRep,
                        Tag = enumPair
                    };
                    comboBox.Items.Add(item);
                }
            }

            comboBox.SelectedIndex = 0;
            comboBox.SelectionChanged += (s, e) => OnValueChanged(controlDef.ParameterRef);

            return comboBox;
        }

        private Control CreateSpinner(FIXatdlControl controlDef, FIXatdlParameter parameter)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textBox = new TextBox
            {
                ToolTip = parameter.Description,
                Text = "0"
            };
            Grid.SetColumn(textBox, 0);
            grid.Children.Add(textBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5, 0, 0, 0) };
            
            var upButton = new Button { Content = "▲", Width = 25, Height = 15, Padding = new Thickness(0) };
            var downButton = new Button { Content = "▼", Width = 25, Height = 15, Padding = new Thickness(0) };

            decimal increment = controlDef.Increment ?? parameter.Increment ?? 1;

            upButton.Click += (s, e) =>
            {
                if (decimal.TryParse(textBox.Text, out decimal value))
                {
                    value += increment;
                    if (parameter.MaxValue.HasValue && value > parameter.MaxValue.Value)
                        value = parameter.MaxValue.Value;
                    textBox.Text = value.ToString();
                }
            };

            downButton.Click += (s, e) =>
            {
                if (decimal.TryParse(textBox.Text, out decimal value))
                {
                    value -= increment;
                    if (parameter.MinValue.HasValue && value < parameter.MinValue.Value)
                        value = parameter.MinValue.Value;
                    textBox.Text = value.ToString();
                }
            };

            buttonPanel.Children.Add(upButton);
            buttonPanel.Children.Add(downButton);
            Grid.SetColumn(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            textBox.TextChanged += (s, e) => OnValueChanged(controlDef.ParameterRef);

            return grid;
        }

        private CheckBox CreateCheckBox(FIXatdlControl controlDef, FIXatdlParameter parameter)
        {
            var checkBox = new CheckBox
            {
                Content = "",
                ToolTip = parameter.Description
            };

            checkBox.Checked += (s, e) => OnValueChanged(controlDef.ParameterRef);
            checkBox.Unchecked += (s, e) => OnValueChanged(controlDef.ParameterRef);

            return checkBox;
        }

        private DatePicker CreateClock(FIXatdlControl controlDef, FIXatdlParameter parameter)
        {
            var datePicker = new DatePicker
            {
                ToolTip = parameter.Description,
                SelectedDate = DateTime.Now
            };

            datePicker.SelectedDateChanged += (s, e) => OnValueChanged(controlDef.ParameterRef);

            return datePicker;
        }

        private void OnValueChanged(string parameterName)
        {
            var value = GetParameterValue(parameterName);
            validationEngine.SetParameterValue(parameterName, value);
            stateRulesEngine.SetParameterValue(parameterName, value);
            
            UpdateControlStates();
            ValidationPanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateControlStates()
        {
            var controlStates = stateRulesEngine.EvaluateAllRules();

            foreach (var kvp in controlStates)
            {
                if (controlMap.ContainsKey(kvp.Key))
                {
                    var control = controlMap[kvp.Key];
                    control.IsEnabled = kvp.Value.IsEnabled;
                    control.Visibility = kvp.Value.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private object GetParameterValue(string parameterName)
        {
            var parameter = parameterMap[parameterName];
            var controlDef = strategy.Layout.Panels
                .SelectMany(p => p.Controls)
                .FirstOrDefault(c => c.ParameterRef == parameterName);

            if (controlDef == null || !controlMap.ContainsKey(controlDef.ID))
                return null;

            var control = controlMap[controlDef.ID];

            if (control is TextBox textBox)
            {
                return textBox.Text;
            }
            else if (control is ComboBox comboBox)
            {
                var selectedItem = comboBox.SelectedItem as ComboBoxItem;
                if (selectedItem?.Tag is EnumPair enumPair)
                {
                    return enumPair.EnumID;
                }
                return null;
            }
            else if (control is CheckBox checkBox)
            {
                return checkBox.IsChecked == true;
            }
            else if (control is DatePicker datePicker)
            {
                return datePicker.SelectedDate;
            }
            else if (control is Grid grid && grid.Children[0] is TextBox spinnerTextBox)
            {
                if (decimal.TryParse(spinnerTextBox.Text, out decimal value))
                    return value;
                return 0;
            }

            return null;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var param in strategy.Parameters)
            {
                var value = GetParameterValue(param.Name);
                validationEngine.SetParameterValue(param.Name, value);
            }

            var validationResult = validationEngine.ValidateAll();

            if (!validationResult.IsValid)
            {
                ValidationErrorsTextBlock.Text = string.Join("\n", validationResult.ErrorMessages);
                ValidationPanel.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                CreatedOrder = CreateOrderFromForm();
                
                var resultWindow = new Window
                {
                    Title = "Order Created Successfully",
                    Width = 600,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var textBlock = new TextBlock
                {
                    Text = CreatedOrder.ToString(),
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    Padding = new Thickness(20),
                    TextWrapping = TextWrapping.Wrap
                };
                scrollViewer.Content = textBlock;
                resultWindow.Content = scrollViewer;
                
                resultWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating order: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private NewOrderSingle CreateOrderFromForm()
        {
            var order = new NewOrderSingle
            {
                StrategyName = strategy.Name
            };

            foreach (var param in strategy.Parameters)
            {
                var value = GetParameterValue(param.Name);
                
                if (value == null)
                    continue;

                string wireValue = null;
                string displayValue = value.ToString();

                if (param.Type == ParameterType.Boolean_t && value is bool boolVal)
                {
                    wireValue = boolVal ? param.TrueWireValue : param.FalseWireValue;
                    displayValue = boolVal.ToString();
                }
                else if (param.HasEnumPairs && value is string enumId)
                {
                    var enumPair = param.EnumPairs.FirstOrDefault(e => e.EnumID == enumId);
                    if (enumPair != null)
                    {
                        wireValue = enumPair.WireValue;
                        displayValue = enumPair.DisplayValue;
                    }
                }
                else
                {
                    wireValue = value.ToString();
                }

                switch (param.Name)
                {
                    case "Symbol":
                        order.Symbol = wireValue;
                        break;
                    case "Side":
                        order.Side = wireValue;
                        break;
                    case "OrderQty":
                        if (decimal.TryParse(wireValue, out decimal qty))
                            order.OrderQty = qty;
                        break;
                    case "OrdType":
                        order.OrdType = wireValue;
                        break;
                    case "Price":
                        if (decimal.TryParse(wireValue, out decimal price))
                            order.Price = price;
                        break;
                    case "StopPx":
                        if (decimal.TryParse(wireValue, out decimal stopPx))
                            order.StopPx = stopPx;
                        break;
                    case "TimeInForce":
                        order.TimeInForce = wireValue;
                        break;
                    case "Account":
                        order.Account = wireValue;
                        break;
                    default:
                        order.StrategyParameters.Add(new StrategyParameter
                        {
                            Name = param.Name,
                            FixTag = param.FixTag,
                            Value = displayValue,
                            WireValue = wireValue
                        });
                        break;
                }
            }

            return order;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var control in controlMap.Values)
            {
                if (control is TextBox textBox)
                {
                    textBox.Text = "";
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = 0;
                }
                else if (control is CheckBox checkBox)
                {
                    checkBox.IsChecked = false;
                }
                else if (control is DatePicker datePicker)
                {
                    datePicker.SelectedDate = DateTime.Now;
                }
                else if (control is Grid grid && grid.Children[0] is TextBox spinnerTextBox)
                {
                    spinnerTextBox.Text = "0";
                }
            }

            ValidationPanel.Visibility = Visibility.Collapsed;
        }

        private class ControlTag
        {
            public FIXatdlControl ControlDef { get; set; }
            public FIXatdlParameter Parameter { get; set; }
        }
    }
}
