using BussinessCard.Server.Data;
using BussinessCard.Server.Data.Entities;
using BussinessCard.Server.Data.Enumerations;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Drawing;
using System.Formats.Asn1;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace BussinessCard.Server.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BusinessCardController : ControllerBase
    {
        private readonly BussinessCardDbContext _context;
        private readonly ILogger<BusinessCardController> _logger;

        public BusinessCardController(BussinessCardDbContext context, ILogger<BusinessCardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetImage/{imageId}")]
        public IActionResult GetImage(string imageId)
        {
            // Construct the path to the QR code image using the ID
            string uploadPath = Path.Combine("uploads", "photos");
            string filePath = Path.Combine(uploadPath, $"{imageId}.png"); // Assuming the file extension is .png

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "image/png");
            }
            else
            {
                return NotFound("Image not found.");
            }
        }

        #region Create New Card
        [HttpPost("CreateBusinessCardFromFile")]
        public async Task<IActionResult> CreateBusinessCardFromFile(IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                switch (extension)
                {
                    case ".xml":
                        await ParseXml(file);
                        break;

                    case ".csv":
                        await ParseCsv(file);
                        break;

                    case var ext when IsImageFile(ext):
                        var qrCard = await ParseQrCode(file);
                        if (qrCard != null)
                        {
                            _context.Cards.Add(qrCard);
                            await GenerateAndSaveQrCodeAsync(qrCard);
                            await _context.SaveChangesAsync();
                            return Ok();
                        }
                        return BadRequest("Failed to parse QR code.");

                    default:
                        return BadRequest("Unsupported file format. Only XML, CSV, and image files for QR codes are supported.");
                }

                return Ok();
            }
            return BadRequest("No File Uploaded");
        }


        [HttpPost("CreateBusinessCard")]
        public async Task<IActionResult> CreateBusinessCard([FromBody] Card cardDto)
        {
            var newCard = new Card
            {
                Name = cardDto.Name,
                Gender = cardDto.Gender,
                DateOfBirth = cardDto.DateOfBirth,
                Email = cardDto.Email,
                Phone = cardDto.Phone,
                Address = cardDto.Address,
                Photo = string.Empty
            };

            _context.Cards.Add(newCard);
            await GenerateAndSaveQrCodeAsync(newCard);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("UpdateBusinessCard/{id}")]
        public async Task<IActionResult> UpdateBusinessCard(int id, [FromBody] Card cardDto)
        {
            var card = await _context.Cards.FindAsync(id);

            if (card == null)
            {
                return NotFound(); // Return 404 if not found
            }

            card.Name = cardDto.Name;
            card.Gender = cardDto.Gender;
            card.DateOfBirth = cardDto.DateOfBirth;
            card.Email = cardDto.Email;
            card.Phone = cardDto.Phone;
            card.Address = cardDto.Address;


            await GenerateAndSaveQrCodeAsync(card);
            await _context.SaveChangesAsync();

            return Ok(card);
        }

        #endregion

        #region Get list card with filters 

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var card = await _context.Cards.FirstOrDefaultAsync(wh => wh.CardId == id);
            if (card == null)
                return NotFound();

            return Ok(card);
        }

        [HttpGet("GetBusinessCards")]
        public async Task<IActionResult> GetBusinessCards(
                      string? name = null,
                      DateTime? dateOfBirth = null,
                      string? phone = null,
                      string? gender = null,
                      string? email = null)
        {
            var query = _context.Cards.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }
            if (dateOfBirth.HasValue)
            {
                query = query.Where(c => c.DateOfBirth == dateOfBirth.Value.Date);
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(c => c.Phone.Contains(phone));
            }
            if (!string.IsNullOrWhiteSpace(gender))
            {
                if (Enum.TryParse<GenderEnum>(gender, true, out var genderEnum))
                {
                    query = query.Where(c => c.Gender == genderEnum);
                }
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(c => c.Email.Contains(email));
            }

            var cards = await query.ToListAsync();

            if (cards == null || !cards.Any())
            {
                return NotFound("No business cards found.");
            }

            return Ok(cards);
        }


        #endregion

        #region DeleteById 

        [HttpDelete("DeleteBusinessCard/{id}")]
        public async Task<IActionResult> DeleteBusinessCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);

            if (card == null)
            {
                return NotFound("Business card not found.");
            }

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return Ok();
        }

        #endregion

        #region Parsing Functions 
        private async Task<Card?> ParseQrCode(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using var bitmap = new System.Drawing.Bitmap(stream);

                    var width = bitmap.Width;
                    var height = bitmap.Height;
                    var rgbData = new byte[width * height * 3];

                    int index = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var color = bitmap.GetPixel(x, y);
                            rgbData[index++] = color.R;
                            rgbData[index++] = color.G;
                            rgbData[index++] = color.B;
                        }
                    }

                    var luminanceSource = new ZXing.RGBLuminanceSource(rgbData, width, height);
                    var binarizer = new ZXing.Common.HybridBinarizer(luminanceSource);
                    var binaryBitmap = new ZXing.BinaryBitmap(binarizer);

                    var reader = new ZXing.QrCode.QRCodeReader();
                    var result = reader.decode(binaryBitmap);

                    if (result != null && !string.IsNullOrWhiteSpace(result.Text))
                    {
                        return JsonConvert.DeserializeObject<Card>(result.Text);
                    }
                    else
                    {
                        _logger.LogWarning("No QR code found or it contains no data.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR code parsing failed for the uploaded file: {FileName}", file.FileName);
            }
            return null;
        }
        private async Task ParseXml(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                var document = XDocument.Load(stream);

                var cards = document.Root.Elements("Card")
                    .Select(element => new Card
                    {
                        Name = element.Element("Name")?.Value ?? string.Empty,
                        Gender = element.Element("Gender")?.Value switch
                        {
                            "Male" => GenderEnum.Male,
                            "Female" => GenderEnum.Female,
                        },
                        DateOfBirth = DateTime.TryParse(element.Element("DateOfBirth")?.Value, out var dateOfBirth) ? dateOfBirth : (DateTime?)null,
                        Email = element.Element("Email")?.Value ?? string.Empty,
                        Phone = element.Element("Phone")?.Value ?? string.Empty,
                        Address = element.Element("Address")?.Value ?? string.Empty,
                        Photo = element.Element("Photo")?.Value ?? string.Empty
                    })
                    .ToList();

                _context.Cards.AddRange(cards);

                foreach (var card in cards)
                    await GenerateAndSaveQrCodeAsync(card);

                await _context.SaveChangesAsync();
            }
        }

        private async Task ParseCsv(IFormFile file)
        {

            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<CardsCSV>();

                var cards = records.Select(record => new Card
                {
                    Name = record?.Name ?? string.Empty,
                    Gender = record?.Gender switch
                    {
                        "Male" => GenderEnum.Male,
                        "Female" => GenderEnum.Female,
                    },
                    DateOfBirth = record?.DateOfBirth,
                    Email = record?.Email ?? string.Empty,
                    Phone = record?.Phone ?? string.Empty,
                    Address = record?.Address ?? string.Empty,
                    Photo = record?.Photo ?? string.Empty
                }).ToList();


                _context.Cards.AddRange(cards);

                foreach (var card in cards)
                    await GenerateAndSaveQrCodeAsync(card);

                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Exporting 

        [HttpGet("ExportToXml")]
        public async Task<IActionResult> ExportToXml()
        {
            var cards = await _context.Cards.ToListAsync();

            if (cards == null || !cards.Any())
            {
                return NotFound("No business cards to export.");
            }

            var xml = new XElement("BusinessCards",
                cards.Select(card => new XElement("Card",
                    new XElement("Name", card.Name),
                    new XElement("Gender", card.Gender),
                    new XElement("DateOfBirth", card.DateOfBirth),
                    new XElement("Email", card.Email),
                    new XElement("Phone", card.Phone),
                    new XElement("Address", card.Address),
                    new XElement("Photo", card.Photo)
                ))
            );

            var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xml.ToString());
            var stream = new MemoryStream(xmlBytes);
            return File(stream, "application/xml", "BusinessCards.xml");
        }

        [HttpGet("ExportToCsv")]
        public async Task<IActionResult> ExportToCsv()
        {
            var cards = await _context.Cards.ToListAsync();

            if (cards == null || !cards.Any())
            {
                return NotFound("No business cards to export.");
            }

            var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteField("Name");
                csvWriter.WriteField("Gender");
                csvWriter.WriteField("DateOfBirth");
                csvWriter.WriteField("Email");
                csvWriter.WriteField("Phone");
                csvWriter.WriteField("Address");
                csvWriter.WriteField("Photo");
                await csvWriter.NextRecordAsync();

                await csvWriter.WriteRecordsAsync(cards);
                await streamWriter.FlushAsync();
            }

            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "BusinessCards.csv");
        }

        #endregion

        #region SavePhoto

        private async Task<string> GenerateAndSaveQrCodeAsync(Card card)
        {
            // Generate the QR code image
            Bitmap qrCodeImage = GenerateQrCode(card);

            // Create a unique filename using GUID
            string qrCodeFileName = Guid.NewGuid().ToString(); // Generate a unique filename
            var uploadPath = Path.Combine("uploads", "photos");
            Directory.CreateDirectory(uploadPath); // Ensure the directory exists

            // Full path for the QR code image
            var qrCodeFilePath = Path.Combine(uploadPath, qrCodeFileName + ".png");

            // Save the QR code image
            qrCodeImage.Save(qrCodeFilePath, System.Drawing.Imaging.ImageFormat.Png);

            // Save QR code file path in the Card object (assuming you want just the filename)
            card.Photo = qrCodeFileName; // Save the QR code image path in the Card object

            // Optionally, return the full path of the saved QR code image for reference
            return qrCodeFilePath;
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            return imageExtensions.Contains(extension);
        }

        #endregion

        public Bitmap GenerateQrCode(Card card)
        {
            // Serialize the Card object to JSON
            string jsonData = JsonConvert.SerializeObject(card);

            // Create a QR code writer
            var writer = new QRCodeWriter();

            // Generate the QR code
            var qrCodeMatrix = writer.encode(jsonData, ZXing.BarcodeFormat.QR_CODE, 250, 250);

            // Convert the QR code to a bitmap
            var qrCodeBitmap = new Bitmap(qrCodeMatrix.Width, qrCodeMatrix.Height);
            for (int x = 0; x < qrCodeMatrix.Width; x++)
            {
                for (int y = 0; y < qrCodeMatrix.Height; y++)
                {
                    qrCodeBitmap.SetPixel(x, y, qrCodeMatrix[x, y] ? Color.Black : Color.White);
                }
            }

            return qrCodeBitmap;
        }
    }
}
