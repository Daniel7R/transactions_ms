using System.Runtime.Serialization;

namespace TransactionMS.Domain.Enums
{
    public enum TransactionStatus
    {
        [EnumMember(Value ="PENDING")]
        PENDING,
        [EnumMember(Value = "FAILED")]
        FAILED,
        [EnumMember(Value = "SUCCESS")]
        SUCCESS
    }
}
