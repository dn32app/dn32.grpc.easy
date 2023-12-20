
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract]
public class GrpcContractExampleV1
{
    [DataMember(Order = 1)]
    public string? Name { get; set; }

    [DataMember(Order = 2)]
    public Dictionary<string, string>? Metadata { get; set; }
}
