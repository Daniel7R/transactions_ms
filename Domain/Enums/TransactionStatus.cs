using System.Runtime.Serialization;

namespace PaymentsMS.Domain.Enums
{
    public enum TransactionStatus
    {
        [EnumMember(Value ="pending")]
        pending,
        [EnumMember(Value = "canceled")]
        canceled,
        [EnumMember(Value = "failed")]
        failed,
        [EnumMember(Value = "succeeded")]
        succeeded
    }
}
