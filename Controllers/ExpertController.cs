using Backend.Data;
using Backend.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpertController : Controller
    {
        private readonly DataContext _context;

        public ExpertController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetProducts(int start, int end)
        {
            var products = await _context.Product.Skip(start).Take(end - start).ToArrayAsync();
            return Ok(products);
        }
        
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            return Ok(product);
        }
        
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] ProductRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid product request.");
            }

            var product = new Product
            {
                Brands = request.Brands,
                Models = request.Models,
                Colors = request.Colors,
                Memory = request.Memory,
                Storage = request.Storage,
                Camera = request.Camera,
                OriginalPrice = request.OriginalPrice,
                Mobile = request.Mobile,
                DiscountPercentage = request.DiscountPercentage,
                OS = request.OS,
                SellersAmount = request.SellersAmount,
                ScreenSize = request.ScreenSize,
                BatterySize = request.BatterySize
            };

            product.SellingPrice = product.OriginalPrice * (100 - product.DiscountPercentage) / 100;
            product.Discount = product.OriginalPrice - product.SellingPrice;

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid product request.");
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            product.Brands = request.Brands;
            product.Models = request.Models;
            product.Colors = request.Colors;
            product.Memory = request.Memory;
            product.Storage = request.Storage;
            product.Camera = request.Camera;
            product.OriginalPrice = request.OriginalPrice;
            product.Mobile = request.Mobile;
            product.DiscountPercentage = request.DiscountPercentage;
            product.OS = request.OS;
            product.SellersAmount = request.SellersAmount;
            product.ScreenSize = request.ScreenSize;
            product.BatterySize = request.BatterySize;

            product.SellingPrice = product.OriginalPrice * (100 - product.DiscountPercentage) / 100;
            product.Discount = product.OriginalPrice - product.SellingPrice;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("UpdateRating/{id}")]
        public async Task<IActionResult> UpdateRating(int id, [FromBody] decimal rating)
        {
            if (rating < 0 || rating > 5)
            {
                return BadRequest("Invalid rating request.");
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            product.Rating = (product.Rating + rating) / 2;
            product.Reviews += 1;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpPut("SetRating/{id}")]
        public async Task<IActionResult> SetRating(int id, [FromBody] RatingRequest request)
        {
            if (request == null || request.Rating < 0 || request.Rating > 5)
            {
                return BadRequest("Invalid rating request.");
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            product.Rating = request.Rating;
            product.Reviews = request.Reviews;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully.");
        }
        
        [HttpPost("Import")]
        public async Task<IActionResult> Import(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return BadRequest("File not found.");
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
            var products = new List<Product>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                await csv.ReadAsync();
                csv.ReadHeader();

                var i = 0;
                while (await csv.ReadAsync())
                {
                    try
                    {
                        var product = new Product
                        {
                            Brands = csv.GetField("Brands"),
                            Models = csv.GetField("Models"),
                            Colors = csv.GetField("Colors"),
                            Memory = SafeIntParse(csv.GetField("Memory").Replace("GB", string.Empty, StringComparison.OrdinalIgnoreCase).Trim()),
                            Storage = SafeIntParse(csv.GetField("Storage").Replace("GB", string.Empty, StringComparison.OrdinalIgnoreCase).Trim()),
                            Camera = !string.IsNullOrWhiteSpace(csv.GetField("Camera")) && csv.GetField("Camera").Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            Rating = SafeDecimalParse(csv.GetField("Rating")),
                            SellingPrice = SafeDecimalParse(csv.GetField("SellingPrice")),
                            OriginalPrice = SafeDecimalParse(csv.GetField("OriginalPrice")),
                            Mobile = csv.GetField("Mobile"),
                            Discount = SafeDecimalParse(csv.GetField("Discount")),
                            DiscountPercentage = SafeDecimalParse(csv.GetField("DiscountPercentage")),
                            OS = csv.GetField("OS"),
                            SellersAmount = SafeIntParse(csv.GetField("SellersAmount")),
                            ScreenSize = SafeDecimalParse(csv.GetField("ScreenSize")),
                            BatterySize = SafeIntParse(csv.GetField("BatterySize")),
                            Reviews = SafeIntParse(csv.GetField("Reviews"))
                        };

                        products.Add(product);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing record \"{i}\": {ex.Message}");
                    }
                    finally
                    {
                        i++;
                    }
                }
            }

            _context.Product.AddRange(products);
            await _context.SaveChangesAsync();

            return Ok(new { ImportedCount = products.Count });
        }

        private int SafeIntParse(string strValue)
        {
            var intValue = 0;
            int.TryParse(strValue, out intValue);
            return intValue;
        }

        private decimal SafeDecimalParse(string strValue)
        {
            var decimalValue = 0m;
            decimal.TryParse(strValue, out decimalValue);
            return decimalValue;
        }

        [HttpPost("Build")]
        public async Task<IActionResult> BuildExpertSystem(bool rebuildDataset = true, bool rebuildCluster = true, bool rebuildProlog = true)
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.DATASET_FILE_NAME);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };
                var productIds = new List<string>();
                if (!System.IO.File.Exists(filePath) || rebuildDataset)
                {
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                    int offset = 0;
                    bool headerWritten = false;

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var streamWriter = new StreamWriter(fileStream))
                    using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                    {
                        while (true)
                        {
                            var batch = await _context.Product
                                .OrderBy(p => p.ProductID)
                                .Skip(offset)
                                .Take(Constants.BATCH_SIZE)
                                .ToListAsync();

                            if (batch == null || batch.Count == 0) break;

                            if (!headerWritten)
                            {
                                csvWriter.WriteHeader<Product>();
                                csvWriter.NextRecord();
                                headerWritten = true;
                            }

                            foreach (var product in batch)
                            {
                                productIds.Add(product.ProductID.ToString());
                                csvWriter.WriteRecord(product);
                                csvWriter.NextRecord();
                            }

                            offset += batch.Count;
                        }

                        await streamWriter.FlushAsync();
                    }
                }
                else
                {
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, config))
                    {
                        while (await csv.ReadAsync())
                        {
                            string productId = csv.GetField("ProductID") ?? string.Empty;
                            productIds.Add(productId);
                        }
                    }
                }

                filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME);

                var clusterAssignments = Array.Empty<int>();
                if (!System.IO.File.Exists(filePath) || rebuildCluster)
                {
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                    filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.DATASET_FILE_NAME);
                    var clusteringFields = new string[]
                    {
                        "Memory",
                        "Storage",
                        "Rating",
                        "OriginalPrice",
                        "DiscountPercentage",
                        "SellersAmount",
                        "ScreenSize",
                        "BatterySize",
                        "Reviews"
                    };
                    var input = new
                    {
                        FilePath = filePath,
                        Fields = clusteringFields,
                        K = -1
                    };

                    var jsonData = JsonSerializer.Serialize(input);
                    var psi = new ProcessStartInfo
                    {
                        FileName = string.Format("\"{0}\"", Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PYTHON_VENV)),
                        Arguments = string.Format("\"{0}\"", Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PYTHON_KMEANS_SCRIPT_FILE_PATH)),
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = psi })
                    {
                        process.Start();
                        using (var sw = process.StandardInput)
                        {
                            if (sw.BaseStream.CanWrite)
                            {
                                await sw.WriteLineAsync(jsonData);
                            }
                        }

                        var output = await process.StandardOutput.ReadToEndAsync();
                        var error = await process.StandardError.ReadToEndAsync();

                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            throw new Exception("Errors: " + error);
                        }

                        clusterAssignments = JsonSerializer.Deserialize<int[]>(output) ?? Array.Empty<int>();
                    }
                }

                if (clusterAssignments == null || clusterAssignments.Length == 0 || productIds.Count != clusterAssignments.Length) return BadRequest();

                filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var streamWriter = new StreamWriter(fileStream))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteField("ProductID");
                    csvWriter.WriteField("ClusterAssignment");
                    csvWriter.NextRecord();

                    for (int i = 0; i < clusterAssignments.Length; i++)
                    {
                        csvWriter.WriteField(productIds[i]);
                        csvWriter.WriteField(clusterAssignments[i]);
                        csvWriter.NextRecord();
                    }
                    await streamWriter.FlushAsync();
                }

                filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.DATASET_FILE_NAME);

                using var reader0 = new StreamReader(filePath);
                using var csv0 = new CsvReader(reader0, config);
                using var dr = new CsvDataReader(csv0);

                var dataTable = new DataTable();
                dataTable.Load(dr);

                if (dataTable.Rows.Count != clusterAssignments.Length)
                    throw new ArgumentException("Cluster assignment length does not match the number of rows in CSV");

                // Build a dictionary of column name to data type.
                var columnTypes = dataTable.Columns.Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => col.DataType);

                columnTypes.Remove("ProductID");
                columnTypes.Remove("Models");
                columnTypes.Remove("Camera");
                columnTypes.Remove("OriginalPrice");
                columnTypes.Remove("Mobile");
                columnTypes.Remove("Discount");

                // Group row indices based on the cluster assignment.
                var grouped = Enumerable.Range(0, dataTable.Rows.Count)
                    .GroupBy(i => clusterAssignments[i])
                    .ToDictionary(g => g.Key, g => g.ToList());

                var summaries = new List<ClusterSummary>();
                foreach (var (clusterId, indices) in grouped)
                {
                    var summary = new ClusterSummary { ClusterId = clusterId };

                    // Prepare to collect values for each column.
                    var featureValues = new Dictionary<string, List<object>>();
                    foreach (var col in columnTypes.Keys)
                        featureValues[col] = new List<object>();

                    // Populate the featureValues dictionary
                    foreach (var i in indices)
                    {
                        var row = dataTable.Rows[i];
                        foreach (var col in columnTypes.Keys)
                            featureValues[col].Add(row[col]);
                    }

                    // Process each column:
                    foreach (var col in columnTypes.Keys)
                    {
                        var values = featureValues[col];

                        // For int or long, compute the average and round.
                        if (columnTypes[col] == typeof(int) || columnTypes[col] == typeof(long))
                        {
                            var avg = values.Select(v => Convert.ToInt64(v)).Average();
                            summary.Features[col] = (int)Math.Round(avg);
                        }
                        // For floating point numbers, calculate the average.
                        else if (columnTypes[col] == typeof(float) || columnTypes[col] == typeof(double) || columnTypes[col] == typeof(decimal))
                        {
                            var avg = values.Select(v => Convert.ToDouble(v)).Average();
                            summary.Features[col] = avg;
                        }
                        // For strings or other types, use the mode.
                        else
                        {
                            var mode = values.GroupBy(v => v.ToString().Trim()).OrderByDescending(g => g.Count()).FirstOrDefault();
                            summary.Features[col] = mode == null ? string.Empty : mode.Key;
                        }
                    }
                    summaries.Add(summary);
                }

                if (summaries.Count == 0) return BadRequest();

                filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PROLOG_FILE_NAME);

                if (!System.IO.File.Exists(filePath) || rebuildProlog)
                {
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);

                    using (var writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("% Expert System rules generated from cluster summaries");
                        writer.WriteLine();

                        foreach (var summary in summaries)
                        {
                            var featuresTerms = summary.Features.Select(kvp =>
                            {
                                var key = "'unknown'";
                                if (!string.IsNullOrEmpty(kvp.Key))
                                {
                                    var lower = kvp.Key.ToLowerInvariant();
                                    key = Regex.IsMatch(lower, "^[a-z][a-z0-9_]*$") ? lower : $"'{lower}'";
                                }

                                var value = string.Empty;
                                switch (kvp.Value)
                                {
                                    case string s:
                                        var escaped = s.Replace("'", "''");
                                        value = $"'{escaped}'";
                                        break;
                                    case bool b:
                                        value = b ? "true" : "false";
                                        break;
                                    default:
                                        value = Convert.ToString(kvp.Value, System.Globalization.CultureInfo.InvariantCulture) ?? "0";
                                        break;
                                }

                                return $"feature({key}, {value})";
                            });

                            var featuresList = "[" + string.Join(", ", featuresTerms) + "]";
                            writer.WriteLine($"cluster({summary.ClusterId}, {featuresList}).");
                        }

                        // Now write the backward-chaining rules using the new version.
                        writer.WriteLine();
                        writer.WriteLine("% --------------------------------------------------------------------");
                        writer.WriteLine("% Backward Chaining Expert System Rules (New Version)");
                        writer.WriteLine("% --------------------------------------------------------------------");
                        writer.WriteLine();
                        // --- Helper Predicates to Extract Feature Values ---
                        writer.WriteLine("get_numeric_feature(Cluster, Field, NumVal) :-");
                        writer.WriteLine("    cluster(Cluster, Features),");
                        writer.WriteLine("    member(feature(Field, Val), Features),");
                        writer.WriteLine("    atom_number(Val, NumVal).");
                        writer.WriteLine();
                        writer.WriteLine("get_string_feature(Cluster, Field, StrVal) :-");
                        writer.WriteLine("    cluster(Cluster, Features),");
                        writer.WriteLine("    member(feature(Field, Val), Features),");
                        writer.WriteLine("    atom_string(Val, StrVal).");
                        writer.WriteLine();
                        // --- Compute Closest Clusters for Numeric Requirements ---
                        writer.WriteLine("compute_closest_clusters(Requirements, FieldToClusterMap) :-");
                        writer.WriteLine("    include([R]>>(R = requirement(_,_,numeric)), Requirements, NumericReqs),");
                        writer.WriteLine("    maplist(get_closest_cluster_for_field, NumericReqs, Pairs),");
                        writer.WriteLine("    dict_create(FieldToClusterMap, _, Pairs).");
                        writer.WriteLine();
                        writer.WriteLine("get_closest_cluster_for_field(requirement(Field, Desired, numeric), Field-BestCluster) :-");
                        writer.WriteLine("    findall(Diff-C, (");
                        writer.WriteLine("         cluster(C, _),");
                        writer.WriteLine("         get_numeric_feature(C, Field, NumVal),");
                        writer.WriteLine("         Diff is abs(Desired - NumVal)");
                        writer.WriteLine("    ), List),");
                        writer.WriteLine("    sort(List, [_-BestCluster | _]).");
                        writer.WriteLine();
                        // --- Requirement Matching Predicates ---
                        writer.WriteLine("% A requirement is represented as requirement(Field, Desired, Type),");
                        writer.WriteLine("% where Type is either 'text' or 'numeric'.");
                        writer.WriteLine();
                        writer.WriteLine("% For text requirements: Exact (case-insensitive) match.");
                        writer.WriteLine("triggered(Cluster, requirement(Field, Desired, text), _, true) :-");
                        writer.WriteLine("    get_string_feature(Cluster, Field, Val),");
                        writer.WriteLine("    downcase_atom(Val, LVal),");
                        writer.WriteLine("    downcase_atom(Desired, LDesired),");
                        writer.WriteLine("    LVal = LDesired.");
                        writer.WriteLine();
                        writer.WriteLine("% For numeric requirements: Use the precomputed field-to-cluster map.");
                        writer.WriteLine("triggered(Cluster, requirement(Field, _, numeric), FieldToClusterMap, true) :-");
                        writer.WriteLine("    get_dict(Field, FieldToClusterMap, Cluster), !.");
                        writer.WriteLine();
                        writer.WriteLine("triggered(_, _, _, false).");
                        writer.WriteLine();
                        // --- Score Calculation ---
                        writer.WriteLine("% score_cluster(+Cluster, +Requirements, +FieldToClusterMap, -Score)");
                        writer.WriteLine("% Score is the count of requirements triggered by the cluster.");
                        writer.WriteLine("score_cluster(Cluster, Requirements, FieldToClusterMap, Score) :-");
                        writer.WriteLine("    maplist(triggered(Cluster), Requirements, FieldToClusterMap, TriggeredList),");
                        writer.WriteLine("    include(==(true), TriggeredList, Filtered),");
                        writer.WriteLine("    length(Filtered, Score).");
                        writer.WriteLine();
                        writer.WriteLine("% best_cluster(+Requirements, -BestCluster)");
                        writer.WriteLine("best_cluster(Requirements, BestCluster) :-");
                        writer.WriteLine("    setof(C, Fs^(cluster(C, Fs)), Clusters),");
                        writer.WriteLine("    compute_closest_clusters(Requirements, FieldMap),");
                        writer.WriteLine("    findall(Score-C, (member(C, Clusters), score_cluster(C, Requirements, FieldMap, Score)), ScorePairs),");
                        writer.WriteLine("    sort(1, @>=, ScorePairs, [_-BestCluster | _]).");
                        writer.WriteLine();
                        // --- Requirement Parsing ---
                        writer.WriteLine("% parse_requirements(+InputString, -Requirements)");
                        writer.WriteLine("% Input is a string with requirements separated by \";\".");
                        writer.WriteLine("% Each requirement is formatted as: \"Field,Desired\".");
                        writer.WriteLine("parse_requirements(InputString, Requirements) :-");
                        writer.WriteLine("    split_string(InputString, \";\", \" \", ReqStrings),");
                        writer.WriteLine("    maplist(parse_requirement, ReqStrings, Requirements).");
                        writer.WriteLine();
                        writer.WriteLine("parse_requirement(ReqStr, requirement(Field, Desired, Type)) :-");
                        writer.WriteLine("    split_string(ReqStr, \",\", \" \", Parts),");
                        writer.WriteLine("    ( Parts = [FieldString, DesiredString] ->");
                        writer.WriteLine("          ( number_string(Num, DesiredString) ->");
                        writer.WriteLine("                Type = numeric, Desired = Num, Field = FieldString");
                        writer.WriteLine("          ;");
                        writer.WriteLine("                Type = text, Desired = DesiredString, Field = FieldString");
                        writer.WriteLine("          )");
                        writer.WriteLine("    ;");
                        writer.WriteLine("       Field = \"\", Desired = \"\", Type = text");
                        writer.WriteLine("    ).");
                        writer.WriteLine();
                        // --- Main Predicate ---
                        writer.WriteLine("% Main predicate for backward chaining testing.");
                        writer.WriteLine("main :-");
                        writer.WriteLine("    read_line_to_string(user_input, Input),");
                        writer.WriteLine("    parse_requirements(Input, Requirements),");
                        writer.WriteLine("    best_cluster(Requirements, BestCluster),");
                        writer.WriteLine("    format(\"Best matching cluster: ~w~n\", [BestCluster]),");
                        writer.WriteLine("    halt.");
                        writer.WriteLine();
                        writer.WriteLine(":- initialization(main).");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        [HttpPost("GetExpertResults")]
        public async Task<IActionResult> GetExpertResults([FromBody] PrologInput input)
        {
            try
            {
                // 1. Build the Prolog input string.
                //    Format each requirement as "field,value" and separate by semicolons.
                //    The order should match what your .pl file expects.
                var requirements = new List<string>
                {
                    $"brands,{input.Brands}",
                    $"colors,{input.Colors}",
                    $"memory,{input.Memory}",
                    $"storage,{input.Storage}",
                    $"rating,{input.Rating}",
                    $"sellingprice,{input.SellingPrice}",
                    $"discountpercentage,{input.DiscountPercentage}",
                    $"os,{input.OS}",
                    $"sellersamount,{input.SellersAmount}",
                    $"screensize,{input.ScreenSize}",
                    $"batterysize,{input.BatterySize}",
                    $"reviews,{input.Reviews}"
                };
                var prologInput = string.Join(";", requirements);

                // 2. Define the file paths.
                var plFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PROLOG_FILE_NAME);
                var swiFilePath = Constants.SWI_FILE_PATH; // Absolute path to swipl.exe

                // 3. Setup the ProcessStartInfo for SWI-Prolog.
                var psi = new ProcessStartInfo
                {
                    FileName = swiFilePath,
                    Arguments = $"-s \"{plFilePath}\"", // load the .pl file
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var prologOutput = string.Empty;
                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Write the input string to stdIn
                    if (process.StandardInput.BaseStream.CanWrite)
                    {
                        await process.StandardInput.WriteLineAsync(prologInput);
                    }
                    process.StandardInput.Close();

                    // Read the output and error
                    prologOutput = await process.StandardOutput.ReadToEndAsync();
                    var errorOutput = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        return BadRequest($"SWI-Prolog error: {errorOutput}");
                    }
                }

                // 4. Extract the cluster ID from the Prolog output.
                //    Assuming that your Prolog program prints something like:
                //      "Best matching cluster: <clusterId>"
                var match = Regex.Match(prologOutput, @"Best matching cluster:\s*(\d+)");
                if (!match.Success)
                {
                    return BadRequest("Could not parse cluster id from prolog output.");
                }
                var clusterId = int.Parse(match.Groups[1].Value);

                // 5. Use the cluster ID to find all matching product IDs from the cluster CSV file.
                var clusterCsvPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME);
                if (!System.IO.File.Exists(clusterCsvPath))
                {
                    return BadRequest("Cluster CSV file not found.");
                }

                var productIds = new List<int>();
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                };
                using (var reader = new StreamReader(clusterCsvPath))
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    // Assuming CSV has headers "ProductID", "ClusterAssignment"
                    var records = csv.GetRecords<dynamic>().ToList();
                    foreach (var record in records)
                    {
                        // We use dynamic binding to get the values.
                        // Adjust the property names as per your actual CSV headers.
                        int productId = int.Parse(record.ProductID.ToString());
                        int rowClusterId = int.Parse(record.ClusterAssignment.ToString());
                        if (rowClusterId == clusterId)
                        {
                            productIds.Add(productId);
                        }
                    }
                }

                // 6. Return the cluster ID along with the matching product IDs.
                var result = new
                {
                    ClusterId = clusterId,
                    MatchingProductIds = productIds
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }
    }
}
