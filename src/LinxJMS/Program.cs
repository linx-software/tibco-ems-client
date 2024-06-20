using System.Linq;
using LinxJMS;

var matchSend = args.Any(stringToCheck => stringToCheck.Contains("-send"));
if (matchSend)
{
    new MessageProducer(args);
}

var matchRead = args.Any(stringToCheck => stringToCheck.Contains("-read"));
if (matchRead)
{
    new MessageConsumer(args);
}
