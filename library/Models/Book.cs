using System.ComponentModel.DataAnnotations;

namespace library.Models;

public class Book
{
    public long BookId { get; set; }

    [StringLength(150, ErrorMessage = "The title can be at most 150 characters.")]
    public string Title { get; set; } = "";

    public string? Category { get; set; }

    [Range(0.01, 99999.99, ErrorMessage = "The price must be from 0.01 to 99,999.99")]
    public decimal? Price { get; set; }
}