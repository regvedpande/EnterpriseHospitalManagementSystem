namespace Hospital.Web.Infrastructure.Messaging.Messages;

/// <summary>Base class for all domain events published to the message bus.</summary>
public abstract class HmsMessage
{
    public Guid   MessageId  { get; init; } = Guid.NewGuid();
    public string MessageType => GetType().Name;
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

// ── Appointment events ────────────────────────────────────────────────────────

public sealed class AppointmentCreatedMessage : HmsMessage
{
    public int    AppointmentId   { get; init; }
    public string PatientId       { get; init; } = "";
    public string PatientName     { get; init; } = "";
    public string PatientEmail    { get; init; } = "";
    public string DoctorName      { get; init; } = "";
    public DateTime AppointmentDate { get; init; }
    public string AppointmentType { get; init; } = "";
}

public sealed class AppointmentStatusChangedMessage : HmsMessage
{
    public int    AppointmentId { get; init; }
    public string PatientEmail  { get; init; } = "";
    public string PatientName   { get; init; } = "";
    public string OldStatus     { get; init; } = "";
    public string NewStatus     { get; init; } = "";
    public string DoctorName    { get; init; } = "";
}

// ── Billing events ────────────────────────────────────────────────────────────

public sealed class BillGeneratedMessage : HmsMessage
{
    public int     BillId       { get; init; }
    public string  PatientId    { get; init; } = "";
    public string  PatientEmail { get; init; } = "";
    public string  PatientName  { get; init; } = "";
    public decimal TotalAmount  { get; init; }
    public string  BillNumber   { get; init; } = "";
}

// ── Lab events ────────────────────────────────────────────────────────────────

public sealed class LabResultReadyMessage : HmsMessage
{
    public int    LabId         { get; init; }
    public string PatientEmail  { get; init; } = "";
    public string PatientName   { get; init; } = "";
    public string TestName      { get; init; } = "";
    public string ResultSummary { get; init; } = "";
}

// ── User events ───────────────────────────────────────────────────────────────

public sealed class UserRegisteredMessage : HmsMessage
{
    public string UserId    { get; init; } = "";
    public string Email     { get; init; } = "";
    public string Name      { get; init; } = "";
    public string? Phone    { get; init; }
}
