using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BussinessCard.Server.Data.Enumerations;

namespace BussinessCard.Server.Data.Entities;

public class Card
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CardId { get; set; }
    public string Name { get; set; }
    public GenderEnum Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }

    [MaxLength(1048576)]
    public string Photo { get; set; }
    public async Task<string> ProcessPhotoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }
        if (file.Length > 1048576)
        {
            throw new ArgumentException("File size should not exceed 1MB.");
        }
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var fileBytes = ms.ToArray();
        return Convert.ToBase64String(fileBytes);
    }
}

public class CardsCSV
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Photo { get; set; }
}