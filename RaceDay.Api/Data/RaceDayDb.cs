
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDb : DbContext
{
    public RaceDayDb(DbContextOptions<RaceDayDb> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.ToTable(t => t.HasCheckConstraint(
                "CK_Users_Role", "[Role] IN ('Organiser','Participant')"));
        });

        modelBuilder.Entity<RaceEvent>(e =>
        {
            e.ToTable("Events");
            e.HasKey(x => x.EventId);
            e.Property(x => x.EventName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.EventDate).HasColumnType("date");
            e.Property(x => x.StartTime).HasColumnType("time");
            e.Property(x => x.Location).HasMaxLength(200).IsRequired();
            e.Property(x => x.DistanceKm).HasColumnType("decimal(6,2)");
            e.Property(x => x.EventType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20)
                .HasDefaultValue("Planned").IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(x => x.Organiser)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.OrganiserId)
                .OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Events_Distance", "[DistanceKm] > 0");
                t.HasCheckConstraint("CK_Events_EventType",
                    "[EventType] IN ('Run','Walk','Cycle')");
                t.HasCheckConstraint("CK_Events_Status",
                    "[Status] IN ('Planned','Open','Closed','Completed')");
            });
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.HasIndex(x => x.CategoryName).IsUnique();
            e.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<EventCategory>(e =>
        {
            e.HasKey(x => x.EventCategoryId);
            e.HasIndex(x => new { x.EventId, x.CategoryId }).IsUnique();
            e.Property(x => x.EntryFee).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Event).WithMany(x => x.EventCategories)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.Category).WithMany(x => x.EventCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_EventCategories_EntryFee", "[EntryFee] >= 0");
                t.HasCheckConstraint("CK_EventCategories_MaxParticipants",
                    "[MaximumParticipants] > 0");
            });
        });

        modelBuilder.Entity<Enrolment>(e =>
        {
            e.HasKey(x => x.EnrolmentId);
            e.HasIndex(x => new { x.ParticipantId, x.EventCategoryId }).IsUnique();
            e.Property(x => x.EnrolmentDate).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(x => x.Status).HasMaxLength(20)
                .HasDefaultValue("Registered");
            e.HasOne(x => x.Participant).WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.EventCategory).WithMany(x => x.Enrolments)
                .HasForeignKey(x => x.EventCategoryId)
                .OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status",
                "[Status] IN ('Registered','Cancelled','Completed')"));
        });

        modelBuilder.Entity<RaceResult>(e =>
        {
            e.ToTable("Results");
            e.HasKey(x => x.ResultId);
            e.HasIndex(x => x.EnrolmentId).IsUnique();
            e.Property(x => x.FinishTime).HasColumnType("time");
            e.Property(x => x.RecordedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result)
                .HasForeignKey<RaceResult>(x => x.EnrolmentId)
                .OnDelete(DeleteBehavior.NoAction);
            e.ToTable(t => t.HasCheckConstraint(
                "CK_Results_Position", "[Position] > 0"));
        });
    }
}
