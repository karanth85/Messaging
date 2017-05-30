namespace Pub
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;    
    using System.Threading;

    using Common;
    using CommandLine;
    using KafkaNet;
    using KafkaNet.Model;
    using KafkaNet.Protocol;
    using Model;
    using Newtonsoft.Json;
    using RabbitMQ.Client;
    using ZeroMQ;
    
    /// <summary>
    /// ZeroMq Pub Model Program
    /// </summary>
    class Program
    {
        private static string Topics = ConfigurationManager.AppSettings["Topics"];
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

                case Key.ZeroMq:
                    ZeroMq(options);
                    break;

                case Key.RabbitMq:
                    RabbitMq(options);
                    break;
            }

            Console.ReadLine();
        }
        
        private static void Kafka(Options options)
        {
            var kafkaOptions = new KafkaOptions(new Uri(KafkaEndpoint));
            var router = new BrokerRouter(kafkaOptions);
            var client = new Producer(router);

            foreach (var sub in options.publishedTopics)
            {    
                int i = 1;
                while (i < options.totalMessages + 1)
                {
                    var pubMessage = GetJSONData(Key.Kafka.ToString(), sub, options.fileData);
                                
                    Console.WriteLine(string.Format("Publishing Topic : {0}", sub));
                    client.SendMessageAsync(sub, new[] { new KafkaNet.Protocol.Message(pubMessage) });
                    Console.WriteLine(string.Format("Published Message : {0}", i));
                    i++;                   
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

                foreach (var sub in options.publishedTopics)
                {
                    int i = 1;

                    while (i < options.totalMessages + 1)
                    {
                        var pubMessage = GetJSONData(Key.RabbitMq.ToString(), sub, options.fileData);
                       
                        var body = Encoding.UTF8.GetBytes(pubMessage);
                        channel.BasicPublish(exchange: "pub.api", routingKey: sub, basicProperties: null, body: body);
                        Console.WriteLine(string.Format("Publishing Topic : {0}", sub));                                                
                        i++;
                    }
                }
            }
        }

        private static void ZeroMq(Options options)
        {
            using (var context = ZmqContext.Create())
            {
                using (var socket = context.CreateSocket(SocketType.PUB))
                {
                    socket.Bind(Endpoint);

                    //To prevent losing published messages
                    Thread.Sleep(1000);

                    foreach (var sub in options.publishedTopics)
                    {
                        int i = 1;

                        while (i < options.totalMessages + 1)
                        {
                            var pubMessage = GetJSONData(Key.ZeroMq.ToString(), sub, options.fileData);

                            var zmqMessage = new ZmqMessage();
                            zmqMessage.Append(Encoding.UTF8.GetBytes(sub));
                            zmqMessage.Append(Encoding.UTF8.GetBytes(pubMessage));

                            Console.WriteLine(string.Format("Publishing Topic : {0}", sub));
                            socket.SendMessage(zmqMessage);
                            Console.WriteLine("Published");
                            i++;
                        }
                    }
                }
            }
        }        

        /// <summary>
        /// Get JSON data to be published as content
        /// </summary>
        /// <param name="source">the source of data</param>
        /// <param name="topic">published topic</param>
        /// <returns></returns>
        private static string GetJSONData(string source, string topic, bool fileData)
        {
            var tContent = ConfigurationManager.AppSettings[topic].Split(',');
            
            var message = new Model.Message();
           
            message.Topic = topic;
            message.Source = source;

            if (!fileData)
            {
                var pubMessage = Common.GetRandomTopic(tContent);
                message.Content = pubMessage;
            }
            else
            {
                // Read 1MB of data for performance testing
                message.Content = File.ReadAllText("pub_data.txt");
            }
           
            message.Created = DateTime.UtcNow;
            
            var jsonPub = JsonConvert.SerializeObject(message);

            return jsonPub;
        }
    }
}
