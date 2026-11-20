using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests;

public class ModelTests
{
    private static RaceDayDb CreateDatabase(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<RaceDayDb>()
            .UseSqlite(connection)
            .Options;

        var db = new RaceDayDb(options);
        db.Database.EnsureCreated();

        return db;
    }

    private static User CreateUser(string email, string role)
    {
        return new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = "test-hash",
            Role = role
        };
    }

    [Fact]
    public void User_CanBeSaved()
    {
        using var connection =
            new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var db = CreateDatabase(connection);

        db.Users.Add(CreateUser(
            "participant@raceday.co.za", "Participant"));

        db.SaveChanges();

        Assert.Single(db.Users);
    }

    [Fact]
    public void DuplicateEmail_IsRejectedByDatabase()
    {
        using var connection =
            new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var db = CreateDatabase(connection);

        db.Users.Add(CreateUser(
            "same@raceday.co.za", "Participant"));
        db.SaveChanges();

        db.Users.Add(CreateUser(
            "same@raceday.co.za", "Organiser"));

        Assert.Throws<DbUpdateException>(
            () => db.SaveChanges());
    }

    [Fact]
    public void Event_CanBeLinkedToOrganiser()
    {
        using var connection =
            new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var db = CreateDatabase(connection);

        var organiser = CreateUser(
            "organiser@raceday.co.za", "Organiser");

        db.Users.Add(organiser);
        db.SaveChanges();

        db.Events.Add(new RaceEvent
        {
            OrganiserId = organiser.UserId,
            EventName = "Test Run",
            EventDate = DateTime.UtcNow.Date.AddDays(10),
            StartTime = new TimeSpan(7, 0, 0),
            Location = "Johannesburg",
            DistanceKm = 10,
            EventType = "Run",
            Status = "Open"
        });

        db.SaveChanges();

        var raceEvent = db.Events
            .Include(e => e.Organiser)
            .Single();

        Assert.Equal(
            organiser.UserId, raceEvent.OrganiserId);
        Assert.Equal("Organiser", raceEvent.Organiser.Role);
    }

    [Fact]
    public void DuplicateEventCategory_IsRejected()
    {
        using var connection =
            new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var db = CreateDatabase(connection);

        var organiser = CreateUser(
            "organiser@raceday.co.za", "Organiser");

        db.Users.Add(organiser);

        var raceEvent = new RaceEvent
        {
            Organiser = organiser,
            EventName = "Test Run",
            EventDate = DateTime.UtcNow.Date.AddDays(10),
            StartTime = new TimeSpan(7, 0, 0),
            Location = "Johannesburg",
            DistanceKm = 10,
            EventType = "Run"
        };

        var category = new Category
        {
            CategoryName = "Open"
        };

        db.Events.Add(raceEvent);
        db.Categories.Add(category);
        db.SaveChanges();

        db.EventCategories.Add(new EventCategory
        {
            EventId = raceEvent.EventId,
            CategoryId = category.CategoryId,
            EntryFee = 100,
            MaximumParticipants = 50
        });
        db.SaveChanges();

        db.EventCategories.Add(new EventCategory
        {
            EventId = raceEvent.EventId,
            CategoryId = category.CategoryId,
            EntryFee = 150,
            MaximumParticipants = 100
        });

        Assert.Throws<DbUpdateException>(
            () => db.SaveChanges());
    }

    [Fact]
    public void Enrolment_CanHaveOnlyOneResult()
    {
        using var connection =
            new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var db = CreateDatabase(connection);

        var organiser = CreateUser(
            "organiser@raceday.co.za", "Organiser");

        var participant = CreateUser(
            "participant@raceday.co.za", "Participant");

        db.Users.AddRange(organiser, participant);

        var raceEvent = new RaceEvent
        {
            Organiser = organiser,
            EventName = "Test Run",
            EventDate = DateTime.UtcNow.Date.AddDays(10),
            StartTime = new TimeSpan(7, 0, 0),
            Location = "Johannesburg",
            DistanceKm = 10,
            EventType = "Run"
        };

        var category = new Category
        {
            CategoryName = "Open"
        };

        var eventCategory = new EventCategory
        {
            Event = raceEvent,
            Category = category,
            EntryFee = 100,
            MaximumParticipants = 50
        };

        var enrolment = new Enrolment
        {
            Participant = participant,
            EventCategory = eventCategory,
            Status = "Completed"
        };

        db.Enrolments.Add(enrolment);
        db.SaveChanges();

        db.Results.Add(new RaceResult
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = new TimeSpan(0, 52, 31),
            Position = 1
        });
        db.SaveChanges();


        var duplicateResult = new RaceResult
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = new TimeSpan(0, 55, 0),
            Position = 2
        };

        db.ChangeTracker.Clear();

        db.Results.Add(duplicateResult);

        Assert.Throws<DbUpdateException>(
            () => db.SaveChanges());

    }
}
