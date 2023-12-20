
using System.Runtime.Serialization;

[DataContract]
public class GrpcResultContractExampleV1
{
    [DataMember(Order = 1)]
    public string? Result { get; set; }
}