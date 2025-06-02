using Back.Data;
using Back.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace Back.Controllers
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
        public async Task<IActionResult> GetProducts([FromBody] int start, [FromBody] int end)
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
        public async Task<IActionResult> Build()
        {
            

            return Ok();
        }
    }
}
