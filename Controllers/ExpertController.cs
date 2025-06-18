using Backend.Data;
using Backend.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
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
        
        [HttpGet("Get/Product/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return BadRequest("Product not found.");
            }

            return Ok(product);
        }

        [HttpGet("Get/Products")]
        public async Task<IActionResult> GetProducts(int start, int end)
        {
            var products = await _context.Product.Skip(start).Take(end - start).ToArrayAsync();
            return Ok(products);
        }

        [HttpGet("Get/Products/{ids}")]
        public async Task<IActionResult> GetProducts(int[] ids)
        {
            var products = await _context.Product.Where(p => ids.Contains(p.ProductID)).ToArrayAsync();
            return Ok(products);
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] ProductRequest request)
        {
            if (request == null) return BadRequest("Invalid product request.");

            var product = new Product
            {
                Brands = request.Brands,
                Models = request.Models,
                Colors = request.Colors,
                Memory = request.Memory,
                Storage = request.Storage,
                Camera = request.Camera,
                Rating = request.Rating,
                OriginalPrice = request.OriginalPrice,
                Mobile = request.Mobile,
                DiscountPercentage = request.DiscountPercentage,
                OS = request.OS,
                SellersAmount = request.SellersAmount,
                ScreenSize = request.ScreenSize,
                BatterySize = request.BatterySize,
                Reviews = request.Reviews
            };

            product.SellingPrice = product.OriginalPrice * (100 - product.DiscountPercentage) / 100;
            product.Discount = product.OriginalPrice - product.SellingPrice;

            var newProductData = new
            {
                Memory = product.Memory,
                Storage = product.Storage,
                Rating = product.Rating,
                OriginalPrice = product.OriginalPrice,
                DiscountPercentage = product.DiscountPercentage,
                SellersAmount = product.SellersAmount,
                ScreenSize = product.ScreenSize,
                BatterySize = product.BatterySize,
                Reviews = request.Reviews
            };
            var jsonInput = JsonSerializer.Serialize(newProductData);
            //var jsonInput = Newtonsoft.Json.JsonConvert.SerializeObject(newProductData);

            var assignedCluster = -1;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = string.Format("\"{0}\"", Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PYTHON_VENV)),
                    Arguments = string.Format("\"{0}\"", Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PYTHON_ASSIGN_CLUSTER_SCRIPT_FILE_PATH)),
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
                            await sw.WriteLineAsync(jsonInput);
                        }
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0) throw new Exception("Errors: " + error);

                    if (!int.TryParse(output.Trim(), out assignedCluster)) throw new Exception("Could not parse the cluster assignment from the script output.");
                }

                var clusterFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME);
                if (!System.IO.File.Exists(clusterFilePath)) throw new Exception("Could not find cluster assignment file.");

                _context.Product.Add(product);
                await _context.SaveChangesAsync();

                using (var fileStream = new FileStream(clusterFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                using (var streamWriter = new StreamWriter(fileStream))
                using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteField(product.ProductID);
                    csvWriter.WriteField(assignedCluster);
                    csvWriter.NextRecord();
                    await streamWriter.FlushAsync();
                }

                return Ok(new { ProductId = product.ProductID, ClusterAssignment = assignedCluster });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while assigning cluster: {ex.Message}");
                return BadRequest();
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
        {
            if (request == null) return BadRequest("Invalid product request.");

            var product = await _context.Product.FindAsync(id);
            if (product == null) return BadRequest("Product not found.");

            product.Brands = request.Brands;
            product.Models = request.Models;
            product.Colors = request.Colors;
            product.Memory = request.Memory;
            product.Storage = request.Storage;
            product.Camera = request.Camera;
            product.Rating = request.Rating;
            product.OriginalPrice = request.OriginalPrice;
            product.Mobile = request.Mobile;
            product.DiscountPercentage = request.DiscountPercentage;
            product.OS = request.OS;
            product.SellersAmount = request.SellersAmount;
            product.ScreenSize = request.ScreenSize;
            product.BatterySize = request.BatterySize;
            product.Reviews = request.Reviews;

            product.SellingPrice = product.OriginalPrice * (100 - product.DiscountPercentage) / 100;
            product.Discount = product.OriginalPrice - product.SellingPrice;

            var newProductData = new
            {
                Memory = product.Memory,
                Storage = product.Storage,
                Rating = product.Rating,
                OriginalPrice = product.OriginalPrice,
                DiscountPercentage = product.DiscountPercentage,
                SellersAmount = product.SellersAmount,
                ScreenSize = product.ScreenSize,
                BatterySize = product.BatterySize,
                Reviews = product.Reviews
            };
            var jsonInput = JsonSerializer.Serialize(newProductData);
            //var jsonInput = Newtonsoft.Json.JsonConvert.SerializeObject(newProductData);

            var assignedCluster = -1;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = string.Format("\"{0}\"", Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PYTHON_VENV)),
                    Arguments = string.Format("\"{0}\"", Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PYTHON_ASSIGN_CLUSTER_SCRIPT_FILE_PATH)),
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
                            await sw.WriteLineAsync(jsonInput);
                        }
                    }

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0) throw new Exception("Errors: " + error);

                    if (!int.TryParse(output.Trim(), out assignedCluster)) throw new Exception("Could not parse the cluster assignment from the script output.");
                }

                var clusterFilePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME);
                if (!System.IO.File.Exists(clusterFilePath)) throw new Exception("Could not find cluster assignment file.");

                var records = new List<dynamic>();
                using (var reader = new StreamReader(clusterFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    records = csv.GetRecords<dynamic>().ToList();
                }

                var updated = false;
                foreach (var record in records)
                {
                    if (record.ProductID.ToString() == product.ProductID.ToString())
                    {
                        record.ClusterAssignment = assignedCluster.ToString();
                        updated = true;
                        break;
                    }
                }
                if (!updated) throw new Exception("Could not find record.");

                using (var writer = new StreamWriter(clusterFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest();
            }

            return Ok(new { ProductId = product.ProductID, ClusterAssignment = assignedCluster });
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
        public async Task<IActionResult> BuildExpertSystem(bool rebuildDataset = true, bool rebuildCluster = true, bool rebuildProlog = true, int k = -1)
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
                        K = k,
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

                        if (process.ExitCode != 0) throw new Exception("Errors: " + error);

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
                        /* ───────────────────────── 1. HEADER ───────────────────────── */
                        writer.WriteLine(":- use_module(library(readutil)).");
                        writer.WriteLine(":- use_module(library(lists)).");
                        writer.WriteLine(":- use_module(library(dcg/basics)).");
                        writer.WriteLine(":- dynamic cluster/2.\n");

                        writer.WriteLine(":- if(\\+current_predicate(string_trim/2)).");
                        writer.WriteLine("string_trim(In,Out):-string_codes(In,C),phrase(trimmed(T),C),string_codes(Out,T).\n");
                        writer.WriteLine("trimmed(T)-->blanks,string(T),blanks,eos.");
                        writer.WriteLine(":- endif.\n");

                        /* ───────────────────────── 2. CLUSTER FACTS ───────────────────────── */
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
                        writer.WriteLine();

                        /* ───────────────────────── 3. NUMERIC UTILS ───────────────────────── */
                        writer.WriteLine("maybe_number(A,N):-catch(atom_number(A,N),_,fail).\n");
                        writer.WriteLine("numeric_similarity(U,C,S):-maybe_number(U,Un),maybe_number(C,Cn),");
                        writer.WriteLine("    D is abs(Un-Cn), S is 1/(1+D).  % 1/(1+|Δ|)\n");

                        /* ───────────────────────── 4. FEATURE-LEVEL SIM -------------------- */
                        writer.WriteLine("feature_similarity(feature(K,U),feature(K,C),S):-");
                        writer.WriteLine("    ( maybe_number(U,_),maybe_number(C,_) ->");
                        writer.WriteLine("        numeric_similarity(U,C,S)");
                        writer.WriteLine("    ; (U==C->S=1;S=0) ).\n");

                        /* ───────────────────────── 5. CLUSTER SCORING ---------------------- */
                        writer.WriteLine("cluster_score(UFs,CID,Score):-");
                        writer.WriteLine("    cluster(CID,CFs),");
                        writer.WriteLine("    findall(S,(member(Fu,UFs),member(Fc,CFs),feature_similarity(Fu,Fc,S)),Ss),");
                        writer.WriteLine("    sum_list(Ss,Score).\n");

                        /* ───────────────────────── 6. BEST CLUSTERS (ties) ----------------- */
                        writer.WriteLine("epsilon(1.0e-6).");
                        writer.WriteLine("best_clusters(UFs,IDs,Best):-");
                        writer.WriteLine("    findall(S-C,(cluster_score(UFs,C,S)),Pairs),");
                        writer.WriteLine("    pairs_keys(Pairs,Scores),max_list(Scores,Best),epsilon(E),");
                        writer.WriteLine("    findall(C,(member(S-C,Pairs),abs(S-Best)=<E),IDs).\n");

                        /* ───────────────────────── 7. INPUT & MAIN -------------------------- */
                        writer.WriteLine("parse_input(UFs):-");
                        writer.WriteLine("    writeln('Enter facts as feature(key,value); feature(...).'),");
                        writer.WriteLine("    read_line_to_codes(user_input,Cs0),");
                        writer.WriteLine("    (append(Cs,[46],Cs0)->true;Cs=Cs0), % strip '.'");
                        writer.WriteLine("    atom_codes(A,Cs),atomic_list_concat(Atoms,';',A),");
                        writer.WriteLine("    findall(feature(K,V),(member(Raw,Atoms),");
                        writer.WriteLine("        atom_string(Raw,S0),string_trim(S0,S),S\\='',");
                        writer.WriteLine("        atom_to_term(S,feature(K,V),_)),UFs).");
                        writer.WriteLine();
                        writer.WriteLine("query_from_input(UFs):-");
                        writer.WriteLine("    best_clusters(UFs,IDs,Best),");
                        writer.WriteLine("    format('Top score ~2f, clusters ~w~n',[Best,IDs]).");
                        writer.WriteLine();
                        writer.WriteLine("main:-");
                        writer.WriteLine("    parse_input(UFs),best_clusters(UFs,IDs,Best),");
                        writer.WriteLine("    format('Top score ~2f, clusters ~w~n',[Best,IDs]).");
                        writer.WriteLine();
                        writer.WriteLine("start:-");
                        writer.WriteLine("    catch((");
                        writer.WriteLine("        prolog_current_frame(Frame),");
                        writer.WriteLine("        prolog_frame_attribute(Frame,parent_goal,Goal),");
                        writer.WriteLine("        ( Goal == user -> main ; true )");
                        writer.WriteLine("    ), _, true).");
                        writer.WriteLine();
                        writer.WriteLine(":- initialization(start).");
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
                var feats = new List<string>();
                if (!string.IsNullOrWhiteSpace(input.Brands)) Add("brands", Atom(input.Brands));
                if (!string.IsNullOrWhiteSpace(input.Colors)) Add("colors", Atom(input.Colors));
                if (input.Memory is { } mem)                  Add("memory", $"'{mem}'");
                if (input.Storage is { } sto)                 Add("storage", $"'{sto}'");
                if (input.Rating is { } rat)                  Add("rating", $"'{rat}'");
                if (input.SellingPrice is { } price)          Add("sellingprice", $"'{price}'");
                if (input.DiscountPercentage is { } disc)     Add("discountpercentage", $"'{disc}'");
                if (!string.IsNullOrWhiteSpace(input.OS))     Add("os", Atom(input.OS));
                if (input.SellersAmount is { } sellers)       Add("sellersamount", $"'{sellers}'");
                if (input.ScreenSize is { } scr)              Add("screensize", $"'{scr}'");
                if (input.BatterySize is { } bat)             Add("batterysize", $"'{bat}'");
                if (input.Reviews is { } rev)                 Add("reviews", $"'{rev}'");

                string Atom(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value)) return "''";

                    var esc = value.Replace("'", "\\'");
                    return $"'{esc}'";
                }
                void Add(string key, string value) => feats.Add($"feature({key},{value})");

                var goal = $"query_from_input([{string.Join(",", feats)}])";
                var arguments = $"/c swipl -q -f \"{Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PROLOG_FILE_NAME)}\" -g \"{goal}\" -t halt";

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(error)) throw new Exception($"Prolog error: {error}");

                output = output.Trim();
                var rx = new Regex(@"Top\s+score\s+([-0-9.]+),\s*clusters\s*\[([0-9,\s]*)\]", RegexOptions.IgnoreCase);

                var m = rx.Match(output);
                if (!m.Success) throw new Exception($"Unexpected Prolog output:\n{output}");

                var score = decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                var idMatches = m.Groups[2].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Int32.Parse).ToArray();
                var clusterCsvPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME);

                if (!System.IO.File.Exists(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.CLUSTER_FILE_NAME))) throw new Exception("Cluster CSV file not found.");

                var productIds = new List<int>();
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };
                using (var reader = new StreamReader(clusterCsvPath))
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    var records = csv.GetRecords<dynamic>().ToList();
                    foreach (var record in records)
                    {
                        int productId = -1;
                        int rowClusterId = -1;
                        int.TryParse(record.ProductID.ToString(), out productId);
                        int.TryParse(record.ClusterAssignment.ToString(), out rowClusterId);

                        if (productId != -1 && idMatches.Contains(rowClusterId)) productIds.Add(productId);
                    }
                }

                var result = new
                {
                    TopScore = score,
                    ClusterIds = idMatches,
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

        [HttpPost("GetExpertResultsTest")]
        public async Task<IActionResult> GetExpertResultsTest()
        {
            var input = "feature(memory,'4'),feature(brands,'SAMSUNG'),feature(rating,'4.3'),feature(storage,'64')";
            var goal = $"query_from_input([{input}])";
            var arguments = $"/c swipl -q -f \"{Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.PROLOG_FILE_NAME)}\" -g \"{goal}\" -t halt";

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(error))
                return BadRequest($"Prolog error: {error}");

            return Ok(output.Trim());
        }
    }
}
