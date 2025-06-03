using Backend.Data;
using Backend.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> GetProducts([FromBody] int start, int end)
        {
            var products = await _context.Product.Skip(start).Take(end - start).ToArrayAsync();
            return Ok(products);
        }

        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetProduct([FromBody] int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
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
                return NotFound("Product not found.");
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
                return NotFound("Product not found.");
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
                return NotFound("Product not found.");
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
                return NotFound("Product not found.");
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully.");
        }

        [HttpPost("Import")]
        public async Task<IActionResult> Import([FromBody] string FilePath)
        {
            if (!System.IO.File.Exists(FilePath))
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

            using (var reader = new StreamReader(FilePath))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                while (await csv.ReadAsync())
                {
                    try
                    {
                        var product = new Product
                        {
                            Brands = csv.GetField("Brands"),
                            Models = csv.GetField("Models"),
                            Colors = csv.GetField("Colors"),
                            Memory = int.Parse(csv.GetField("Memory")
                                              .Replace("GB", string.Empty, StringComparison.OrdinalIgnoreCase)
                                              .Trim()),
                            Storage = int.Parse(csv.GetField("Storage")
                                               .Replace("GB", string.Empty, StringComparison.OrdinalIgnoreCase)
                                               .Trim()),
                            Camera = !string.IsNullOrWhiteSpace(csv.GetField("Camera")) && csv.GetField("Camera").Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            Rating = decimal.Parse(csv.GetField("Rating"), CultureInfo.InvariantCulture),
                            SellingPrice = decimal.Parse(csv.GetField("SellingPrice"), CultureInfo.InvariantCulture),
                            OriginalPrice = decimal.Parse(csv.GetField("OriginalPrice"), CultureInfo.InvariantCulture),
                            Mobile = csv.GetField("Mobile"),
                            Discount = decimal.Parse(csv.GetField("Discount"), CultureInfo.InvariantCulture),
                            DiscountPercentage = decimal.Parse(csv.GetField("DiscountPercentage"), CultureInfo.InvariantCulture),
                            OS = csv.GetField("OS"),
                            SellersAmount = int.Parse(csv.GetField("SellersAmount"), CultureInfo.InvariantCulture),
                            ScreenSize = decimal.Parse(csv.GetField("ScreenSize"), CultureInfo.InvariantCulture),
                            BatterySize = int.Parse(csv.GetField("BatterySize"), CultureInfo.InvariantCulture),
                            Reviews = int.Parse(csv.GetField("Reviews"), CultureInfo.InvariantCulture)
                        };

                        products.Add(product);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing record: {ex.Message}");
                    }
                }
            }

            _context.Product.AddRange(products);
            await _context.SaveChangesAsync();

            return Ok(new { ImportedCount = products.Count });
        }

        [HttpPost("Build")]
        public async Task<IActionResult> BuildExpertSystem()
        {
            try
            {
                var filePath = await CreateDatasetAsync();
                var clusterAssignments = await BuildKMeans(filePath);

                if (clusterAssignments == null || clusterAssignments.Length == 0) return BadRequest();

                CreateClusterAsync(clusterAssignments);
                var summaries = SummarizeClusters(filePath, clusterAssignments);
                var prolog = BuildPrologFile(summaries);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        public async Task<string> CreateDatasetAsync()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Constants.DATASET_FILE_NAME);

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

            return filePath;
        }

        private async Task<int[]> BuildKMeans(string filePath)
        {
            try
            {
                var input = new
                {
                    FilePath = filePath,
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

                    return JsonSerializer.Deserialize<int[]>(output) ?? Array.Empty<int>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Array.Empty<int>();
            }
        }

        public async void CreateClusterAsync(int[] clusterAssignments)
        {
            string assignmentFilePath = Path.Combine(Directory.GetCurrentDirectory(), "cluster_assignments.csv");

            using (var fileStream = new FileStream(assignmentFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                // Write header.
                csvWriter.WriteField("ProductRow");
                csvWriter.WriteField("ClusterAssignment");
                csvWriter.NextRecord();

                // Write each cluster assignment. Here we assume the array's index + 1 corresponds to the product row.
                for (int i = 0; i < clusterAssignments.Length; i++)
                {
                    csvWriter.WriteField(i + 1);
                    csvWriter.WriteField(clusterAssignments[i]);
                    csvWriter.NextRecord();
                }
                await streamWriter.FlushAsync();
            }
        }

        public List<ClusterSummary> SummarizeClusters(string filePath, int[] clusterAssignments)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);
            using var dr = new CsvDataReader(csv);

            var dataTable = new System.Data.DataTable();
            dataTable.Load(dr);

            if (dataTable.Rows.Count != clusterAssignments.Length)
                throw new ArgumentException("Cluster assignment length does not match the number of rows in CSV");

            // Build a dictionary of column name to data type.
            var columnTypes = dataTable.Columns.Cast<System.Data.DataColumn>()
                .ToDictionary(col => col.ColumnName, col => col.DataType);

            // Group row indices based on the cluster assignment.
            var grouped = Enumerable.Range(0, dataTable.Rows.Count)
                .GroupBy(i => clusterAssignments[i])
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<ClusterSummary>();

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
                        var mode = values.GroupBy(v => v.ToString().Trim().ToLowerInvariant())
                                         .OrderByDescending(g => g.Count())
                                         .First().Key;
                        summary.Features[col] = mode;
                    }
                }
                result.Add(summary);
            }

            return result;
        }

        // Generates a Prolog (.pl) file from the cluster summaries, returning the full file path.
        private string BuildPrologFile(List<ClusterSummary> summaries)
        {
            string fileName = "expert_system.pl";
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // Delete any existing file with the same name.
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            using (var writer = new StreamWriter(fullPath))
            {
                // Write a header comment.
                writer.WriteLine("% Expert System rules generated from cluster summaries");
                writer.WriteLine();

                foreach (var summary in summaries)
                {
                    // For each cluster, build a list of feature terms.
                    var featuresTerms = summary.Features.Select(kvp =>
                        $"feature({EscapePrologIdentifier(kvp.Key)}, {FormatPrologValue(kvp.Value)})");
                    string featuresList = "[" + string.Join(", ", featuresTerms) + "]";

                    // Write a clause for the cluster.
                    writer.WriteLine($"cluster({summary.ClusterId}, {featuresList}).");
                }
            }

            return fullPath;
        }

        // Helper: Formats a value as a Prolog term.
        private string FormatPrologValue(object value)
        {
            if (value is string)
            {
                // Escape single quotes if any exist.
                string escaped = value.ToString().Replace("'", "\\'");
                return $"'{escaped}'";
            }
            else if (value is bool)
            {
                return (bool)value ? "true" : "false";
            }
            else
            {
                return value.ToString();
            }
        }

        // Helper: Ensures a string is a valid Prolog atom (lowercase, quoted if necessary).
        private string EscapePrologIdentifier(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "'unknown'";

            string lower = id.ToLowerInvariant();

            // Check if the identifier is simple (starts with a lowercase letter followed by alphanumeric underscores).
            if (Regex.IsMatch(lower, "^[a-z][a-z0-9_]*$"))
                return lower;
            else
                return $"'{lower}'";
        }
    }
}
