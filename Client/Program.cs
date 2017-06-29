namespace Client
{
    using System;
    using System.Configuration;
    using System.Text;

    using Common;
    using CommandLine;
    using ZeroMQ;
    using Model;
    using Newtonsoft.Json;
    using RabbitMQ.Client;

    /// <summary>
    /// ZeroMq Client Program
    /// </summary>
    class Program
    {
        private static IConnection connection;
        private static IModel channel;
        private static string replyQueueName;
        private static QueueingBasicConsumer consumer;

        private static string Endpoint = ConfigurationManager.AppSettings["ReqRepUrl"];
        private static string Messages = ConfigurationManager.AppSettings["Messages"];

        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));

            if (!parser.ParseArguments(args, options))
                Environment.Exit(1);

            var msgContent = Messages.Split(',');

            Key source;

            Enum.TryParse(options.messageQueue, out source);

            switch (source)
            {
                case Key.ZeroMq:
                    ZeroMq(options, msgContent);
                    break;

                case Key.RabbitMq:
                    RabbitMq(options, msgContent);
                    break;
            }

            Console.ReadLine();
        }

        private static void RabbitMq(Options options, string[] msgContent)
        {
            Console.WriteLine("Total Messages : " + options.totalMessages);

            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queue: replyQueueName,
                                 noAck: true,
                                 consumer: consumer);

            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;

            long i = 1;

            while (i < options.totalMessages + 1)
            {
                Console.WriteLine("Sending Request # " + i);
                var corrId = Guid.NewGuid().ToString();
                props.CorrelationId = corrId;

                var message = new Message();

                message.Created = DateTime.UtcNow;
                message.Source = Key.RabbitMq.ToString();
                message.Content = Common.GetRandomTopic(msgContent);

                var json = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(json);
               
                channel.BasicPublish(exchange: "",
                                     routingKey: "rpc_queue",
                                     basicProperties: props,
                                     body: messageBytes);
                Console.WriteLine("Request Sent # " + i);
                i++;                
            }
        }

        private static void ZeroMq(Options options, string[] msgContent)
        {
            using (var context = ZmqContext.Create())
            {
                using (var socket = context.CreateSocket(SocketType.REQ))
                {
                    socket.Connect(Endpoint);

                    long i = 1;

                    Console.WriteLine("Total Messages : " + options.totalMessages);

                    while (i < options.totalMessages + 1)
                    {
                        Console.WriteLine("Sending Request#" + i);

                        var message = new Message();

                        message.Created = DateTime.UtcNow;
                        message.Source = Key.ZeroMq.ToString();

                        message.Content = Common.GetRandomTopic(msgContent);

                        var json = JsonConvert.SerializeObject(message);

                        socket.Send(json, Encoding.UTF8);
                        var replyMsg = socket.Receive(Encoding.UTF8);
                        Console.WriteLine(replyMsg);
                        i++;
                    }
                }
            }
        }
    }
}
