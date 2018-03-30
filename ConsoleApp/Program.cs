using System;
using System.ComponentModel;
using System.Linq;
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
                byte[] b = BitConverter.GetBytes('\0');
                byte[] tb = BitConverter.GetBytes(-1L);
                byte[] tb2 = BitConverter.GetBytes(-1L);
                Console.WriteLine(tb == tb2);
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