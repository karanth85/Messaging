using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
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
