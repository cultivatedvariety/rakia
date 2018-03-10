namespace Core
{
    /**
     * Represents an index over an ILogSlice
     */
    public interface ISliceIndex
    {
        /**
         * Update the index with the kep and associated seek position
         */
        void Put(byte[] key, long seekPosition);
        
        /***
         * Get the seek position for the associated key, returning NULL if the key does not exist
         */
        long? Get(byte[] key);

    }
}