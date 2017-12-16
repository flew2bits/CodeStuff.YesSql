using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YesSql.Indexes;

namespace CodeStuff.YesSql
{
    class BlogPostIndexProvider : IndexProvider<BlogPost>
    {
        public override void Describe(DescribeContext<BlogPost> context)
        {
            context.For<BlogPostByAuthor>().Map(blogPost => new BlogPostByAuthor {
                Author = blogPost.Author,
                Title = blogPost.Title
            });

            context.For<BlogPostByTag>().Map(blogPost => blogPost.Tags.Select(t => new BlogPostByTag { Tag = t }));

            context.For<BlogPostByMonth>().Map(blogPost => new BlogPostByMonth
            {
                PublishedMonth = blogPost.Published.ToString("yyyyMM"),
                Count = 1
            })
            .Group(map => map.PublishedMonth)
            .Reduce(group => new BlogPostByMonth { PublishedMonth = group.Key, Count = group.Sum(m => m.Count) })
            .Delete((index, map) =>
            {
                index.Count -= map.Sum(x => x.Count);
                return index.Count > 0 ? index : null;
            });
        }
    }
}
