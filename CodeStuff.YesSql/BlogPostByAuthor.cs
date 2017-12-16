using System;
using System.Collections.Generic;
using System.Text;
using YesSql.Indexes;

namespace CodeStuff.YesSql
{
    public class BlogPostByAuthor: MapIndex
    {
        public string Author { get; set; }

        public string Title { get; set; }
    }
}
