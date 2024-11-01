using System.Diagnostics.CodeAnalysis;

namespace dn32.grpc.easy.server.model;

public readonly record struct InternalValuesForGrpcControllers
{
    public Type InterfaceType { get; init; }
    public Type ConcreteType { get; init; }

    [SetsRequiredMembers]
    public InternalValuesForGrpcControllers(Type interfaceType, Type concreteType)
    {
        InterfaceType = interfaceType;
        ConcreteType = concreteType;
    }
}
