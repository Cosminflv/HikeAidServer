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
        public DbSet<TrackModel> RecordedTracks { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Coordinates> Coordinates { get; set; }

        public PacePalContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //// User and Friendships
            //modelBuilder.Entity<FriendshipModel>()
            //    .HasOne(f => f.Requester)
            //    .WithMany(u => u.Friendships)
            //    .HasForeignKey(f => f.RequesterId)
            //    .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

            //modelBuilder.Entity<FriendshipModel>()
            //    .HasOne(f => f.Receiver)
            //    .WithMany()
            //    .HasForeignKey(f => f.ReceiverId)
            //    .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

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

            // User and Tracks
            modelBuilder.Entity<TrackModel>()
                .HasOne(u => u.UserModel)
                .WithMany(t => t.RecordedTracks)
                .HasForeignKey(track => track.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cascade delete

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

            // Alert and Coordinates (One-to-One)
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.LocationCoords)
                .WithOne()
                .HasForeignKey<Alert>(a => a.CoordinatesId)
                .OnDelete(DeleteBehavior.Cascade); // If an alert is deleted, remove the coordinates

            // Many-to-Many relationship between Alerts and Confirmed Users
            modelBuilder.Entity<Alert>()
                .HasMany(a => a.ConfirmedUsers)
                .WithMany(u => u.ConfirmedAlerts)
                .UsingEntity(j => j.ToTable("AlertConfirmedUsers"));
        }
    }
}
