using System;
using System.Linq;
using System.Text;
using Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SliceIndex sliceIndex = null;
            try
            {
                byte[] bytes = new byte[2] {1,1};
                byte[] bytes2 = new byte[] {1, 1};
                Console.WriteLine(bytes.SequenceEqual(bytes2));
                foreach (var b in bytes)
                {
                    Console.Write(b);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            sliceIndex?.Close();
            Console.WriteLine("\n\nDone");
            Console.ReadLine();
        }
    }
}