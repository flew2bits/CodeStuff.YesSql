using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;
using YesSql.Provider.SqlServer;
using YesSql.Services;
using YesSql.Sql;

namespace CodeStuff.YesSql
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain(args).Wait();
        }

        static async Task AsyncMain(string[] args)
        {
            var firstNames = new[] { "Bob", "Jim", "John", "Mary", "Sarah", "Felix", "April" };
            var lastNames = new[] { "Jones", "Smith", "Peters", "Thompson" };
            var userNames = firstNames.Join(lastNames, x => true, y => true, (x, y) => $"{x} {y}").ToArray();
            var tags = new[] { "Foo", "Bar", "Baz", "Bat" };

            var store = new Store(config =>
            {
                config.UseSqlServer(@"Data Source=(localdb)\MSSQLLocaldb;Initial Catalog=CodeStuff;Integrated Security=True;Pooling=False");
            });
            store.RegisterIndexes<BlogPostIndexProvider>();

            ResetDatabase(store);

            await BuildDatabase(store);
            await CreateBlogPosts(store, userNames, tags, 500);

            //Console.WriteLine("All Posts\n");
            //await DumpAllPosts(store);

            //Console.WriteLine("\nPosts for author Bob Jones\n");
            //await DumpPostsForAuthor(store, "Bob Jones");

            //Console.WriteLine("\nPosts for author Bob Jones (index data only)\n");
            //await DumpPostsForAuthorIndexOnly(store, "Bob Jones");

            //Console.WriteLine("\nPosts for tag Foo\n");
            //await DumpPostsForTag(store, "Foo");

            //Console.WriteLine("\nPosts for author Bob Jones and tag Foo\n");
            //await DumpPostsForAuthorWithTag(store, "Bob Jones", "Foo");

            Console.WriteLine("\nPosts for author Bob Jones in January, 2017\n");
            await DumpPostsForAuthorInMonth(store, "Bob Jones", "201701");

            Console.WriteLine("\nPost count by month\n");
            await DumpPostCounts(store);
        }

        static void ResetDatabase(IStore store)
        {
            using (var session = store.CreateSession())
            {
                var builder = new SchemaBuilder(session) { ThrowOnError = false };

                builder.DropMapIndexTable(nameof(BlogPostByAuthor));
                builder.DropMapIndexTable(nameof(BlogPostByTag));
                //builder.DropReduceIndexTable(nameof(BlogPostByMonth));

                builder.DropTable(Store.DocumentTable);
                builder.DropTable(LinearBlockIdGenerator.TableName);
            }
        }

        static async Task BuildDatabase(IStore store)
        {
            await store.InitializeAsync();
            using (var session = store.CreateSession())
            {
                var builder = new SchemaBuilder(session);

                builder.CreateMapIndexTable(nameof(BlogPostByAuthor), table =>
                {
                    table.Column<string>(nameof(BlogPostByAuthor.Author));
                    table.Column<string>(nameof(BlogPostByAuthor.Title));
                });

                builder.CreateMapIndexTable(nameof(BlogPostByTag), table =>
                {
                    table.Column<string>(nameof(BlogPostByTag.Tag));
                });

                builder.CreateReduceIndexTable(nameof(BlogPostByMonth), table =>
                {
                    table.Column<string>(nameof(BlogPostByMonth.PublishedMonth));
                    table.Column<int>(nameof(BlogPostByMonth.Count));
                });
            }
        }

        static async Task CreateBlogPosts(IStore store, string[] userNames, string[] tags, int count)
        {
            var random = new Random();

            using (var session = store.CreateSession()) {
                for (var c = 0; c < count; c++)
                {
                    var post = new BlogPost
                    {
                        Title = $"Lorem Ipsum {c + 1}",
                        Author = userNames[random.Next(userNames.Count())],
                        Content = "Lorem ipsum sit dolor...",
                        Published = new DateTime(2017, 1, 1).AddDays(random.Next(365)),
                        Tags = tags.Where(_ => random.Next(2) == 1).ToArray()
                    };

                    session.Save(post);
                }
                await session.CommitAsync();
            }
        }

        static async Task DumpAllPosts(IStore store)
        {
            using (var session = store.CreateSession())
            {
                var posts = await session.Query<BlogPost>().ListAsync();
                foreach (var post in posts) Console.WriteLine(post);
            }
        }

        static async Task DumpPostsForAuthor(IStore store, string author)
        {
            using (var session = store.CreateSession())
            {
                var posts = await session.Query<BlogPost, BlogPostByAuthor>(b => b.Author == author).ListAsync();
                DumpPosts(posts);
            }
        }

        static async Task DumpPostsForAuthorIndexOnly(IStore store, string author)
        {
            using (var session = store.CreateSession())
            {
                var postIndex = await session.QueryIndex<BlogPostByAuthor>(b => b.Author == author).ListAsync();
                foreach (var post in postIndex) Console.WriteLine($"\"{post.Title}\" by {post.Author} (with document id {post.Id})");
            }
        }

        static async Task DumpPostsForTag(IStore store, string tag)
        {
            using (var session = store.CreateSession())
            {
                var posts = await session.Query<BlogPost, BlogPostByTag>(b => b.Tag == tag).ListAsync();
                DumpPosts(posts);
            }
        }

        static async Task DumpPostsForAuthorWithTag(IStore store, string author, string tag)
        {
            using (var session = store.CreateSession())
            {
                var posts = await session.Query<BlogPost>()
                    .With<BlogPostByAuthor>(b => b.Author == author)
                    .With<BlogPostByTag>(b => b.Tag == tag)
                    .ListAsync();

                DumpPosts(posts);
            }
        }

        static async Task DumpPostsForAuthorInMonth(IStore store, string author, string month)
        {
            using (var session = store.CreateSession())
            {
                var posts = await session
                    .Query<BlogPost>()
                    .With<BlogPostByMonth>(m => m.PublishedMonth == month)
                    .With<BlogPostByAuthor>(b => b.Author == author)
                    .ListAsync();
                DumpPosts(posts);
            }
        }

        static async Task DumpPostCounts(IStore store)
        {
            using (var session = store.CreateSession())
            {
                var entries = await session.QueryIndex<BlogPostByMonth>().ListAsync();
                foreach (var entry in entries.OrderByDescending(e => e.PublishedMonth))
                {
                    Console.WriteLine($"Month: {entry.PublishedMonth}, Count: {entry.Count}");
                }
            }
        }

        static void DumpPosts(IEnumerable<BlogPost> posts)
        {
            foreach (var post in posts) Console.WriteLine(post);
        }
    }
}
