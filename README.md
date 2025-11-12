# FIXatdl Order Entry System

A comprehensive C# WPF application for capital markets order entry that dynamically generates trading order interfaces from FIXatdl (FIX Algorithmic Trading Definition Language) 1.2 XML definitions.

## Overview

This application implements a complete FIXatdl 1.2 compliant order entry system for equities trading. It reads FIXatdl XML strategy definitions and dynamically generates a professional order entry interface with full support for:

- Standard FIX order fields (Symbol, Side, OrderQty, OrdType, Price, TimeInForce, etc.)
- Algorithmic strategy parameters with custom controls
- Complex validation rules and cross-field dependencies
- State rules for dynamic enable/disable and visibility control
- Enum mapping between display values and wire values
- NewOrderSingle (FIX 35=D) message generation

## What is FIXatdl?

FIXatdl (FIX Algorithmic Trading Definition Language) is an XML-based standard that allows brokers and trading venues to publish the parameters, layouts, and validation rules for their algorithmic trading strategies. Instead of hardcoding order entry screens for each broker, buy-side firms can load the broker's FIXatdl file and dynamically render the appropriate order ticket.

## Features

### Dynamic UI Generation
- Reads FIXatdl 1.2 XML definitions
- Dynamically creates WPF controls based on control types (TextField, DropDownList, SingleSpinner, CheckBox, Clock)
- Organizes controls into strategy panels with configurable orientation
- Supports multiple field types: String, Char, Int, Float, Qty, Price, Boolean, UTCTimestamp

### Comprehensive Validation
- Required field validation
- Min/max value range validation
- Increment validation for numeric fields
- Complex cross-field validation rules with logical operators (AND, OR, XOR, NOT)
- Real-time validation feedback with inline error messages

### State Rules Engine
- Dynamic enable/disable of controls based on other field values
- Dynamic visibility control
- Automatic re-evaluation on field changes
- Support for complex conditional logic

### Enum Mapping
- Maps display-friendly text to wire values for FIX transmission
- Supports dropdown lists with enum pairs
- Handles boolean wire values (Y/N, true/false, etc.)

### FIX Order Generation
- Creates NewOrderSingle DTO with standard FIX fields
- Includes strategy parameters with proper wire value mapping
- Generates FIX tag/value pairs ready for transmission
- Displays formatted order summary

## Project Structure

```
FIXatdlOrderEntry/
├── Models/
│   ├── FIXatdlParameter.cs       # Parameter definitions and enums
│   ├── FIXatdlControl.cs         # UI control definitions
│   ├── FIXatdlLayout.cs          # Layout and panel definitions
│   ├── FIXatdlRules.cs           # Validation and state rules
│   ├── FIXatdlStrategy.cs        # Main strategy model
│   └── NewOrderSingle.cs         # FIX order DTO
├── Parsers/
│   └── FIXatdlParser.cs          # XML parser for FIXatdl 1.2
├── Engine/
│   ├── ValidationEngine.cs       # Validation rules engine
│   └── StateRulesEngine.cs       # State rules engine
├── UI/
│   ├── OrderEntryWindow.xaml     # Main WPF window
│   └── OrderEntryWindow.xaml.cs  # UI logic and dynamic form generation
├── App.xaml                      # WPF application definition
├── App.xaml.cs                   # Application entry point
├── SampleStrategy.xml            # Sample VWAP strategy definition
└── ATDOrderSystem.csproj         # Project file
```

## FIXatdl XML Structure

### Parameters
Define the data fields for the order:

```xml
<Parameter name="Symbol" xsi:type="String_t" fixTag="55" use="required">
    <Description>Trading symbol</Description>
</Parameter>

<Parameter name="Side" xsi:type="Char_t" fixTag="54" use="required">
    <Description>Order side</Description>
    <EnumPair enumID="e_Buy" wireValue="1">Buy</EnumPair>
    <EnumPair enumID="e_Sell" wireValue="2">Sell</EnumPair>
</Parameter>
```

### Layout
Define the UI structure:

```xml
<lay:StrategyLayout>
    <lay:StrategyPanel orientation="VERTICAL" title="Order Details">
        <lay:Control ID="c_Symbol" xsi:type="lay:TextField_t" 
                     label="Symbol:" parameterRef="Symbol"/>
        <lay:Control ID="c_Side" xsi:type="lay:DropDownList_t" 
                     label="Side:" parameterRef="Side">
            <lay:ListItem enumID="e_Buy" uiRep="Buy"/>
            <lay:ListItem enumID="e_Sell" uiRep="Sell"/>
        </lay:Control>
    </lay:StrategyPanel>
</lay:StrategyLayout>
```

### Validation Rules
Define cross-field validation:

```xml
<val:StrategyEdit errorMessage="Price is required for Limit orders">
    <val:Edit logicOperator="OR">
        <val:Edit field="OrdType" operator="NE" value="e_Limit"/>
        <val:Edit field="Price" operator="EX"/>
    </val:Edit>
</val:StrategyEdit>
```

### State Rules
Define dynamic control behavior:

```xml
<flow:StateRule enabled="false">
    <val:Edit field="OrdType" operator="NE" value="e_Limit"/>
    <flow:AffectedControl id="c_Price"/>
</flow:StateRule>
```

## Building and Running

### Prerequisites
- .NET 6.0 SDK or later
- Windows operating system (required for WPF)

### Build Instructions

```bash
cd ATDOrderEntry
dotnet build
dotnet run
```

Or open in Visual Studio and press F5.

## Usage

1. **Launch Application**: The application loads `SampleStrategy.xml` on startup
2. **Fill Order Details**: Enter required fields (Symbol, Side, Quantity, Order Type, Time in Force)
3. **Configure Strategy**: Set VWAP strategy parameters (Start/End Time, Participation Rate, etc.)
4. **Dynamic Validation**: Controls enable/disable based on order type (e.g., Price field only for Limit orders)
5. **Submit Order**: Click "Submit Order" to validate and create the NewOrderSingle DTO
6. **View Results**: See the formatted FIX message with all fields and strategy parameters

## Sample Strategy

The included `SampleStrategy.xml` defines a VWAP (Volume Weighted Average Price) strategy with:

### Standard Order Fields
- Symbol (55)
- Side (54): Buy, Sell, Sell Short
- OrderQty (38)
- OrdType (40): Market, Limit, Stop, Stop Limit
- Price (44)
- StopPx (99)
- TimeInForce (59): Day, GTC, IOC, FOK
- Account (1)

### VWAP Strategy Parameters
- StartTime (7602): Strategy start time
- EndTime (7603): Strategy end time
- ParticipationRate (7604): Target participation rate (1-50%)
- MaxPctVolume (7605): Maximum percentage of volume (1-100%)
- DisplayQty (7606): Display quantity for iceberg orders
- WouldCross (7607): Allow crossing the spread
- Aggression (7608): Low, Medium, High
- DarkPoolPreference (7609): None, Prefer, Only

### Validation Rules
- Price required for Limit and Stop Limit orders
- Stop Price required for Stop and Stop Limit orders
- Numeric range validation for participation rates

### State Rules
- Price field disabled unless Order Type is Limit or Stop Limit
- Stop Price field disabled unless Order Type is Stop or Stop Limit

## Supported Control Types

- **TextField_t**: Single-line text input
- **DropDownList_t**: Dropdown selection with enum mapping
- **SingleSpinner_t**: Numeric input with increment/decrement buttons
- **CheckBox_t**: Boolean checkbox
- **Clock_t**: Date/time picker

## Supported Parameter Types

- **String_t**: Text strings
- **Char_t**: Single character (often used with enums)
- **Int_t**: Integer numbers
- **Float_t**: Floating point numbers
- **Qty_t**: Quantity (decimal)
- **Price_t**: Price (decimal)
- **Amt_t**: Amount (decimal)
- **Boolean_t**: True/false with custom wire values
- **UTCTimestamp_t**: UTC timestamp
- **LocalMktDate_t**: Local market date

## Validation Operators

- **EQ**: Equal to
- **NE**: Not equal to
- **LT**: Less than
- **LE**: Less than or equal to
- **GT**: Greater than
- **GE**: Greater than or equal to
- **EX**: Exists (not null/empty)
- **NX**: Not exists (null/empty)

## Logic Operators

- **AND**: All conditions must be true
- **OR**: At least one condition must be true
- **XOR**: Exactly one condition must be true
- **NOT**: Condition must be false

## NewOrderSingle Output

The application creates a `NewOrderSingle` DTO containing:

### Standard FIX Fields
- ClOrdID (11): Auto-generated unique order ID
- Symbol (55)
- Side (54)
- OrderQty (38)
- OrdType (40)
- Price (44) - if applicable
- StopPx (99) - if applicable
- TimeInForce (59)
- Account (1) - if provided
- HandlInst (21): Set to "1" (automated)
- TransactTime (60): UTC timestamp

### Strategy Parameters
List of parameter name/value pairs with:
- Parameter name
- FIX tag (if defined)
- Display value
- Wire value (for FIX transmission)

## Extending the Application

### Adding New Strategies

1. Create a new FIXatdl 1.2 XML file following the schema
2. Define parameters with appropriate types and enums
3. Design the layout with controls and panels
4. Add validation rules for cross-field dependencies
5. Add state rules for dynamic behavior
6. Update the application to load your XML file

### Integrating with FIX Engine

To send actual FIX orders:

1. Add QuickFIX/n NuGet package
2. Use `NewOrderSingle.ToFIXTagValuePairs()` to get FIX fields
3. Create QuickFIX Message and populate fields
4. Send via FIX session

### Customizing UI

The WPF UI can be customized by:
- Modifying styles in `OrderEntryWindow.xaml`
- Adjusting control creation logic in `OrderEntryWindow.xaml.cs`
- Adding custom control types in the control factory

## FIXatdl 1.2 Compliance

This implementation supports core FIXatdl 1.2 features:
- ✅ Parameters with all standard types
- ✅ EnumPairs for display/wire value mapping
- ✅ StrategyLayout with panels and controls
- ✅ Validation rules with logical operators
- ✅ State rules for enable/disable and visibility
- ✅ Multiple control types
- ✅ Required/optional field handling
- ✅ Min/max/increment constraints

## Technical Details

### Architecture
- **Model Layer**: FIXatdl data structures
- **Parser Layer**: XML deserialization
- **Engine Layer**: Validation and state rule evaluation
- **UI Layer**: Dynamic WPF form generation

### Design Patterns
- **Factory Pattern**: Dynamic control creation
- **Strategy Pattern**: Pluggable validation rules
- **Observer Pattern**: Field change notifications
- **DTO Pattern**: NewOrderSingle data transfer

## License

This is a demonstration application for FIXatdl 1.2 order entry systems.

## References

- [FIXatdl Specification](http://www.fixprotocol.org/FIXatdl)
- [FIX Protocol](https://www.fixtrading.org/)
- [FIX 4.4 Specification](https://www.fixtrading.org/standards/fix-4-4/)
