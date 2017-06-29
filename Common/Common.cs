namespace Common
{
    using System;
    using System.Linq;
    
    public static class Common
    {
        public static string GetRandomTopic(string [] searchText)
        {
            var randNum = new Random(Guid.NewGuid().GetHashCode());

            var index = randNum.Next(searchText.Count());

            return searchText[index];
        }
    }
}
