using System;
using System.Diagnostics.CodeAnalysis;

namespace dn32.grpc.easy.server.model;

public readonly record struct InternalValuesForGrpcControllers
{
    internal Type InterfaceType { get; init; }
    internal Type ConcreteType { get; init; }

    [SetsRequiredMembers]
    internal InternalValuesForGrpcControllers(Type interfaceType, Type concreteType)
    {
        InterfaceType = interfaceType;
        ConcreteType = concreteType;
    }
}
