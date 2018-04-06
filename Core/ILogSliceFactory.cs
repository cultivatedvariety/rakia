namespace Core
{
    public interface ILogSliceFactory
    {
        ILogSlice CreateSlice(string filePath);
    }
}