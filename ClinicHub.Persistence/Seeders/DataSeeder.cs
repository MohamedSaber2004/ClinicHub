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

            // Ensure all users have at least a default role assigned (fixes existing users created before role-assignment checks)
            var rolelessUsers = await userManager.Users.ToListAsync();
            foreach (var user in rolelessUsers)
            {
                var existingRoles = await userManager.GetRolesAsync(user);
                if (existingRoles.Count == 0)
                {
                    await userManager.AddToRoleAsync(user, UserType.User.ToString());
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
            var hasClinicsWithoutWorkingData = await context.Clinics.AnyAsync(c => c.WorkingDays == null);
            if (!await context.Clinics.AnyAsync() || hasClinicsWithoutWorkingData)
            {
                if (hasClinicsWithoutWorkingData)
                {
                    logger.LogInformation("Updating existing clinics with working day data...");
                }
                else
                {
                    logger.LogInformation("Seeding clinics...");
                }

                var specializations = await context.Specializations.ToListAsync();

                if (specializations.Any())
                {
                    var faker = new Faker();
                    var allDays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };

                    if (!await context.Clinics.AnyAsync())
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
                                    f.Random.Double(29.9, 31.5),
                                    f.Random.Double(30.9, 31.8)))
                                { SRID = 4326 },
                                IsRegistered = true,
                                SpecializationId = f.PickRandom(specializations).Id,
                                Rating = f.Random.Double(3.5, 5.0),
                                ImageUrl = f.Image.PicsumUrl(),
                                WorkingHours = "09:00 - 17:00",
                                WorkingHoursStart = new TimeOnly(9, 0),
                                WorkingHoursEnd = new TimeOnly(17, 0),
                                WorkingDays = string.Join(",", allDays)
                            });

                        var clinics = clinicFaker.Generate(10);
                        context.Clinics.AddRange(clinics);
                    }
                    else
                    {
                        var existingClinics = await context.Clinics.Where(c => c.WorkingDays == null).ToListAsync();
                        foreach (var clinic in existingClinics)
                        {
                            clinic.WorkingHours = "09:00 - 17:00";
                            clinic.WorkingHoursStart = new TimeOnly(9, 0);
                            clinic.WorkingHoursEnd = new TimeOnly(17, 0);
                            clinic.WorkingDays = string.Join(",", allDays);
                        }
                    }

                    await context.SaveChangesAsync();
                }
            }

            var allClinics = await context.Clinics.ToListAsync();

            // 4.5 Seed Booking Configurations (cash-only)
            if (!await context.Set<BookingConfiguration>().AnyAsync())
            {
                logger.LogInformation("Seeding booking configurations...");
                foreach (var clinic in allClinics)
                {
                    var config = new BookingConfiguration(
                        clinicId: clinic.Id,
                        consultationFee: 350m,
                        currency: "EGP",
                        maxAdvanceBookingDays: 30,
                        reservationTtlMinutes: 15);
                    context.Set<BookingConfiguration>().Add(config);
                }
                await context.SaveChangesAsync();
            }

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
                            yearsOfExperience: new Faker().Random.Int(1, 30)
                        );

                        doctors.Add(doctor);
                    }

                    context.Set<Doctor>().AddRange(doctors);
                    await context.SaveChangesAsync();
                }
            }

            // 6. Seed Doctor Availabilities (one-time: clears old Mon-Fri pattern if found, then seeds Sun-Thu)
            var existingSlots = await context.Set<DoctorAvailability>().IgnoreQueryFilters().ToListAsync();
            var hasOldPattern = existingSlots.Any(a => a.DayOfWeek == DayOfWeek.Friday || a.DayOfWeek == DayOfWeek.Saturday);

            if (!existingSlots.Any() || hasOldPattern)
            {
                if (hasOldPattern)
                {
                    logger.LogInformation("Replacing old availabilities with new Sun-Thu pattern...");
                    context.Set<DoctorAvailability>().RemoveRange(existingSlots);
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Seeding doctor availabilities...");
                var allDoctors = await context.Set<Doctor>().ToListAsync();

                if (allDoctors.Any())
                {
                    var availabilities = new List<DoctorAvailability>();
                    var faker = new Faker();

                    var workingDays = new[]
                    {
                        DayOfWeek.Sunday,
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday
                    };

                    foreach (var doctor in allDoctors.Take(Math.Min(settings.DoctorAvailabilityCount ?? 5, allDoctors.Count)))
                    {
                        foreach (var dayOfWeek in workingDays)
                        {
                            var startHour = faker.Random.Int(8, 10);
                            var hoursRange = dayOfWeek switch
                            {
                                DayOfWeek.Sunday => faker.Random.Int(4, 5),
                                DayOfWeek.Monday => faker.Random.Int(7, 8),
                                DayOfWeek.Tuesday => faker.Random.Int(6, 8),
                                DayOfWeek.Wednesday => faker.Random.Int(7, 8),
                                DayOfWeek.Thursday => faker.Random.Int(4, 6),
                                _ => 6
                            };

                            var slotDuration = dayOfWeek switch
                            {
                                DayOfWeek.Sunday => 45,
                                DayOfWeek.Monday => 30,
                                DayOfWeek.Tuesday => faker.Random.Bool() ? 30 : 45,
                                DayOfWeek.Wednesday => 30,
                                DayOfWeek.Thursday => 60,
                                _ => 30
                            };

                            availabilities.Add(new DoctorAvailability(
                                doctorId: doctor.Id,
                                clinicId: doctor.ClinicId.Value,
                                dayOfWeek: dayOfWeek,
                                startTime: new TimeSpan(startHour, 0, 0),
                                endTime: new TimeSpan(startHour, 0, 0).Add(TimeSpan.FromHours(hoursRange)),
                                slotDurationMinutes: slotDuration
                            ));
                        }
                    }

                    context.Set<DoctorAvailability>().AddRange(availabilities);
                    await context.SaveChangesAsync();
                }
            }

            var superAdminUser = allUsers.FirstOrDefault();

            // If SuperAdmin role exists, try to find a user with that role
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == nameof(UserType.SuperAdmin));
            if (superAdminRole != null)
            {
                var superAdminUserEntry = await context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.RoleId == superAdminRole.Id);
                if (superAdminUserEntry != null)
                {
                    superAdminUser = allUsers.FirstOrDefault(u => u.Id == superAdminUserEntry.UserId);
                }
            }

            // 8. Seed Support Tickets
            if (!await context.Set<SupportTicket>().AnyAsync())
            {
                logger.LogInformation("Seeding {Count} support tickets...", settings.SupportTicketCount);
                var ticketFaker = new Faker<SupportTicket>()
                    .CustomInstantiator(f => new SupportTicket
                    {
                        UserId = f.PickRandom(userIds),
                        ClinicId = f.Random.Bool(0.5f) ? f.PickRandom(allClinics).Id : null,
                        Subject = f.Lorem.Sentence(3, 8),
                        Description = f.Lorem.Paragraphs(1, 3),
                        Status = f.PickRandom<SupportTicketStatus>(),
                        Priority = f.PickRandom<SupportTicketPriority>(),
                        ResolvedAt = f.Random.Bool(0.3f) ? f.Date.Recent(5) : null
                    });

                var tickets = ticketFaker.Generate(settings.SupportTicketCount ?? 10);
                context.Set<SupportTicket>().AddRange(tickets);
                await context.SaveChangesAsync();
            }

            // 10. Seed Subscriptions
            if (!await context.Set<Subscription>().AnyAsync())
            {
                logger.LogInformation("Seeding {Count} subscriptions...", settings.SubscriptionCount);
                var subscriptionFaker = new Faker<Subscription>()
                    .CustomInstantiator(f =>
                    {
                        var clinic = f.PickRandom(allClinics);
                        var startDate = f.Date.Past(60);
                        return new Subscription
                        {
                            ClinicId = clinic.Id,
                            Plan = f.PickRandom<SubscriptionPlan>(),
                            StartDate = startDate,
                            EndDate = startDate.AddDays(f.Random.Int(30, 365)),
                            Status = f.PickRandom(new[] { SubscriptionStatus.Active, SubscriptionStatus.Active, SubscriptionStatus.Expired, SubscriptionStatus.Revoked }),
                            Amount = f.Random.Decimal(100, 5000),
                            PaidAt = f.Random.Bool(0.8f) ? f.Date.Recent(30) : null
                        };
                    });

                var subscriptions = subscriptionFaker.Generate(settings.SubscriptionCount ?? 5);
                context.Set<Subscription>().AddRange(subscriptions);
                await context.SaveChangesAsync();
            }

            // 11. Seed Advertisements
            if (!await context.Set<Advertisement>().AnyAsync())
            {
                logger.LogInformation("Seeding {Count} advertisements...", settings.AdvertisementCount);
                var adFaker = new Faker<Advertisement>()
                    .CustomInstantiator(f => new Advertisement
                    {
                        ClinicId = f.Random.Bool(0.7f) ? f.PickRandom(allClinics).Id : null,
                        Title = f.Commerce.ProductName(),
                        ImageUrl = f.Image.PicsumUrl(),
                        TargetUrl = f.Internet.Url(),
                        StartDate = f.Date.Past(10),
                        EndDate = f.Date.Future(30),
                        Status = f.PickRandom<AdvertisementStatus>(),
                        AmountPaid = f.Random.Decimal(50, 2000)
                    });

                var ads = adFaker.Generate(settings.AdvertisementCount ?? 5);
                context.Set<Advertisement>().AddRange(ads);
                await context.SaveChangesAsync();
            }

            // 12. Seed Audit Logs
            if (!await context.Set<AuditLog>().AnyAsync())
            {
                logger.LogInformation("Seeding {Count} audit logs...", settings.AuditLogCount);
                var logFaker = new Faker<AuditLog>()
                    .CustomInstantiator(f => new AuditLog
                    {
                        ClinicId = f.Random.Bool(0.6f) ? f.PickRandom(allClinics).Id : null,
                        UserId = f.PickRandom(allUsers).Id,
                        Action = f.PickRandom(new[] { "ClinicCreated", "ClinicUpdated", "ClinicActivated", "ClinicDeactivated", "DoctorAdded", "DoctorRemoved", "PaymentProcessed", "VerificationApproved", "VerificationRejected" }),
                        EntityType = f.PickRandom(new[] { "Clinic", "Doctor", "Payment", "Verification", "Subscription" }),
                        EntityId = Guid.NewGuid().ToString(),
                        OldValues = f.Random.Bool(0.3f) ? f.Lorem.Sentence() : null,
                        NewValues = f.Random.Bool(0.5f) ? f.Lorem.Sentence() : null,
                        Timestamp = f.Date.Recent(60)
                    });

                var logs = logFaker.Generate(settings.AuditLogCount ?? 20);
                context.Set<AuditLog>().AddRange(logs);
                await context.SaveChangesAsync();
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
