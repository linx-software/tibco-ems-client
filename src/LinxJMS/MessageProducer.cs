/// <summary>
///  This is a sample of a basic message producer.
/// 
///  This samples publishes specified message(s) on a specified
///  destination and quits.
/// 
///  Notice that the specified destination should exist in your configuration
///  or your topics/queues configuration file should allow
///  creation of the specified topic or queue. Sample configuration supplied with
///  the TIBCO EMS allows creation of any destination.
/// 
///  If this sample is used to publish messages into
///  csMsgConsumer sample, the csMsgConsumer
///  sample must be started first.
/// 
///  If -topic is not specified this sample will use a topic named
///  "topic.sample".
/// 
///  Usage: exe-file-name [options]
///                                <message-text1>
///                                ...
///                                <message-textN>
/// 
///   where options are:
/// 
///    -server    <server-url>  Server URL.
///                             If not specified this sample assumes a
///                             serverUrl of "tcp://localhost:7222"
///    -user      <user-name>   User name. Default is null.
///    -password  <password>    User password. Default is null.
///    -topic     <topic-name>  Topic name. Default value is "topic.sample"
///    -queue     <queue-name>  Queue name. No default
/// 
/// </summary>

using System;
using System.Collections;
using TIBCO.EMS;

namespace LinxJMS;

public class MessageProducer
{
    private string serverUrl;
    private string userName;
    private string password;
    private string name = "topic.sample";
    private ArrayList data = [];
    private bool useTopic = true;
    private bool useAsync;

    private Connection connection;
    private Session session;
    private TIBCO.EMS.MessageProducer msgProducer;
    private Destination destination;

    private EMSCompletionListener completionListener;

    private class EMSCompletionListener : ICompletionListener
    {
        // Note:  Use caution when modifying a message in a completion
        // listener to avoid concurrent message use.

        public void OnCompletion(Message msg)
        {
            try
            {
                Console.WriteLine("Successfully sent message {0}.",
                    ((TextMessage)msg).Text);
            }
            catch (EMSException e)
            {
                Console.WriteLine("Error retrieving message text.");
                Console.WriteLine("Message: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public void OnException(Message msg, Exception ex)
        {
            try
            {
                Console.WriteLine("Error sending message {0}.",
                        ((TextMessage)msg).Text);
            }
            catch (EMSException e)
            {
                Console.WriteLine("Error retrieving message text.");
                Console.WriteLine("Message: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }

            Console.WriteLine("Message: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

    }

    public MessageProducer(string[] args)
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
        Console.WriteLine($"{nameof(MessageProducer)} SAMPLE");
        Console.WriteLine("------------------------------------------------------------------------");
        Console.WriteLine("Server....................... " + (this.serverUrl != null ? this.serverUrl : "localhost"));
        Console.WriteLine("User......................... " + (this.userName != null ? this.userName : "(null)"));
        Console.WriteLine("Destination.................. " + this.name);
        Console.WriteLine("Send Asynchronously.......... " + this.useAsync);
        Console.WriteLine("Message Text................. ");

        for (int i = 0; i < this.data.Count; i++)
        {
            Console.WriteLine(this.data[i]);
        }
        Console.WriteLine("------------------------------------------------------------------------\n");

        try
        {
            TextMessage msg;
            int i;

            if (this.data.Count == 0)
            {
                Console.Error.WriteLine("Error: must specify at least one message text\n");
                Usage();
            }

            Console.WriteLine("Publishing to destination '" + this.name + "'\n");

            ConnectionFactory factory = new ConnectionFactory(this.serverUrl);

            this.connection = factory.CreateConnection(this.userName, this.password);

            // create the session
            this.session = this.connection.CreateSession(false, Session.AUTO_ACKNOWLEDGE);

            // create the destination
            if (this.useTopic)
                this.destination = this.session.CreateTopic(this.name);
            else
                this.destination = this.session.CreateQueue(this.name);

            // create the producer
            this.msgProducer = this.session.CreateProducer(null);

            if (this.useAsync)
                this.completionListener = new EMSCompletionListener();

            // publish messages
            for (i = 0; i < this.data.Count; i++)
            {
                // create text message
                msg = this.session.CreateTextMessage();

                // set message text
                msg.Text = (string)this.data[i];

                // publish message
                if (this.useAsync)
                    this.msgProducer.Send(this.destination, msg, this.completionListener);
                else
                    this.msgProducer.Send(this.destination, msg);

                Console.WriteLine("Published message: " + this.data[i]);
            }

            // close the connection
            this.connection.Close();
        }
        catch (EMSException e)
        {
            Console.Error.WriteLine($"Exception in {nameof(MessageProducer)}: " + e.Message);
            Console.Error.WriteLine(e.StackTrace);
            Environment.Exit(-1);
        }
    }

    private void Usage()
    {
        Console.WriteLine("\nUsage: LinxJMS.exe -send [options]");
        Console.WriteLine("                       <message-text-1>");
        Console.WriteLine("                       [<message-text-2>] ...\n");
        Console.WriteLine("");
        Console.WriteLine("   where options are:");
        Console.WriteLine("");
        Console.WriteLine("   -server   <server URL>  - Server URL, default is local server");
        Console.WriteLine("   -user     <user name>   - user name, default is null");
        Console.WriteLine("   -password <password>    - password, default is null");
        Console.WriteLine("   -topic    <topic-name>  - topic name, default is \"topic.sample\"");
        Console.WriteLine("   -queue    <queue-name>  - queue name, no default");
        Console.WriteLine("   -async                   - send asynchronously, default is false");
        Console.WriteLine("   -help-ssl               - help on ssl parameters");
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
            if (args[i].CompareTo("-async") == 0)
            {
                i += 1;
                this.useAsync = true;
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
            if (args[i].StartsWith("-send"))
            {
                i += 1;
            }
            else
            {
                this.data.Add(args[i]);
                i++;
            }
        }
    }
}