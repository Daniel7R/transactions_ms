using System.Runtime.Serialization;

namespace PaymentsMS.Application.Messages.Enums
{
    public enum TicketStatus
    {
        [EnumMember(Value = "ACTIVE")]
        ACTIVE,
        [EnumMember(Value = "USED")]
        USED,
        [EnumMember(Value = "CANCELED")]
        CANCELED,
        [EnumMember(Value = "GENERATED")]
        GENERATED
    }
}
