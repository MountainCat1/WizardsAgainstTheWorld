namespace Utilities
{
    public interface ISaveable<T>
    {
        string GetFileName();
        T CreateDefault();
    }
}