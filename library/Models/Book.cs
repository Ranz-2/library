namespace library.Models;

using System.ComponentModel.DataAnnotations;

namespace library.Models;

public class Book
{
    public long BookId { get; set; }

    [StringLength(150, ErrorMessage = "The title can be at most 150 characters.")]   // 1. → B
    public string Title { get; set; } = "";

    public string? Category { get; set; }

    [Range(0.01, 99999.99, ErrorMessage = "The price must be from 0.01 to 99,999.99")] // 2. → A
    public decimal? Price { get; set; }
}
// One Book object holds one row of lending.book.
public class Book
{
    public long BookId { get; set; }          // book_id   BIGINT, the primary key
    public string Title { get; set; } = "";   // title     VARCHAR(150) NOT NULL
    public string? Category { get; set; }     // category  VARCHAR(40), may be NULL
    public decimal? Price { get; set; }       // price     NUMERIC(7,2), may be NULL

}

