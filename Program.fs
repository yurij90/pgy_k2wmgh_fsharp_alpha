// For more information see https://aka.ms/fsharp-console-apps

open System
open System.IO
open System.Globalization

// Generic CSV row as map of column name to value
type CsvRow = Map<string, obj>

// Function to parse a value, trying different types
let parseValue (value: string) =
    match Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture) with
    | true, i -> box i
    | _ ->
        match Decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture) with
        | true, d -> box d
        | _ ->
            match DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None) with
            | true, dt -> box dt
            | _ -> box value

// Function to read CSV file and return sequence of maps
let readCsv filePath =
    let lines = File.ReadAllLines(filePath)
    let headers = lines.[0].Split(',') |> List.ofArray
    lines
    |> Seq.skip 1
    |> Seq.map (fun line ->
        let values = line.Split(',')
        headers
        |> Seq.mapi (fun i header -> header, parseValue values.[i])
        |> Map.ofSeq)

// Generic analysis functions
let countRows (rows: seq<CsvRow>) = Seq.length rows

let getNumericColumns (rows: seq<CsvRow>) =
    rows
    |> Seq.head
    |> Map.keys
    |> Seq.filter (fun col ->
        rows
        |> Seq.take 5  // Sample first 5 rows
        |> Seq.forall (fun row ->
            match Map.tryFind col row with
            | Some (:? int) | Some (:? decimal) -> true
            | _ -> false))

let columnStats (rows: seq<CsvRow>) column =
    let values =
        rows
        |> Seq.choose (fun row ->
            match Map.tryFind column row with
            | Some (:? int as i) -> Some (decimal i)
            | Some (:? decimal as d) -> Some d
            | _ -> None)
        |> Seq.toList

    if values.IsEmpty then
        None
    else
        let sum = List.sum values
        let avg = sum / decimal values.Length
        let min = List.min values
        let max = List.max values
        Some (sum, avg, min, max, values.Length)

let groupByColumn (rows: seq<CsvRow>) groupColumn valueColumn =
    rows
    |> Seq.groupBy (fun row ->
        match Map.tryFind groupColumn row with
        | Some v -> string v
        | None -> "Unknown")
    |> Seq.map (fun (key, groupRows) ->
        let stats = columnStats groupRows valueColumn
        match stats with
        | Some (sum, _, _, _, count) -> key, sum, count
        | None -> key, 0M, 0)

// Main application
[<EntryPoint>]
let main argv =
    let filePath = if argv.Length > 0 then argv.[0] else "data.csv"

    if not (File.Exists(filePath)) then
        printfn "Error: File '%s' not found." filePath
        1
    else
        let rows = readCsv filePath
        let rowCount = countRows rows
        let numericCols = getNumericColumns rows |> Seq.toList

        printfn "Universal Functional Data Analysis"
        printfn "=================================="
        printfn "File: %s" filePath
        printfn "Rows: %d" rowCount
        printfn ""

        if numericCols.IsEmpty then
            printfn "No numeric columns found for analysis."
        else
            printfn "Numeric Columns Analysis:"
            for col in numericCols do
                match columnStats rows col with
                | Some (sum, avg, min, max, count) ->
                    printfn "  %s:" col
                    printfn "    Count: %d" count
                    printfn "    Sum: %.2f" sum
                    printfn "    Average: %.2f" avg
                    printfn "    Min: %.2f" min
                    printfn "    Max: %.2f" max
                | None -> ()
            printfn ""

            // If we have at least 2 columns, try grouping
            let headers = rows |> Seq.head |> Map.keys |> Seq.toList
            if headers.Length >= 2 then
                let groupCol = headers.[0]  // First column as group
                let valueCol = numericCols |> List.tryHead  // First numeric column
                match valueCol with
                | Some vc ->
                    printfn "Grouped Analysis (%s by %s):" vc groupCol
                    groupByColumn rows groupCol vc
                    |> Seq.iter (fun (key, sum, count) ->
                        printfn "  %s: %.2f (count: %d)" key sum count)
                | None -> ()

        0 // return an integer exit code
