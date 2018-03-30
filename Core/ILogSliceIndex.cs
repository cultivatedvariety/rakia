namespace Core
{
    /**
     * Represents an index over an ILogSlice
     */
    public interface ILogSliceIndex
    {
        /**
         * Update the index with the kep and associated seek position
         */
        void UpdateIndex(byte[] key, long seekPosition);
        
        /***
         * Get the seek position for the associated key, returning NULL if the key does not exist
         */
        long? GetSeekPosition(byte[] key);

        void RemoveFromIndex(byte[] key);

        void Close();

    }
}