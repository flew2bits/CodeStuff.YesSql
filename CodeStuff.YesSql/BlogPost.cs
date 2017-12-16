using System;
using System.Collections.Generic;
using System.Text;

namespace CodeStuff.YesSql
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime Published { get; set; }
        public string[] Tags { get; set; }

        public override string ToString()
        {
            return $"\"{Title}\" by {Author} published on {Published.ToShortDateString()} {TagList()}";
        }

        private string TagList()
        {
            return Tags.Length == 0 ? "(No tags)" : $"[{string.Join(",", Tags)}]";
        }
    }
}
