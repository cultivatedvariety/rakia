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
        long Append(byte[] key, byte[] value);

        /**
         * Check if this slice contains a given key, returning
         * the seek position of the kvp if it does or null if not
         */
        long? Contains(byte[] key);

        /**
         * Read the key value pair from the specfied seek position
         */
        (byte[] key, byte[] value) Read(long seekPosition);
    }
}