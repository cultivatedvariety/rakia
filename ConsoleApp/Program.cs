using System;
using System.Text;
using Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SliceIndex sliceIndex = new SliceIndex(@"/Users/rob/Documents/work/rakia/ConsoleApp/slice.idx");
                byte[] key1 = Encoding.UTF8.GetBytes("key1");
                byte[] key2 = Encoding.UTF8.GetBytes("key1");
                Console.WriteLine(key1 == key2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.WriteLine("\n\nDone");
            Console.ReadLine();
        }
    }
}