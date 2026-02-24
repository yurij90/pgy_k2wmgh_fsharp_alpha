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

// Function to read CSV file and return a result with a sequence of maps or an error
let readCsv filePath =
    try
        let lines = File.ReadAllLines(filePath)
        if lines.Length = 0 then
            Error "CSV file is empty or header is missing."
        else
            let headers = lines.[0].Split(',') |> List.ofArray
            
            let isData (s: string) =
                let mutable i = 0
                let mutable d = 0m
                let mutable dt = System.DateTime.MinValue
                System.Int32.TryParse(s, &i) || System.Decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, &d) || System.DateTime.TryParse(s, &dt)

            let dataLikeCount = headers |> List.filter isData |> List.length
            
            if dataLikeCount > headers.Length / 2 then
                Error "The first line of the CSV appears to be data, not a header."
            else
                let data =
                    lines
                    |> Seq.skip 1
                    |> Seq.map (fun line ->
                        let values = line.Split(',')
                        headers
                        |> Seq.mapi (fun i header ->
                            if i < values.Length then
                                header, parseValue values.[i]
                            else
                                header, box "") // Handle rows with fewer columns than headers
                        |> Map.ofSeq)
                Ok data
    with
    | ex -> Error $"An error occurred while reading the CSV file: %s{ex.Message}"

// Generic analysis functions
let countRows (rows: seq<CsvRow>) = Seq.length rows

let getNumericColumns (rows: seq<CsvRow>) =
    rows
    |> Seq.head
    |> Map.keys
    |> Seq.filter (fun col ->
        rows
        |> Seq.truncate 5  // Sample first 5 rows
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
        match readCsv filePath with
        | Error msg ->
            printfn "Error: %s" msg
            1
        | Ok rows ->
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
                let headers =
                    rows
                    |> Seq.tryHead
                    |> Option.map (fun head -> Map.keys head |> Seq.toList)
                    |> Option.defaultValue []

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
