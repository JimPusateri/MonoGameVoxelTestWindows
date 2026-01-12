using System;
using System.Collections.Generic;

namespace MonoGameVoxelTestWindows;

public class ServiceContainer : IServiceContainer
{
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Type> _transientTypes = new();

    public void Register<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _transientTypes[typeof(TInterface)] = typeof(TImplementation);
    }

    public void RegisterSingleton<TInterface>(TInterface instance)
    {
        _singletons[typeof(TInterface)] = instance;
    }

    public T Resolve<T>()
    {
        var type = typeof(T);
        
        if (_singletons.TryGetValue(type, out var singleton))
        {
            return (T)singleton;
        }
        
        if (_transientTypes.TryGetValue(type, out var implementationType))
        {
            return (T)Activator.CreateInstance(implementationType);
        }
        
        throw new InvalidOperationException($"Type {type.Name} not registered");
    }
}
