using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryStream ms = new MemoryStream();
            string str1 = "First String";
            string str2 = "Second String";
            byte[] strBuffer = Encoding.UTF8.GetBytes(str1);


            ms.Write(strBuffer, 0, strBuffer.Length);

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());

            ms.Write(strBuffer, 0, strBuffer.Length);


            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());


            int count = (int)ms.Length;
            byte[] outBuffer = new byte[count];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(outBuffer, 0, count);

            string outputStr = Encoding.UTF8.GetString(outBuffer);

            Console.WriteLine(outputStr);
            Console.ReadLine();

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());


             count = (int)ms.Length;
            outBuffer = new byte[count];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(outBuffer, 0, count);



            ms.Seek(0, SeekOrigin.Begin);

            str2 = "testtesttesttest";
             strBuffer = Encoding.UTF8.GetBytes(str2);

             ms.Write(strBuffer, 0, strBuffer.Length);







            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(outBuffer, 0, count);

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                ms.Capacity.ToString(),
                ms.Length.ToString(),
                ms.Position.ToString());

             outputStr = Encoding.UTF8.GetString(outBuffer);

            Console.WriteLine(outputStr);
            Console.ReadLine();


        }
    }
}
