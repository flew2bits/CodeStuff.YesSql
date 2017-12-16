using System;
using System.Collections.Generic;
using System.Text;
using YesSql.Indexes;

namespace CodeStuff.YesSql
{
    public class BlogPostByMonth: ReduceIndex
    {
        public string PublishedMonth { get; set; }
        public int Count { get; set; }
    }
}
