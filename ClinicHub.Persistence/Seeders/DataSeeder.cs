using Bogus;
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

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

            // 4. Seed Clinics
            if (!await context.Clinics.AnyAsync())
            {
                logger.LogInformation("Seeding clinics...");
                var specializations = await context.Specializations.ToListAsync();

                if (specializations.Any())
                {
                    var clinicFaker = new Faker<Clinic>()
                        .CustomInstantiator(f => new Clinic
                        {
                            Name = f.Company.CompanyName(),
                            NameAr = f.Company.CompanyName(),
                            Address = f.Address.FullAddress(),
                            AddressAr = f.Address.FullAddress(),
                            Phone = f.Phone.PhoneNumber("010########"),
                            Location = new Point(new Coordinate(
                                f.Random.Double(29.9, 31.5),  // Cairo latitude range
                                f.Random.Double(30.9, 31.8))) // Cairo longitude range
                            { SRID = 4326 },
                            IsRegistered = true,
                            SpecializationId = f.PickRandom(specializations).Id,
                            Rating = f.Random.Double(3.5, 5.0),
                            ImageUrl = f.Image.PicsumUrl()
                        });

                    var clinics = clinicFaker.Generate(10);
                    context.Clinics.AddRange(clinics);
                    await context.SaveChangesAsync();
                }
            }

            var allClinics = await context.Clinics.ToListAsync();

            // 5. Seed Doctors
            if (!await context.Set<Doctor>().AnyAsync())
            {
                logger.LogInformation("Seeding doctors...");
                var specializations = await context.Specializations.ToListAsync();
                var doctorUsers = allUsers.Take(Math.Min(10, allUsers.Count)).ToList();

                if (allClinics.Any() && specializations.Any() && doctorUsers.Any())
                {
                    var doctors = new List<Doctor>();

                    for (int i = 0; i < Math.Min(5, allClinics.Count); i++)
                    {
                        var doctorUser = doctorUsers.Count > i ? doctorUsers[i] : doctorUsers[new Faker().Random.Int(0, doctorUsers.Count - 1)];
                        var clinic = allClinics[i % allClinics.Count];
                        var specialization = new Faker().PickRandom(specializations);

                        var doctor = new Doctor(
                            userId: doctorUser.Id,
                            clinicId: clinic.Id,
                            specializationId: specialization.Id,
                            bio: new Faker().Lorem.Sentence(10, 15),
                            consultationFee: new Faker().Random.Decimal(50, 300),
                            yearsOfExperience: new Faker().Random.Int(1, 30)
                        );

                        doctors.Add(doctor);
                    }

                    context.Set<Doctor>().AddRange(doctors);
                    await context.SaveChangesAsync();
                }
            }

            // 6. Seed Doctor Availabilities
            if (!await context.Set<DoctorAvailability>().AnyAsync())
            {
                logger.LogInformation("Seeding doctor availabilities...");
                var doctors = await context.Set<Doctor>().ToListAsync();

                if (doctors.Any())
                {
                    var availabilities = new List<DoctorAvailability>();
                    var faker = new Faker();

                    foreach (var doctor in doctors.Take(Math.Min(settings.DoctorAvailabilityCount ?? 5, doctors.Count)))
                    {
                        // Create availability for each day of the week
                        for (int day = 1; day <= 5; day++) // Monday to Friday
                        {
                            var dayOfWeek = (DayOfWeek)day;
                            var startTime = new TimeSpan(faker.Random.Int(8, 10), 0, 0);
                            var endTime = startTime.Add(TimeSpan.FromHours(faker.Random.Int(4, 8)));

                            availabilities.Add(new DoctorAvailability(
                                doctorId: doctor.Id,
                                dayOfWeek: dayOfWeek,
                                startTime: startTime,
                                endTime: endTime,
                                slotDurationMinutes: 30
                            ));
                        }
                    }

                    context.Set<DoctorAvailability>().AddRange(availabilities);
                    await context.SaveChangesAsync();
                }
            }

            // 7. Seed Appointments
            if (!await context.Appointments.AnyAsync())
            {
                logger.LogInformation("Seeding {Count} appointments...", settings.AppointmentCount);

                var doctors = await context.Set<Doctor>().Include(d => d.Clinic).ToListAsync();
                var clinics = await context.Set<Clinic>().ToListAsync();

                if (doctors.Any() && clinics.Any())
                {
                    var appointmentFaker = new Faker<Appointment>()
                        .CustomInstantiator(f =>
                        {
                            var doctor = f.PickRandom(doctors);
                            var clinic = f.PickRandom(clinics);
                            var appointmentDate = f.Date.FutureOffset(30).DateTime;
                            var startHour = f.Random.Int(8, 17);
                            var startTime = new TimeSpan(startHour, 0, 0);
                            var endTime = startTime.Add(TimeSpan.FromMinutes(30));

                            return new Appointment(
                                bookedByUserId: f.PickRandom(userIds),
                                doctorId: doctor.Id,
                                clinicId: clinic.Id,
                                appointmentDate: appointmentDate,
                                startTime: startTime,
                                endTime: endTime,
                                appointmentType: f.PickRandom<AppointmentType>(),
                                patientFullName: f.Name.FullName(),
                                patientPhoneNumber: f.Phone.PhoneNumber("010########"),
                                patientAge: f.Random.Int(18, 80),
                                patientGender: f.PickRandom<Gender>(),
                                complaint: f.Lorem.Sentence(5, 10),
                                chronicDiseases: f.Random.Bool(0.4f) ? f.Lorem.Words(f.Random.Int(1, 3)).Aggregate((a, b) => $"{a}, {b}") : null
                            );
                        });

                    var appointments = appointmentFaker.Generate(settings.AppointmentCount ?? 20);
                    context.Appointments.AddRange(appointments);
                    await context.SaveChangesAsync();
                }
            }

            logger.LogInformation("Database seeding completed successfully.");
        }
    }
}
