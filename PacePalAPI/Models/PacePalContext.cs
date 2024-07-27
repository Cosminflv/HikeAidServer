using Microsoft.EntityFrameworkCore;

namespace PacePalAPI.Models
{
    public class PacePalContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }

        public PacePalContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
