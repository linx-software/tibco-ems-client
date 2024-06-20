/// <summary>
/// This is a sample of a basic message consumer.
///
/// This sample subscribes to specified destination and receives and prints all
/// received messages. 
///
/// Notice that the specified destination should exist in your configuration
/// or your topics/queues configuration file should allow creation of the
/// specified destination.
///
/// If this sample is used to receive messages published by csMsgProducer
/// sample, it must be started prior to running the csMsgProducer sample.
///
/// Usage: exe-file-name [options]
///
///    where options are:
///
///    -server    <server-url>  Server URL.
///                             If not specified this sample assumes a
///                             serverUrl of "tcp://localhost:7222"
///    -user      <user-name>   User name. Default is null.
///    -password  <password>    User password. Default is null.
///    -topic     <topic-name>  Topic name. Default value is "topic.sample"
///    -queue     <queue-name>  Queue name. No default
///    -ackmode   <mode>        Message acknowledge mode. Default is AUTO.
///                             Other values: DUPS_OK, CLIENT, EXPLICIT_CLIENT,
///                             EXPLICIT_CLIENT_DUPS_OK and NO.
///
/// </summary>

using System;
using TIBCO.EMS;

namespace LinxJMS;

public class MessageConsumer : IExceptionListener
{
    private string serverUrl;
    private string userName;
    private string password;
    private string name = "topic.sample";
    private bool useTopic = true;
    private int ackMode = Session.AUTO_ACKNOWLEDGE;

    private Connection connection;
    private Session session;
    private TIBCO.EMS.MessageConsumer msgConsumer;
    private Destination destination;

    public MessageConsumer(string[] args)
    {
        ParseArgs(args);

        try
        {
            SslHelpers.InitSSLParams(this.serverUrl, args);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
            Console.WriteLine(e.StackTrace);
            Environment.Exit(-1);
        }

        Console.WriteLine("\n------------------------------------------------------------------------");
        Console.WriteLine($"{nameof(MessageConsumer)} SAMPLE");
        Console.WriteLine("------------------------------------------------------------------------");
        Console.WriteLine("Server....................... " + (this.serverUrl != null ? this.serverUrl : "localhost"));
        Console.WriteLine("User......................... " + (this.userName != null ? this.userName : "(null)"));
        Console.WriteLine("Destination.................. " + this.name);
        Console.WriteLine("------------------------------------------------------------------------\n");

        try
        {
            Run();
        }
        catch (EMSException e)
        {
            Console.Error.WriteLine($"Exception in {nameof(MessageConsumer)}: " + e.Message);
            Console.Error.WriteLine(e.StackTrace);
        }
    }

    private void Usage()
    {
        Console.WriteLine("\nUsage: LinxJMS.exe -read [options]");
        Console.WriteLine("");
        Console.WriteLine("   where options are:");
        Console.WriteLine("");
        Console.WriteLine("   -server   <server URL> - Server URL, default is local server");
        Console.WriteLine("   -user     <user name>  - user name, default is null");
        Console.WriteLine("   -password <password>   - password, default is null");
        Console.WriteLine("   -topic    <topic-name> - topic name, default is \"topic.sample\"");
        Console.WriteLine("   -queue    <queue-name> - queue name, no default");
        Console.WriteLine("   -ackmode  <ack-mode>   - acknowledge mode, default is AUTO");
        Console.WriteLine("                            other modes: CLIENT, DUPS_OK, NO,");
        Console.WriteLine("                            EXPLICIT_CLIENT and EXPLICIT_CLIENT_DUPS_OK");
        Console.WriteLine("   -help-ssl              - help on ssl parameters");
        Environment.Exit(0);
    }

    private void ParseArgs(string[] args)
    {
        int i = 0;

        while (i < args.Length)
        {
            if (args[i].CompareTo("-server") == 0)
            {
                if (i + 1 >= args.Length)
                    Usage();
                this.serverUrl = args[i + 1];
                i += 2;
            }
            else
            if (args[i].CompareTo("-topic") == 0)
            {
                if (i + 1 >= args.Length)
                    Usage();
                this.name = args[i + 1];
                i += 2;
            }
            else
            if (args[i].CompareTo("-queue") == 0)
            {
                if (i + 1 >= args.Length)
                    Usage();
                this.name = args[i + 1];
                i += 2;
                this.useTopic = false;
            }
            else
            if (args[i].CompareTo("-user") == 0)
            {
                if (i + 1 >= args.Length)
                    Usage();
                this.userName = args[i + 1];
                i += 2;
            }
            else
            if (args[i].CompareTo("-password") == 0)
            {
                if (i + 1 >= args.Length)
                    Usage();
                this.password = args[i + 1];
                i += 2;
            }
            else
            if (args[i].CompareTo("-ackmode") == 0)
            {
                if (i + 1 >= args.Length)
                    Usage();
                if (args[i + 1].CompareTo("AUTO") == 0)
                    this.ackMode = Session.AUTO_ACKNOWLEDGE;
                else if (args[i + 1].CompareTo("CLIENT") == 0)
                    this.ackMode = Session.CLIENT_ACKNOWLEDGE;
                else if (args[i + 1].CompareTo("DUPS_OK") == 0)
                    this.ackMode = Session.DUPS_OK_ACKNOWLEDGE;
                else if (args[i + 1].CompareTo("EXPLICIT_CLIENT") == 0)
                    this.ackMode = Session.EXPLICIT_CLIENT_ACKNOWLEDGE;
                else if (args[i + 1].CompareTo("EXPLICIT_CLIENT_DUPS_OK") == 0)
                    this.ackMode = Session.EXPLICIT_CLIENT_DUPS_OK_ACKNOWLEDGE;
                else if (args[i + 1].CompareTo("NO") == 0)
                    this.ackMode = Session.NO_ACKNOWLEDGE;
                else
                {
                    Console.Error.WriteLine("Unrecognized -ackmode: " + args[i + 1]);
                    Usage();
                }
                i += 2;
            }
            else
            if (args[i].CompareTo("-help") == 0)
            {
                Usage();
            }
            else
            if (args[i].CompareTo("-help-ssl") == 0)
            {
                SslHelpers.SslUsage();
            }
            else
            if (args[i].StartsWith("-ssl"))
            {
                i += 2;
            }
            else
            if (args[i].StartsWith("-read"))
            {
                i += 1;
            }
            else
            {
                Console.Error.WriteLine("Unrecognized parameter: " + args[i]);
                Usage();
            }
        }
    }

    public void OnException(EMSException e)
    {
        // print the connection exception status
        Console.Error.WriteLine("Connection Exception: " + e.Message);
    }

    private void Run()
    {

        Message msg = null;

        Console.WriteLine("Subscribing to destination: " + this.name + "\n");

        ConnectionFactory factory = new ConnectionFactory(this.serverUrl);

        // create the connection
        this.connection = factory.CreateConnection(this.userName, this.password);

        // create the session
        this.session = this.connection.CreateSession(false, this.ackMode);

        // set the exception listener
        this.connection.ExceptionListener = this;

        // create the destination
        if (this.useTopic)
            this.destination = this.session.CreateTopic(this.name);
        else
            this.destination = this.session.CreateQueue(this.name);

        // create the consumer
        this.msgConsumer = this.session.CreateConsumer(this.destination);

        // start the connection
        this.connection.Start();

        // read messages
        while (true)
        {
            // receive the message
            msg = this.msgConsumer.ReceiveNoWait();
            if (msg == null)
                break;

            if (this.ackMode == Session.CLIENT_ACKNOWLEDGE ||
                this.ackMode == Session.EXPLICIT_CLIENT_ACKNOWLEDGE ||
                this.ackMode == Session.EXPLICIT_CLIENT_DUPS_OK_ACKNOWLEDGE)
                msg.Acknowledge();

            Console.WriteLine("Received message: \r\n=============\r\n" + msg + "\r\n=============");
            if (msg is BytesMessage)
            {
                BytesMessage bm = (BytesMessage)msg;
                Console.WriteLine(bm.ReadBoolean());
                Console.WriteLine(bm.ReadChar());
                Console.WriteLine(bm.ReadShort());
                Console.WriteLine(bm.ReadInt());
                Console.WriteLine(bm.ReadLong());
                Console.WriteLine(bm.ReadFloat());
                Console.WriteLine(bm.ReadDouble());
            }
        }

        // close the connection
        this.connection.Close();
    }
}