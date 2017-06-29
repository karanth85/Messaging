namespace Server
{
    using System;
    using System.Configuration;
    using System.Text;

    using Common;
    using CommandLine;
    using DataAccess;
    using Model;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using ZeroMQ;

    /// <summary>
    /// ZeroMq Server Program
    /// </summary>
    class Program
    {
        private static string Endpoint = ConfigurationManager.AppSettings["ReqRepUrl"];

        static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));

            if (!parser.ParseArguments(args, options))
                Environment.Exit(1);

            Key source;

            Enum.TryParse(options.messageQueue, out source);

            switch (source)
            {
                case Key.ZeroMq:
                    ZeroMq();
                    break;

                case Key.RabbitMq:
                    RabbitMq();
                    break;
            }

            Console.ReadLine();
        }

        private static void ZeroMq()
        {
            using (var context = ZmqContext.Create())
            {
                using (var socket = context.CreateSocket(SocketType.REP))
                {
                    socket.Bind(Endpoint);

                    while (true)
                    {
                        Console.WriteLine("Receiving Request...");
                        var replyMsg = socket.Receive(Encoding.UTF8);

                        var message = JsonConvert.DeserializeObject<Message>(replyMsg);
                        message.Received = DateTime.UtcNow;

                        MessageDataStoreDao.InsertRecord(message);
                        Console.WriteLine(replyMsg);
                        socket.Send("Acknowledge from Server...", Encoding.UTF8);
                    }
                }
            }  
        }

        private static void RabbitMq()
        {
            Console.WriteLine("Listening RabbitMq Server");
            var factory = new ConnectionFactory() { HostName = "localhost" };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        while(true)
                        {
                            channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                            channel.BasicQos(0, 1, false);

                            var consumer = new EventingBasicConsumer(channel);

                            channel.BasicConsume(queue: "rpc_queue", noAck: false, consumer: consumer);
                            consumer.Received += (model, ea) =>
                            {
                                string response = null;

                                var body = ea.Body;
                                var props = ea.BasicProperties;
                                var replyProps = channel.CreateBasicProperties();
                                replyProps.CorrelationId = props.CorrelationId;

                                try
                                {
                                    var request = Encoding.UTF8.GetString(body);
                                    Console.WriteLine("Message from Client : {0}", request);
                                    var message = JsonConvert.DeserializeObject<Message>(request);
                                    message.Received = DateTime.UtcNow;

                                    MessageDataStoreDao.InsertRecord(message);
                                   
                                    response = "Message Received by Server";
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    response = "";
                                }
                                finally
                                {
                                    var responseBytes = Encoding.UTF8.GetBytes(response);
                                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                      basicProperties: replyProps, body: responseBytes);
                                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                      multiple: false);
                                }
                            };
                        }                        
                    }
                }            
        }
    }
}
