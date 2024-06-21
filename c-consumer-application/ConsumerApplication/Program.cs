using DependentDLL;
using System;


namespace ConsumerApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            DependentDLLClass p = new DependentDLLClass();
            Console.WriteLine(p.GetStringFromPrimaryDLL());
            Console.Read();
        }
    }
}
