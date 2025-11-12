# Quick Start Guide

## Running the Application

### Option 1: Using .NET CLI
```bash
cd atd-order-system
dotnet build
dotnet run
```

### Option 2: Using Visual Studio
1. Open `ATDOrderSystem.csproj` in Visual Studio
2. Press F5 or click "Start" to run

## Testing the Application

### Sample Valid Data

Use these values to test the application:

- **Order Number**: ORD-123456
- **Customer Name**: John Smith
- **Customer Email**: john.smith@example.com
- **Customer Phone**: 555-123-4567
- **Order Date**: (select today's date)
- **Shipping Address**: 123 Main Street, Apartment 4B
- **City**: New York
- **State**: NY
- **Zip Code**: 10001
- **Product Name**: Wireless Headphones
- **Quantity**: 2
- **Unit Price**: 79.99
- **Shipping Method**: Express (2-3 days)
- **Payment Method**: Credit Card
- **Special Instructions**: Please call before delivery
- **Gift Wrap**: (check if desired)
- **Newsletter**: (check if desired)

## Expected Behavior

1. **Form Loads**: All fields appear based on OrderDefinition.xml
2. **Required Fields**: Marked with asterisk (*)
3. **Validation**: Occurs when Submit button is clicked
4. **Errors**: Displayed in red text below invalid fields
5. **Success**: Shows order summary in message box
6. **New Order**: Option to create another order or exit

## Common Validation Errors

- **Order Number**: Must match format ORD-XXXXXX
- **Email**: Must be valid email format
- **Phone**: Must be valid US phone format
- **Zip Code**: Must be 5 digits or 5+4 format
- **Quantity**: Must be between 1 and 9999
- **Unit Price**: Must be between $0.01 and $999,999.99

## Customizing the Form

Edit `OrderDefinition.xml` to:
- Add new fields
- Change validation rules
- Modify field types
- Update dropdown options
- Change default values

The form will automatically rebuild based on the XML definition.
