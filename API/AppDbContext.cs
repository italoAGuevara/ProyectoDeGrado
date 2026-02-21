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
        public DbSet<Origen> Origenes => Set<Origen>();
        public DbSet<RelationJobsAndScript> relationJobsAndScripts => Set<RelationJobsAndScript>();
        public DbSet<ScriptConfiguration> ScriptConfigurations => Set<ScriptConfiguration>();
        public DbSet<StorageProvider> StorageProviders => Set<StorageProvider>();
        public DbSet<UserStorages> UserStorages => Set<UserStorages>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BackupJob>(entity =>
            {
                entity.HasOne(b => b.Origen)
                      .WithMany()
                      .HasForeignKey(b => b.OrigenId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RelationJobsAndScript>(entity =>
            {
                entity.HasKey(rj => new { rj.ScriptId, rj.JobId });

                entity.HasOne(rj => rj.BackupJob)
                      .WithMany(b => b.Scripts)
                      .HasForeignKey(rj => rj.JobId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rj => rj.Script)
                      .WithMany(s => s.Jobs)
                      .HasForeignKey(rj => rj.ScriptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=mibase.db");
            // No lanzar excepción si el modelo tiene cambios pendientes (hay que añadir una migración).
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }
    }
}
