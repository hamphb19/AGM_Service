using AGM_API.Models;
using AGM_API.Models.Animal;
using AGM_API.Models.Audit;
using AGM_API.Models.Crop;
using AGM_API.Models.Farm;
using AGM_API.Models.Farm.FarmMember;
using AGM_API.Models.Field;
using AGM_API.Models.Person;
using AGM_API.Models.Season;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace AGM_API.Database
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public AppDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }


        #region Farm
        public DbSet<Farm> Farms { get; set; }
        public DbSet<FarmType> FarmTypes { get; set; }
        public DbSet<FarmMember> FarmMembers { get; set; }
        public DbSet<MemberRole> MemberRoles { get; set; }
        public DbSet<FarmAnimal> FarmAnimals { get; set; }

        #endregion


        #region Crop

        public DbSet<Crop> Crops { get; set; }
        public DbSet<CropType> CropTypes { get; set; }

        #endregion

        #region

        public DbSet<Season> Seasons { get; set; }
        public DbSet<SeasonField> SeasonFields { get; set; }

        #endregion

        #region Task

        public DbSet<Models.Task.Task> Tasks { get; set; }
        public DbSet<Models.Task.TaskField> TaskFields { get; set; }

        #endregion

        #region Machine

        public DbSet<Models.Machine.Machine> Machines { get; set; }
        public DbSet<Models.Machine.MachineType> MachineTypes { get; set; }
        public DbSet<Models.Machine.MachineBrand> MachineBrands { get; set; }
        public DbSet<Models.Machine.MachineModel> MachineModels { get; set; }
        public DbSet<Models.Machine.MachineService> MachineServices { get; set; }

        #endregion

        #region Animal

        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalType> AnimalTypes { get; set; }
        public DbSet<StallType> StallTypes { get; set; }
        public DbSet<Stall> Stalls { get; set; }
        public DbSet<StallAnimal> StallAnimals { get; set; }
        public DbSet<StallEvent> StallEvents { get; set; }

        #endregion

        #region Person

        public DbSet<Person> Persons { get; set; }

        #endregion

        #region Security

        public DbSet<User> Users { get; set; }

        #endregion

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        #region Field

        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldKeyPoint> FieldKeyPoints { get; set; }
        public DbSet<FieldAction> FieldActions { get; set; }
        public DbSet<FieldActionType> FieldActionTypes { get; set; }
        public DbSet<FieldActionPhoto> FieldActionPhotos { get; set; }
        public DbSet<FieldActionMachine> FieldActionMachines { get; set; }

        #endregion

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries<Auditable>();
            var currentUser = GetCurrentUser();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreateDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.ChangeDate = DateTime.UtcNow;
                    entry.Entity.ChangeBy = currentUser;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ChangeDate = DateTime.UtcNow;
                    entry.Entity.ChangeBy = currentUser;
                }
            }
        }

        private User? GetCurrentUser()
        {
            var userId = _httpContextAccessor?.HttpContext?.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !long.TryParse(userId, out var id))
                return null;

            return Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Auditable).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(Auditable))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasOne(typeof(User), "CreatedBy")
                        .WithMany()
                        .OnDelete(DeleteBehavior.Restrict);

                    modelBuilder.Entity(entityType.ClrType)
                        .HasOne(typeof(User), "ChangeBy")
                        .WithMany()
                        .OnDelete(DeleteBehavior.Restrict);
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clr = entityType.ClrType;
                var geometryProps = clr
                    .GetProperties()
                    .Where(p => typeof(Geometry).IsAssignableFrom(p.PropertyType));

                foreach (var p in geometryProps)
                {
                    modelBuilder.Entity(clr)
                        .Property(p.Name)
                        .HasColumnType("geometry");
                }
            }

            modelBuilder.Entity<Person>()
                .HasOne(u => u.User)
                .WithOne(p => p.Person)
                .HasForeignKey<Person>(u => u.UserId);

            modelBuilder.Entity<Field>(e =>
            {
                e.HasOne(x => x.Farm)
                 .WithMany()
                 .HasForeignKey(x => x.FarmId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FieldAction>(e =>
            {
                e.HasOne(x => x.Season)
                 .WithMany()
                 .HasForeignKey(x => x.SeasonId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ActivityLog>()
                .HasIndex(a => new { a.FarmId, a.Timestamp });

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserCode)
                .IsUnique()
                .HasFilter("\"UserCode\" IS NOT NULL");

            modelBuilder.Entity<Models.Farm.Farm>(e =>
            {
                e.HasOne(x => x.FarmType)
                 .WithMany()
                 .HasForeignKey(x => x.FarmTypeId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Models.Machine.MachineModel>(e =>
            {
                e.HasOne(x => x.MachineBrand)
                 .WithMany()
                 .HasForeignKey(x => x.MachineBrandId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Models.Field.FieldActionMachine>(e =>
            {
                e.HasKey(x => new { x.FieldActionId, x.MachineId });
                e.HasOne(x => x.FieldAction)
                 .WithMany(x => x.Machines)
                 .HasForeignKey(x => x.FieldActionId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Machine)
                 .WithMany()
                 .HasForeignKey(x => x.MachineId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SeasonField>(e =>
            {
                e.HasKey(x => new { x.season_Id, x.field_Id });

                e.HasOne(x => x.Season)
                    .WithMany(x => x.SeasonFields)
                    .HasForeignKey(x => x.season_Id)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Field)
                    .WithMany()
                    .HasForeignKey(x => x.field_Id)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<FarmMember>(e =>
            {
                e.HasKey(x => new { x.farm_Id, x.person_Id, x.role_Id });

                // Beziehungen + FKs
                e.HasOne(x => x.Farm)
                 .WithMany(f => f.farmMembers)
                 .HasForeignKey(x => x.farm_Id)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Person)
                 .WithMany(p => p.MemberOfFarms)
                 .HasForeignKey(x => x.person_Id)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Role)
                 .WithMany(r => r.memberRoles)
                 .HasForeignKey(x => x.role_Id)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FarmAnimal>(e =>
            {
                e.HasKey(x => new { x.farm_Id, x.animal_Id });

                e.HasOne(x => x.Farm)
                 .WithMany(f => f.farmAnimals)
                 .HasForeignKey(x => x.farm_Id)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Animal)
                 .WithMany()
                 .HasForeignKey(x => x.animal_Id)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Models.Task.Task>(e =>
            {
                e.HasOne(x => x.Farm)
                 .WithMany()
                 .HasForeignKey(x => x.FarmId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.Task.TaskField>(e =>
            {
                e.HasKey(x => new { x.TaskId, x.FieldId });
                e.HasOne(x => x.Task)
                 .WithMany(t => t.Fields)
                 .HasForeignKey(x => x.TaskId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Field)
                 .WithMany()
                 .HasForeignKey(x => x.FieldId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
