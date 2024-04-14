namespace IdentityService.Models;
public class UserFinance
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public decimal Credit { get; set; }
    public decimal Debit { get; set; }
    public DateTimeOffset ActionDate { get; set; }
    public int Status { get; set; }
    public decimal LastBalance { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

}