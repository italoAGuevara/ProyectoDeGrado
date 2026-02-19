using API.Features.Login.Entities;
using HostedService.Entities;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class AppDbContext : DbContext
    {
        public DbSet<BackupJob> BackupJobs => Set<BackupJob>();
        public DbSet<User> Users => Set<User>();
        public DbSet<FileMetadata> FileMetadatas => Set<FileMetadata>();
        public DbSet<HistoryBackupExecutions> HistoryBackupExecutions => Set<HistoryBackupExecutions>();
        public DbSet<RelationJobsAndScript> relationJobsAndScripts => Set<RelationJobsAndScript>();
        public DbSet<ScriptConfiguration> ScriptConfigurations => Set<ScriptConfiguration>();
        public DbSet<StorageProvider> StorageProviders => Set<StorageProvider>();
        public DbSet<UserStorages> UserStorages => Set<UserStorages>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RelationJobsAndScript>(entity =>
            {
                // Definir Clave Primaria Compuesta
                entity.HasKey(rj => new { rj.ScriptId, rj.JobId });

                // Relación con BackupJob
                entity.HasOne(rj => rj.BackupJob)
                      .WithMany(b => b.Scripts)
                      .HasForeignKey(rj => rj.ScriptId);

                // Relación con ScriptConfiguration
                entity.HasOne(rj => rj.Script)
                      .WithMany(s => s.Jobs)
                      .HasForeignKey(rj => rj.Id);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=mibase.db");
    }
}
