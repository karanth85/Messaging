namespace Model
{
    using System;
    using System.Runtime.Serialization;
    
    [DataContract]
    public class Message
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "topic")]
        public string Topic { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "received")]
        public DateTime Received { get; set; }
    }
}
