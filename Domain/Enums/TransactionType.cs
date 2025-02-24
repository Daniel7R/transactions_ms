using System.Runtime.Serialization;

namespace TransactionMS.Domain.Enums
{
    public enum TransactionType
    {
        //In case there're more payment methods, this would be more scalable
        [EnumMember(Value = "ONLINE_PAYMENT")]
        ONLINE_PAYMENT
    }
}
