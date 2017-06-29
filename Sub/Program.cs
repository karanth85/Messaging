namespace Subscribe
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    using Common;
    using CommandLine;
    using DataAccess;
    using KafkaNet;
    using KafkaNet.Model;
    using Model;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using ZeroMQ;    
    
    /// <summary>
    /// ZeroMq Sub Model Program
    /// </summary>
    class Program
    {
        private static string Endpoint = ConfigurationManager.AppSettings["PubSubUrl"];
        private static string KafkaEndpoint = ConfigurationManager.AppSettings["KafkaUrl"];

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
                case Key.Kafka:
                    Kafka(options);
                    break;

                case Key.RabbitMq:
                    RabbitMq(options);
                    break;

                case Key.ZeroMq:
                    ZeroMq(options);
                    break;                
            }                            
        }

        private static void Kafka(Options options)
        {
            var kafkaOptions = new KafkaOptions(new Uri(KafkaEndpoint));
            var router = new BrokerRouter(kafkaOptions);

            int sequence = 1;
            foreach (var topic in options.subscribedTopics)
            {
                var consumer = new Consumer(new ConsumerOptions(topic, new BrokerRouter(kafkaOptions)));

                Console.WriteLine("Subscribed Topic: " + topic);

                foreach (var message in consumer.Consume())
                {
                    Console.WriteLine("Response: {0}", Encoding.UTF8.GetString(message.Value));
                    var msgData = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(message.Value));
                    msgData.Received = DateTime.UtcNow;

                    MessageDataStoreDao.InsertRecord(msgData);
                    Console.WriteLine(string.Format("Seq #{0} Received Topic : {1}", sequence, topic));
                    sequence++;
                }
            }
        }

        private static void RabbitMq(Options options)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "pub.api", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                if (options.subscribedTopics.Count() == 0)
                {
                    Console.WriteLine("Subscribed to All");
                    channel.QueueBind(queue: queueName, exchange: "pub.api", routingKey: "");
                }
                else
                {
                    Console.WriteLine("Subscribed Topic: " + options.subscribedTopics.Count());
                    foreach (var topic in options.subscribedTopics)
                    {
                        channel.QueueBind(queue: queueName, exchange: "pub.api", routingKey: topic);
                    }
                }

                int sequence = 1;
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var routingKey = ea.RoutingKey;
                    var message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(body));
                    message.Received = DateTime.UtcNow;
                    
                    MessageDataStoreDao.InsertRecord(message);
                    Console.WriteLine(string.Format("Seq #{0} Received Topic : {1}", sequence, ea.RoutingKey));
                    sequence++;
                };
                channel.BasicConsume(queue: queueName, noAck: true, consumer: consumer);
                
                Console.ReadLine();
            }
        }

        private static void ZeroMq(Options options)
        {
            using (var context = ZmqContext.Create())
            {
                using (var socket = context.CreateSocket(SocketType.SUB))
                {
                    if (options.subscribedTopics.Count() == 0)
                    {
                        Console.WriteLine("Subscribed to All");
                        socket.SubscribeAll();
                    }
                    else
                    {
                        Console.WriteLine("Subscribed Topic: " + options.subscribedTopics.Count());
                        foreach (var topic in options.subscribedTopics)
                            socket.Subscribe(Encoding.UTF8.GetBytes(topic));
                    }

                    socket.Connect(Endpoint);

                    int sequence = 1;

                    while (true)
                    {
                        Console.WriteLine("Receiving Subscribed Topic.");
                        var msg = socket.ReceiveMessage();
                        var message = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(msg[1]));
                        message.Received = DateTime.UtcNow;

                        MessageDataStoreDao.InsertRecord(message);                        
                        Console.WriteLine(string.Format("Seq #{0} Received Topic : {1}", sequence, Encoding.UTF8.GetString(msg[0])));
                        sequence++;
                    }
                }
            }  
        }
    }
}
