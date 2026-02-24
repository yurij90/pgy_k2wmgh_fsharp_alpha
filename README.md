# Universal Functional Data Analysis

## Project Alpha K2WMGH

A functional programming console application written in F# that performs universal CSV data analysis using functional programming principles.

## Features

- **Universal CSV Parser**: Automatically detects and parses any CSV file with headers
- **Type-Safe Data Processing**: Uses F#'s type system for safe data handling
- **Functional Programming**: Demonstrates core FP concepts including:
  - Immutable data structures (records, maps)
  - Higher-order functions (map, filter, groupBy, fold)
  - Function composition with pipeline operator (|>)
  - Pattern matching for type detection
  - Lazy sequences for efficient processing

## Sample Datasets

The project includes several sample CSV files for testing:

### 1. Sales Data (`data.csv`)

- Product sales with categories, quantities, and prices
- Demonstrates business data analysis
- Sample output: Total sales $10,569.65, category breakdowns

### 2. Employee Data (`employee_data.csv`)

- Employee information with salaries, experience, and ratings
- Shows HR/analytics data processing
- Sample output: Average salary $67,500, department ratings

### 3. Student Performance (`student_data.csv`)

- Academic data with grades, attendance, and study hours
- Educational metrics analysis
- Sample output: Average grade 85.13, attendance 90.88%

### 4. Weather Data (`weather_data.csv`)

- Environmental data with temperature, humidity, wind speed
- Temporal and geographic data handling
- Sample output: Average temperature 45.11°F, city-based humidity analysis

## Usage

### Basic Usage

```bash
dotnet run
```

This will analyze the default `data.csv` file.

### Analyze Specific File

```bash
dotnet run your_file.csv
```

## Analysis Capabilities

The application automatically performs:

- **Row Count**: Total number of data records
- **Numeric Column Detection**: Identifies columns with numeric data
- **Statistical Analysis**: For each numeric column:
  - Count of values
  - Sum and average
  - Minimum and maximum values
- **Grouped Analysis**: Groups data by the first column and analyzes numeric columns

## Functional Programming Concepts Demonstrated

### 1. Immutable Data Structures

```fsharp
type CsvRow = Map<string, obj>
```

Uses immutable maps to represent CSV rows.

### 2. Pattern Matching

```fsharp
let parseValue (value: string) =
    match Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture) with
    | true, i -> box i
    | _ -> // Try other types...
```

Type-safe value parsing with pattern matching.

### 3. Higher-Order Functions

```fsharp
let columnStats (rows: seq<CsvRow>) column =
    rows
    |> Seq.choose (fun row -> /* extract numeric values */)
    |> Seq.toList
    |> List.sum  // Sum using fold
```

Function composition for data transformation.

### 4. Pipeline Operator

```fsharp
rows
|> Seq.groupBy (fun row -> /* grouping logic */)
|> Seq.map (fun (key, groupRows) -> /* analysis */)
```

Readable data processing pipelines.

## Installation

1. Ensure you have .NET SDK installed
2. Clone or download this repository
3. Navigate to the project directory
4. Run `dotnet restore` to restore packages
5. Run `dotnet run` to execute

## Creating Your Own CSV Files

The application works with any CSV file that:

- Has a header row with column names
- Contains numeric data in at least one column
- Uses comma separation

Example format:

```csv
Column1,Column2,Column3,Column4
value1,value2,123,45.6
value1,value2,456,78.9
```

## Technical Implementation

### Core Functions

- `readCsv`: Reads and parses CSV files into typed data structures
- `parseValue`: Type-safe value parsing with automatic type detection
- `columnStats`: Statistical analysis of numeric columns
- `groupByColumn`: Grouped analysis functionality
- `getNumericColumns`: Automatic detection of numeric data

### Error Handling

The application includes robust error handling for:

- Missing files
- Invalid data formats
- Type conversion errors
- Empty datasets

## Contributing

This project demonstrates functional programming best practices in F#. Feel free to:

- Add new sample datasets
- Enhance analysis capabilities
- Improve error handling
- Add new functional programming examples

## License

This project is open source and available under the MIT License.
