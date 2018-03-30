namespace Core
{
    /**
     * Represents a segment/portion/chunk of an append-only log.
     */
    public interface ILogSlice
    {
        /**
         * Size in bytes
         */
        long Size { get; }

        /**
         * Append a key-value pair to the log slice, returning the file seek position
         * at which the key starts
         */
        void Append(byte[] key, byte[] value);

        /**
         * Check if this slice contains a given key, returning
         * the seek position of the kvp if it does or null if not
         */
        bool Contains(byte[] key);

        /*
         * Remove a key and its associated value from the log
         */
        void Remove(byte[] key);

        /**
         * Get the value associated with the key
         */
        byte[] Get(byte[] key);
    }
}