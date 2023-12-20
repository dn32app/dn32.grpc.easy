using System;
using System.Diagnostics.CodeAnalysis;

namespace dn32.grpc.easy.server.model;

internal readonly record struct ValorInternoParaControladoresGrpc
{
    internal Type InterfaceType { get; init; }
    internal Type ConcreteType { get; init; }

    [SetsRequiredMembers]
    internal ValorInternoParaControladoresGrpc(Type interfaceType, Type concreteType)
    {
        InterfaceType = interfaceType;
        ConcreteType = concreteType;
    }
}
