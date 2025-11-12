# ATD Order System

A C# Windows Forms application that reads an ATD XML definition file and dynamically generates an order input interface with comprehensive field validations.

## Overview

This application demonstrates a data-driven approach to form generation where the form structure, field types, validations, and behavior are all defined in an XML file. The application reads the XML definition and creates a complete order entry interface with proper validation rules.

## Features

- **Dynamic Form Generation**: Reads ATD XML definition and automatically creates form fields
- **Multiple Field Types**: Supports text, email, phone, date, number, decimal, dropdown, textarea, and checkbox fields
- **Comprehensive Validations**:
  - Required field validation
  - Min/max length validation
  - Regular expression pattern validation
  - Numeric range validation
  - Email format validation
  - Phone number format validation
  - Zip code format validation
- **Real-time Error Display**: Shows validation errors inline with each field
- **Order Object Creation**: Creates a strongly-typed C# Order object when form is submitted
- **User-Friendly Interface**: Clean, scrollable form with clear labels and error messages

## Project Structure

```
atd-order-system/
├── ATDOrderSystem.csproj          # Project file
├── OrderDefinition.xml            # ATD XML definition file
├── Program.cs                     # Application entry point
├── Order.cs                       # Order data model class
├── ATDFieldDefinition.cs          # Field definition model
├── ATDOrderDefinitionParser.cs    # XML parser
├── OrderForm.cs                   # Main Windows Forms UI
└── README.md                      # This file
```

## ATD XML Definition

The `OrderDefinition.xml` file defines the structure of the order form. It includes:

### Metadata Section
- Title: Form title
- Version: Definition version
- Description: Form description

### Fields Section
Each field can have the following properties:

- **Name**: Internal field name (used in Order class)
- **Label**: Display label for the field
- **Type**: Field type (Text, Email, Phone, Date, Number, Decimal, Dropdown, TextArea, Checkbox)
- **Required**: Whether the field is required (true/false)
- **MinLength/MaxLength**: Length constraints for text fields
- **MinValue/MaxValue**: Range constraints for numeric fields
- **ValidationPattern**: Regular expression for pattern validation
- **ValidationMessage**: Custom error message
- **DefaultValue**: Default value for the field
- **DecimalPlaces**: Number of decimal places for decimal fields
- **Options**: List of options for dropdown fields

### Example Field Definition

```xml
<Field>
  <Name>CustomerEmail</Name>
  <Label>Customer Email</Label>
  <Type>Email</Type>
  <Required>true</Required>
  <ValidationPattern>^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$</ValidationPattern>
  <ValidationMessage>Please enter a valid email address</ValidationMessage>
</Field>
```

## Building and Running

### Prerequisites

- .NET 6.0 SDK or later
- Windows operating system (required for Windows Forms)

### Build Instructions

1. Open a command prompt or terminal
2. Navigate to the project directory:
   ```
   cd atd-order-system
   ```
3. Build the project:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   dotnet run
   ```

Alternatively, you can open the project in Visual Studio and run it from there.

## Usage

1. **Launch the Application**: Run the executable or use `dotnet run`
2. **Fill Out the Form**: Enter information in all required fields (marked with *)
3. **Validation**: The form validates fields in real-time when you click Submit
4. **Submit**: Click the "Submit Order" button to create the order
5. **View Order**: A message box displays the complete order details
6. **Create Another**: Choose to create another order or close the application

## Order Class

The `Order` class represents the order data with the following properties:

- OrderNumber
- CustomerName
- CustomerEmail
- CustomerPhone
- OrderDate
- ShippingAddress
- City
- State
- ZipCode
- ProductName
- Quantity
- UnitPrice
- ShippingMethod
- PaymentMethod
- SpecialInstructions
- GiftWrap
- Newsletter
- TotalPrice (calculated property)

## Validation Rules

The application implements the following validation rules:

### Order Number
- Format: ORD-XXXXXX (e.g., ORD-123456)
- Required field

### Customer Name
- Length: 2-100 characters
- Required field

### Customer Email
- Valid email format
- Required field

### Customer Phone
- Format: (XXX) XXX-XXXX or XXX-XXX-XXXX
- Required field

### Zip Code
- Format: XXXXX or XXXXX-XXXX
- Required field

### Quantity
- Range: 1-9999
- Required field

### Unit Price
- Range: $0.01 - $999,999.99
- 2 decimal places
- Required field

### Shipping Address
- Length: 10-500 characters
- Required field

### State
- Must select from dropdown list
- Required field

### Shipping Method
- Must select from dropdown list
- Required field

### Payment Method
- Must select from dropdown list
- Required field

## Extending the Application

### Adding New Fields

To add new fields to the order form:

1. **Update OrderDefinition.xml**: Add a new `<Field>` element with appropriate properties
2. **Update Order.cs**: Add a new property to the Order class
3. **Update OrderForm.cs**: Add getter method in `CreateOrderFromForm()` to map the field value

### Adding New Field Types

To add new field types:

1. **Update ATDFieldDefinition.cs**: Add any new properties needed for the field type
2. **Update OrderForm.cs**: Add a new case in `CreateFieldControl()` method
3. **Update OrderForm.cs**: Add validation logic in `ValidateField()` method

### Customizing Validations

Validations are defined in the XML file and can be customized without changing code:

- Use `ValidationPattern` for regex-based validation
- Use `MinLength`/`MaxLength` for text length validation
- Use `MinValue`/`MaxValue` for numeric range validation
- Use `Required` to make fields mandatory

## Technical Details

### Architecture

The application follows a clean separation of concerns:

- **Data Layer**: `Order.cs` - Data model
- **Definition Layer**: `ATDFieldDefinition.cs`, `ATDOrderDefinitionParser.cs` - XML parsing
- **Presentation Layer**: `OrderForm.cs` - UI and validation logic
- **Entry Point**: `Program.cs` - Application startup

### Design Patterns

- **Data-Driven Design**: Form structure defined in XML
- **Factory Pattern**: Dynamic control creation based on field type
- **Model-View Pattern**: Separation of data (Order) and presentation (OrderForm)

### Error Handling

- XML parsing errors are caught and displayed to the user
- Validation errors are displayed inline with red text
- Invalid fields are highlighted with a light red background
- All errors must be corrected before order submission

## License

This is a demonstration application created for educational purposes.

## Support

For questions or issues, please refer to the code comments or contact the development team.
