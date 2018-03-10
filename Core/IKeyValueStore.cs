namespace Core
{
    /**
     * Represents a key to value store
     */
    public interface IKeyValueStore
    {
        byte[] Get(byte[] key);
        void Put(byte[] key, byte[] value);
        void Delete(byte[] key);
    }
}