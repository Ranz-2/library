using System.ComponentModel.DataAnnotations;

namespace library.Models;

public class BorrowForm
{
    [Required(ErrorMessage = "Choose a member.")]
    public long? MemberId { get; set; }

    [Required(ErrorMessage = "Choose a book.")]
    public long? BookId { get; set; }

    [Range(1, 30, ErrorMessage = "A loan lasts from 1 to 30 days.")]
    public int DaysAllowed { get; set; } = 7;
}