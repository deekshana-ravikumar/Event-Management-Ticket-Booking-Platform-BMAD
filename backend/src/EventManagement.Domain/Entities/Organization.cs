using EventManagement.Domain.Enums;

namespace EventManagement.Domain.Entities;

/// <summary>
/// Minimal organisation record created atomically alongside the Organizer User in S1.
/// Additional columns (Logo, Description, Address, PAN, GSTIN, etc.) are added via S2 migration.
/// </summary>
public class Organization
{
    public long Id { get; set; }

    /// <summary>One-to-one with User. Unique index enforced in DbContext.</summary>
    public long UserId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public OrganizationStatus Status { get; set; } = OrganizationStatus.PendingApproval;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
