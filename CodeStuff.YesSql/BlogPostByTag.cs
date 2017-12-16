using System;
using System.Collections.Generic;
using System.Text;
using YesSql.Indexes;

namespace CodeStuff.YesSql
{
    public class BlogPostByTag: MapIndex
    {
        public string Tag { get; set; }
    }
}
