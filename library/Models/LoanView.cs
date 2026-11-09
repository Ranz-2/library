namespace library.Models;

public class LoanView
{
    public long LoanId { get; set; }
    public long MemberId { get; set; }
    public string MemberName { get; set; } = "";
    public string BookTitle { get; set; } = "";
    public DateTime LoanedOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
    public int DaysAllowed { get; set; }
    public string Status { get; set; } = "";
}