namespace library.Models;

public class MemberDetails
{
    public Member Member { get; set; } = new();
    public List<LoanView> Loans { get; set; } = new();
}