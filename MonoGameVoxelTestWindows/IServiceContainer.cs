namespace MonoGameVoxelTestWindows;

public interface IServiceContainer
{
    void Register<TInterface, TImplementation>() where TImplementation : TInterface;
    void RegisterSingleton<TInterface>(TInterface instance);
    T Resolve<T>();
}
