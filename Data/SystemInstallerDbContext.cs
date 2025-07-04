using Microsoft.EntityFrameworkCore;

namespace SystemInstaller.Web.Data
{
    public class SystemInstallerDbContext : DbContext
    {
        public SystemInstallerDbContext(DbContextOptions<SystemInstallerDbContext> options) : base(options) { }

        public DbSet<InstallationEnvironment> Environments { get; set; }
        public DbSet<InstallationTask> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Weitere Konfigurationen, falls nötig
        }
    }
}
