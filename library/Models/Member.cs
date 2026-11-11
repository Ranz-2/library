using System.ComponentModel.DataAnnotations;

namespace library.Models;

public class Member
{
    public long MemberId { get; set; }

    [StringLength(80, ErrorMessage = "The name can be at most 80 characters.")]
    [RegularExpression(@"^[A-Za-z-]+,\s[A-Za-z-]+$", ErrorMessage = "Write the name as Lastname, Firstname.")]
    public string FullName { get; set; } = "";

    [StringLength(120, ErrorMessage = "The email can be at most 120 characters.")]
    [RegularExpression(@"^[^@\s]+@[A-Za-z]+\.[A-Za-z]+$", ErrorMessage = "Enter an email such as ana@ubaguio.edu.")]
    public string? Email { get; set; }

    public string MemberType { get; set; } = "";

    public long OpenLoans { get; set; } // filled only by the member list; never saved
}