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
        public async Task<IActionResult> BuildExpertSystem(bool rebuildDataset = false, bool rebuildCluster = false, bool rebuildProlog = true)
        {
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.DATASET_FILE_NAME);

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
                                csvWriter.WriteRecord(product);
                                csvWriter.NextRecord();
                            }

                            offset += batch.Count;
                        }

                        await streamWriter.FlushAsync();
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

                if (clusterAssignments == null || clusterAssignments.Length == 0) return BadRequest();
                
                filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", Constants.DATASET_FILE_NAME);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);
                using var dr = new CsvDataReader(csv);

                var dataTable = new DataTable();
                dataTable.Load(dr);

                if (dataTable.Rows.Count != clusterAssignments.Length)
                    throw new ArgumentException("Cluster assignment length does not match the number of rows in CSV");

                // Build a dictionary of column name to data type.
                var columnTypes = dataTable.Columns.Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => col.DataType);

                columnTypes.Remove("ProductID");

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
                            var mode = values.GroupBy(v => v.ToString()).OrderByDescending(g => g.Count()).FirstOrDefault();
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
                                if (kvp.Value is string)
                                {
                                    var escaped = kvp.Value.ToString().Replace("'", "\\'");
                                    value = $"'{escaped}'";
                                }
                                else if (kvp.Value is bool)
                                {
                                    value = (bool)kvp.Value ? "true" : "false";
                                }
                                else
                                {
                                    value = kvp.Value.ToString();
                                }

                                return $"feature({key}, {value})";
                            });
                            var featuresList = "[" + string.Join(", ", featuresTerms) + "]";

                            writer.WriteLine($"cluster({summary.ClusterId}, {featuresList}).");
                        }
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
    }
}
