using Bogus;
using ClinicHub.Application.Common.Models;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicHub.Persistence.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ClinicHubContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var settings = scope.ServiceProvider.GetRequiredService<IOptions<SeedingSettings>>().Value;
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");

            if (!settings.Enabled) return;

            // 1. Seed Users if none exist (besides maybe an initial admin)
            if (await userManager.Users.CountAsync() <= 1)
            {
                logger.LogInformation("Seeding {Count} users...", settings.UserCount);
                var userFaker = new Faker<ApplicationUser>()
                    .RuleFor(u => u.FullName, f => f.Name.FullName())
                    .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FullName))
                    .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FullName))
                    .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("010########"))
                    .RuleFor(u => u.BirthDate, f => f.Date.Past(30, DateTime.Now.AddYears(-18)))
                    .RuleFor(u => u.EmailConfirmed, true);

                var users = userFaker.Generate(settings.UserCount);
                foreach (var user in users)
                {
                    var result = await userManager.CreateAsync(user, "Mo@123456");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserType.User.ToString());
                    }
                }
            }

            var allUsers = await userManager.Users.ToListAsync();
            var userIds = allUsers.Select(u => u.Id).ToList();

            // 2. Seed Posts
            if (!await context.Posts.AnyAsync())
            {
                logger.LogInformation("Seeding {Count} posts...", settings.PostCount);
                var postFaker = new Faker<Post>()
                    .CustomInstantiator(f => new Post(
                        f.Lorem.Paragraphs(1, 3),
                        f.PickRandom(userIds)));

                var posts = postFaker.Generate(settings.PostCount);
                context.Posts.AddRange(posts);
                await context.SaveChangesAsync();
            }

            var allPosts = await context.Posts.ToListAsync();

            // 3. Seed Comments & Reactions
            if (!await context.Comments.AnyAsync())
            {
                logger.LogInformation("Seeding comments and reactions...");
                var faker = new Faker();
                
                foreach (var post in allPosts)
                {
                    // Add comments
                    for (int i = 0; i < settings.CommentsPerPost; i++)
                    {
                        var authorId = faker.PickRandom(userIds);
                        var comment = post.AddComment(faker.Lorem.Sentence(), authorId);
                        context.Add(comment); // Force Added state
                        
                        // Randomly add a reply
                        if (faker.Random.Bool(0.3f))
                        {
                            var reply = comment.AddReply(faker.Lorem.Sentence(), faker.PickRandom(userIds));
                            context.Add(reply); // Force Added state
                        }
                    }

                    // Add reactions (ensuring unique users per post to avoid concurrency issues with soft-delete)
                    var reactionAuthors = faker.PickRandom(userIds, Math.Min(settings.ReactionsPerPost, userIds.Count)).ToList();
                    foreach (var authorId in reactionAuthors)
                    {
                        post.AddReaction(faker.PickRandom<ReactionType>(), authorId);
                        // Access the newly added reaction to add it to context tracking explicitly
                        var reaction = post.Reactions.First(r => r.AuthorId == authorId);
                        context.Add(reaction); // Force Added state
                    }

                    // Add media
                    if (faker.Random.Bool(0.5f))
                    {
                        post.AddMedia(faker.Image.PicsumUrl(), faker.PickRandom<MediaType>());
                        var media = post.Media.Last();
                        context.Add(media); // Force Added state
                    }
                }

                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database seeding completed successfully.");
        }
    }
}
