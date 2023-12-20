using System.ServiceModel;
using System.Threading.Tasks;

namespace shared.example.contract;

[ServiceContract]
public interface IExampleGrpcController
{
    Task<GrpcResultContractExampleV1> SimpleExampleAsync(GrpcContractExampleV1 exampleGrpcContract);
}