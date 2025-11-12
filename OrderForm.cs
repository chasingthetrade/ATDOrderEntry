using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ATDOrderSystem
{
    public class OrderForm : Form
    {
        private ATDOrderDefinitionParser parser;
        private Dictionary<string, Control> fieldControls;
        private Dictionary<string, Label> errorLabels;
        private Button submitButton;
        private Panel mainPanel;
        private int currentY = 20;

        public Order CreatedOrder { get; private set; }

        public OrderForm()
        {
            fieldControls = new Dictionary<string, Control>();
            errorLabels = new Dictionary<string, Label>();
            InitializeForm();
            LoadDefinitionAndBuildForm();
        }

        private void InitializeForm()
        {
            this.Text = "ATD Order Entry System";
            this.Size = new Size(700, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScroll = true;

            mainPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(680, 2000),
                AutoScroll = true
            };
            this.Controls.Add(mainPanel);
        }

        private void LoadDefinitionAndBuildForm()
        {
            try
            {
                parser = new ATDOrderDefinitionParser();
                string xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OrderDefinition.xml");
                parser.LoadFromFile(xmlPath);

                Label titleLabel = new Label
                {
                    Text = parser.Title,
                    Font = new Font("Arial", 16, FontStyle.Bold),
                    Location = new Point(20, currentY),
                    Size = new Size(640, 30),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                mainPanel.Controls.Add(titleLabel);
                currentY += 50;

                foreach (var field in parser.Fields)
                {
                    CreateFieldControl(field);
                }

                submitButton = new Button
                {
                    Text = "Submit Order",
                    Location = new Point(250, currentY),
                    Size = new Size(150, 40),
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                submitButton.Click += SubmitButton_Click;
                mainPanel.Controls.Add(submitButton);
                currentY += 60;

                mainPanel.Height = currentY;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order definition: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateFieldControl(ATDFieldDefinition field)
        {
            Label label = new Label
            {
                Text = field.Label + (field.Required ? " *" : ""),
                Location = new Point(20, currentY),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10, FontStyle.Regular)
            };
            mainPanel.Controls.Add(label);

            Control inputControl = null;

            switch (field.Type.ToLower())
            {
                case "text":
                case "email":
                case "phone":
                    TextBox textBox = new TextBox
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 10)
                    };
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        textBox.Text = field.DefaultValue;
                    }
                    if (field.MaxLength.HasValue)
                    {
                        textBox.MaxLength = field.MaxLength.Value;
                    }
                    inputControl = textBox;
                    break;

                case "textarea":
                    TextBox textArea = new TextBox
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 60),
                        Multiline = true,
                        Font = new Font("Arial", 10),
                        ScrollBars = ScrollBars.Vertical
                    };
                    if (field.MaxLength.HasValue)
                    {
                        textArea.MaxLength = field.MaxLength.Value;
                    }
                    inputControl = textArea;
                    currentY += 40;
                    break;

                case "date":
                    DateTimePicker datePicker = new DateTimePicker
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 10),
                        Format = DateTimePickerFormat.Short
                    };
                    inputControl = datePicker;
                    break;

                case "number":
                    NumericUpDown numericUpDown = new NumericUpDown
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 10)
                    };
                    if (field.MinValue.HasValue)
                    {
                        numericUpDown.Minimum = field.MinValue.Value;
                    }
                    if (field.MaxValue.HasValue)
                    {
                        numericUpDown.Maximum = field.MaxValue.Value;
                    }
                    inputControl = numericUpDown;
                    break;

                case "decimal":
                    NumericUpDown decimalUpDown = new NumericUpDown
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 10),
                        DecimalPlaces = field.DecimalPlaces ?? 2
                    };
                    if (field.MinValue.HasValue)
                    {
                        decimalUpDown.Minimum = field.MinValue.Value;
                    }
                    if (field.MaxValue.HasValue)
                    {
                        decimalUpDown.Maximum = field.MaxValue.Value;
                    }
                    inputControl = decimalUpDown;
                    break;

                case "dropdown":
                    ComboBox comboBox = new ComboBox
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 10),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    comboBox.Items.Add("");
                    foreach (var option in field.Options)
                    {
                        comboBox.Items.Add(option);
                    }
                    comboBox.SelectedIndex = 0;
                    inputControl = comboBox;
                    break;

                case "checkbox":
                    CheckBox checkBox = new CheckBox
                    {
                        Location = new Point(230, currentY),
                        Size = new Size(400, 25),
                        Font = new Font("Arial", 10)
                    };
                    if (!string.IsNullOrEmpty(field.DefaultValue))
                    {
                        checkBox.Checked = bool.Parse(field.DefaultValue);
                    }
                    inputControl = checkBox;
                    break;
            }

            if (inputControl != null)
            {
                inputControl.Tag = field;
                fieldControls[field.Name] = inputControl;
                mainPanel.Controls.Add(inputControl);

                Label errorLabel = new Label
                {
                    Location = new Point(230, currentY + 28),
                    Size = new Size(400, 20),
                    ForeColor = Color.Red,
                    Font = new Font("Arial", 8),
                    Text = ""
                };
                errorLabels[field.Name] = errorLabel;
                mainPanel.Controls.Add(errorLabel);

                currentY += 55;
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            ClearAllErrors();

            if (ValidateAllFields())
            {
                try
                {
                    CreatedOrder = CreateOrderFromForm();
                    
                    MessageBox.Show(CreatedOrder.ToString(), "Order Created Successfully", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    DialogResult result = MessageBox.Show("Would you like to create another order?", "New Order", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        ClearForm();
                    }
                    else
                    {
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please correct the errors in the form before submitting.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateAllFields()
        {
            bool isValid = true;

            foreach (var kvp in fieldControls)
            {
                string fieldName = kvp.Key;
                Control control = kvp.Value;
                ATDFieldDefinition field = (ATDFieldDefinition)control.Tag;

                if (!ValidateField(field, control))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidateField(ATDFieldDefinition field, Control control)
        {
            string errorMessage = "";

            if (control is TextBox textBox)
            {
                string value = textBox.Text.Trim();

                if (field.Required && string.IsNullOrEmpty(value))
                {
                    errorMessage = field.ValidationMessage ?? $"{field.Label} is required";
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    if (field.MinLength.HasValue && value.Length < field.MinLength.Value)
                    {
                        errorMessage = field.ValidationMessage ?? $"{field.Label} must be at least {field.MinLength.Value} characters";
                    }
                    else if (field.MaxLength.HasValue && value.Length > field.MaxLength.Value)
                    {
                        errorMessage = field.ValidationMessage ?? $"{field.Label} cannot exceed {field.MaxLength.Value} characters";
                    }
                    else if (!string.IsNullOrEmpty(field.ValidationPattern))
                    {
                        if (!Regex.IsMatch(value, field.ValidationPattern))
                        {
                            errorMessage = field.ValidationMessage ?? $"{field.Label} format is invalid";
                        }
                    }
                }
            }
            else if (control is ComboBox comboBox)
            {
                if (field.Required && (comboBox.SelectedIndex <= 0 || string.IsNullOrEmpty(comboBox.Text)))
                {
                    errorMessage = field.ValidationMessage ?? $"{field.Label} is required";
                }
            }
            else if (control is DateTimePicker datePicker)
            {
                if (field.Required && datePicker.Value == null)
                {
                    errorMessage = field.ValidationMessage ?? $"{field.Label} is required";
                }
            }
            else if (control is NumericUpDown numericUpDown)
            {
                if (field.Required && numericUpDown.Value == 0)
                {
                    errorMessage = field.ValidationMessage ?? $"{field.Label} is required";
                }
                else if (field.MinValue.HasValue && numericUpDown.Value < field.MinValue.Value)
                {
                    errorMessage = field.ValidationMessage ?? $"{field.Label} must be at least {field.MinValue.Value}";
                }
                else if (field.MaxValue.HasValue && numericUpDown.Value > field.MaxValue.Value)
                {
                    errorMessage = field.ValidationMessage ?? $"{field.Label} cannot exceed {field.MaxValue.Value}";
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                errorLabels[field.Name].Text = errorMessage;
                control.BackColor = Color.FromArgb(255, 240, 240);
                return false;
            }

            return true;
        }

        private void ClearAllErrors()
        {
            foreach (var errorLabel in errorLabels.Values)
            {
                errorLabel.Text = "";
            }

            foreach (var control in fieldControls.Values)
            {
                control.BackColor = Color.White;
            }
        }

        private Order CreateOrderFromForm()
        {
            Order order = new Order();

            order.OrderNumber = GetTextValue("OrderNumber");
            order.CustomerName = GetTextValue("CustomerName");
            order.CustomerEmail = GetTextValue("CustomerEmail");
            order.CustomerPhone = GetTextValue("CustomerPhone");
            order.OrderDate = GetDateValue("OrderDate");
            order.ShippingAddress = GetTextValue("ShippingAddress");
            order.City = GetTextValue("City");
            order.State = GetComboValue("State");
            order.ZipCode = GetTextValue("ZipCode");
            order.ProductName = GetTextValue("ProductName");
            order.Quantity = GetNumericValue("Quantity");
            order.UnitPrice = GetDecimalValue("UnitPrice");
            order.ShippingMethod = GetComboValue("ShippingMethod");
            order.PaymentMethod = GetComboValue("PaymentMethod");
            order.SpecialInstructions = GetTextValue("SpecialInstructions");
            order.GiftWrap = GetCheckboxValue("GiftWrap");
            order.Newsletter = GetCheckboxValue("Newsletter");

            return order;
        }

        private string GetTextValue(string fieldName)
        {
            if (fieldControls.ContainsKey(fieldName) && fieldControls[fieldName] is TextBox textBox)
            {
                return textBox.Text.Trim();
            }
            return string.Empty;
        }

        private string GetComboValue(string fieldName)
        {
            if (fieldControls.ContainsKey(fieldName) && fieldControls[fieldName] is ComboBox comboBox)
            {
                return comboBox.Text;
            }
            return string.Empty;
        }

        private DateTime GetDateValue(string fieldName)
        {
            if (fieldControls.ContainsKey(fieldName) && fieldControls[fieldName] is DateTimePicker datePicker)
            {
                return datePicker.Value;
            }
            return DateTime.Now;
        }

        private int GetNumericValue(string fieldName)
        {
            if (fieldControls.ContainsKey(fieldName) && fieldControls[fieldName] is NumericUpDown numericUpDown)
            {
                return (int)numericUpDown.Value;
            }
            return 0;
        }

        private decimal GetDecimalValue(string fieldName)
        {
            if (fieldControls.ContainsKey(fieldName) && fieldControls[fieldName] is NumericUpDown numericUpDown)
            {
                return numericUpDown.Value;
            }
            return 0;
        }

        private bool GetCheckboxValue(string fieldName)
        {
            if (fieldControls.ContainsKey(fieldName) && fieldControls[fieldName] is CheckBox checkBox)
            {
                return checkBox.Checked;
            }
            return false;
        }

        private void ClearForm()
        {
            foreach (var kvp in fieldControls)
            {
                Control control = kvp.Value;

                if (control is TextBox textBox)
                {
                    ATDFieldDefinition field = (ATDFieldDefinition)control.Tag;
                    textBox.Text = field.DefaultValue ?? "";
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = 0;
                }
                else if (control is DateTimePicker datePicker)
                {
                    datePicker.Value = DateTime.Now;
                }
                else if (control is NumericUpDown numericUpDown)
                {
                    numericUpDown.Value = 0;
                }
                else if (control is CheckBox checkBox)
                {
                    ATDFieldDefinition field = (ATDFieldDefinition)control.Tag;
                    checkBox.Checked = !string.IsNullOrEmpty(field.DefaultValue) && bool.Parse(field.DefaultValue);
                }
            }

            ClearAllErrors();
        }
    }
}
