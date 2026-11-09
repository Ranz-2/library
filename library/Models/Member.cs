namespace library.Models;

using System.ComponentModel.DataAnnotations;

namespace library.Models;

public class Member
{
    public long MemberId { get; set; }

    [StringLength(80, ErrorMessage = "The name can be at most 80 characters.")]          // 1. → C
    [RegularExpression(@"^[A-Za-z]+,\s[A-Za-z]+$",
        ErrorMessage = "Write the name as Lastname, Firstname.")]                        // 2. → A
    public string FullName { get; set; } = "";

    [StringLength(120, ErrorMessage = "The email can be at most 120 characters.")]       // 3. → D
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[A-Za-z]+$",
        ErrorMessage = "Enter an email such as ana@ubaguio.edu.")]                       // 4. → B
    public string? Email { get; set; }

    public string MemberType { get; set; } = "";
}
// One Member object holds one row of lending.member.
public class Member
{
    public long MemberId { get; set; }           // member_id   BIGINT, primary key
    public string FullName { get; set; } = "";   // full_name   VARCHAR(80) NOT NULL
    public string? Email { get; set; }           // email       VARCHAR(120), may be NULL
    public string MemberType { get; set; } = ""; // member_type VARCHAR(10) NOT NULL
}