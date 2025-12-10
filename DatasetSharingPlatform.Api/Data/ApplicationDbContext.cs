using DatasetSharingPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatasetSharingPlatform.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DownloadPermissionRequest> DownloadPermissionRequests { get; set; }
        public DbSet<DatasetPermission> DatasetPermissions { get; set; }
        public DbSet<DatasetViewRecord> DatasetViewRecords { get; set; }
        public DbSet<DatasetDownloadRecord> DatasetDownloadRecords { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<DatasetTag> DatasetTags { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置类的字段，确保数据库表结构正确
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()"); // 设置 CreatedAt 字段的默认值为当前时间

            modelBuilder.Entity<DatasetPermission>()
                .HasOne(dp => dp.Dataset)
                .WithMany(d => d.Permissions)
                .HasForeignKey(dp => dp.DatasetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DatasetPermission>()
                .HasOne(dp => dp.User)
                .WithMany(u => u.DatasetPermissions)
                .HasForeignKey(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DatasetDownloadRecord>()
                .HasOne(d => d.User) 
                .WithMany(u => u.DatasetDownloadRecords) 
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DatasetDownloadRecord>()
                .HasOne(d => d.Dataset) 
                .WithMany(ds => ds.DatasetDownloadRecords)
                .HasForeignKey(d => d.DatasetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Dataset)
                .WithMany(d => d.Comments)
                .HasForeignKey(c => c.DatasetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // DatasetTag: 配置复合主键
            modelBuilder.Entity<DatasetTag>()
                .HasKey(dt => new { dt.DatasetId, dt.TagId });

            modelBuilder.Entity<DatasetTag>()
                .HasOne(dt => dt.Dataset)
                .WithMany(d => d.DatasetTags)
                .HasForeignKey(dt => dt.DatasetId);

            modelBuilder.Entity<DatasetTag>()
                .HasOne(dt => dt.Tag)
                .WithMany(t => t.DatasetTags)
                .HasForeignKey(dt => dt.TagId);

            // Tag: 配置自引用层次结构
            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict); 
        }

    }
}
