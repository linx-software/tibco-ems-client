using System;
using System.Linq;

namespace LinxJMS
{
    class Program
    {
        static void Main(string[] args)
        {

            var matchSend = args.FirstOrDefault(stringToCheck => stringToCheck.Contains("-send"));

            if (matchSend != null)
            {
              
                new csMsgProducer(args);
            }
            var matchRead = args.FirstOrDefault(stringToCheck => stringToCheck.Contains("-read"));

            if (matchRead != null)
            {
             
                new csMsgConsumer(args);
            }


            // Console.WriteLine("Hello World!");
        }
    }
}
