using Microsoft.EntityFrameworkCore;
using Trail365.Entities;

namespace Trail365.Data
{
    public partial class IdentityContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<FederatedIdentity> Identities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();

            modelBuilder.Entity<FederatedIdentity>()
                .HasIndex(b => new { b.Identifier, b.AuthenticationType })
                .IsUnique();

            modelBuilder.Entity<FederatedIdentity>()
               .HasOne(fi => fi.User)
               .WithMany(u => u.Identities)
               .HasForeignKey(fi => fi.UserID)
               .HasConstraintName("FK_FederatedIdentity_User");
        }

        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }
    }
}
