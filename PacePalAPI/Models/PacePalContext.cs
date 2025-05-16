using Microsoft.EntityFrameworkCore;

namespace PacePalAPI.Models
{
    public class PacePalContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<FriendshipModel> Friendships { get; set; }
        public DbSet<SocialPostModel> SocialPosts { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<LikeModel> Likes { get; set; }
        public DbSet<TourModel> Tours { get; set; }
        public DbSet<TourCoordinates> TourCoordinates { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<ConfirmedCurrentHike> ConfirmedCurrentHikes { get; set; }

        public PacePalContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FriendshipModel>()
                .HasOne(f => f.Requester)
                .WithMany(u => u.SentFriendships) // List of friendships where user is the requester
                .HasForeignKey(f => f.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendshipModel>()
                .HasOne(f => f.Receiver)
                .WithMany(u => u.ReceivedFriendships) // List of friendships where user is the receiver
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // User and Tours
            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.HasMany(u => u.RecordedTracks)
                      .WithOne(t => t.Author)
                      .HasForeignKey(t => t.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TourModel>(entity =>
            {
                entity.HasMany(t => t.Coordinates)
                      .WithOne(c => c.Tour)
                      .HasForeignKey(c => c.TourId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TourCoordinates>(entity =>
            {
                entity.Property(c => c.Latitude).IsRequired();
                entity.Property(c => c.Longitude).IsRequired();
            });

            // User and Posts
            modelBuilder.Entity<SocialPostModel>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            // User and Comments
            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            // User and Likes
            modelBuilder.Entity<LikeModel>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            // Post and Comments
            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            // Post and Likes
            modelBuilder.Entity<LikeModel>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            // User and Alerts (One-to-Many)
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Author)
                .WithMany(u => u.CreatedAlerts) // A user can create multiple alerts
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            // Many-to-Many relationship between Alerts and Confirmed Users
            modelBuilder.Entity<Alert>()
                .HasMany(a => a.ConfirmedUsers)
                .WithMany(u => u.ConfirmedAlerts)
                .UsingEntity(j => j.ToTable("AlertConfirmedUsers"));

            // User ↔ ConfirmedCurrentHike
            modelBuilder.Entity<ConfirmedCurrentHike>()
                .HasOne(c => c.User)
                .WithMany(u => u.ConfirmedCurrentHikes)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ConfirmedCurrentHike ↔ TrackCoordinates
            modelBuilder.Entity<ConfirmedCurrentHike>()
                .HasMany(c => c.TrackCoordinates)
                .WithOne(c => c.TrackCoordinatesConfirmedCurrentHike)
                .HasForeignKey(c => c.TrackCoordinatesConfirmedCurrentHikeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ConfirmedCurrentHike ↔ UserProgressCoordinates
            modelBuilder.Entity<ConfirmedCurrentHike>()
                .HasMany(c => c.UserProgressCoordinates)
                .WithOne(c => c.UserProgressCoordinatesConfirmedCurrentHike)
                .HasForeignKey(c => c.UserProgressCoordinatesConfirmedCurrentHikeId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
