using System.Runtime.Serialization;

namespace PaymentsMS.Domain.Enums
{
    public enum TransactionType
    {
        //In case there're more payment methods, this would be more scalable
        [EnumMember(Value = "SALE")]
        SALE,
        [EnumMember(Value = "DONATION")]
        DONATION
    }
}
