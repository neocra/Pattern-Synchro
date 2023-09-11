using System;

namespace Pattern.Synchro.Client;

public class TypeToSync
{
    public Type Type { get; }

    public TypeToSync(Type type)
    {
        this.Type = type;
    }
}