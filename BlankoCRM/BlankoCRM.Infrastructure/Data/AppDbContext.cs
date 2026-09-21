using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlankoCRM.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    private readonly IOrganizationContext _organizationContext;

    /// <summary>
    /// ID текущей организации для глобального фильтра.
    /// Читается из IOrganizationContext для каждого экземпляра DbContext.
    /// </summary>
    public Guid? CurrentOrganizationId => _organizationContext.GetCurrentOrganizationId();
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IOrganizationContext organizationContext) : base(options)
    {
        _organizationContext = organizationContext;
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientContact> ClientContacts { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentService> AppointmentServices { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Salon> Salons { get; set; }
    public DbSet<MasterProfile> MasterProfiles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dbContextConstant = Expression.Constant(this);
        var currentOrgIdProperty = Expression.Property(
            dbContextConstant,
            nameof(CurrentOrganizationId));
        // Глобальный фильтр для всех сущностей с OrganizationId.
        // Фильтр применяется ТОЛЬКО если OrganizationContext.CurrentOrganizationId не null.
        // Это защищает неавторизованные запросы (регистрация, логин) от некорректной фильтрации.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(IOrganizationEntity).IsAssignableFrom(e.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var organizationIdProperty = Expression.Property(parameter, nameof(IOrganizationEntity.OrganizationId));

            var hasValue = Expression.Property(currentOrgIdProperty, nameof(Nullable<Guid>.HasValue));
            var value = Expression.Property(currentOrgIdProperty, nameof(Nullable<Guid>.Value));
            var comparison = Expression.Equal(organizationIdProperty, value);
            var andAlso = Expression.AndAlso(hasValue, comparison);

            var lambda = Expression.Lambda(andAlso, parameter);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        // Настройка таблиц, связей, индексов
        ConfigureOrganization(modelBuilder);
        ConfigureClient(modelBuilder);
        ConfigureClientContact(modelBuilder);
        ConfigureService(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureAppointmentService(modelBuilder);
        ConfigureSchedule(modelBuilder);
        ConfigureIdentityEntities(modelBuilder);
    }

    private void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
        });
    }

    private void ConfigureClient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(20);

            // Индексы для быстрого поиска
            entity.HasIndex(c => c.OrganizationId);
            entity.HasIndex(c => c.Phone);
            // Составной уникальный индекс: телефон должен быть уникален в пределах одного мастера
            entity.HasIndex(c => new { c.OrganizationId, c.Phone })
                .IsUnique()
                .HasFilter("\"Phone\" IS NOT NULL");

            // Связь с Organization
            entity.HasOne(c => c.Organization)
                .WithMany(t => t.Clients)
                .HasForeignKey(c => c.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении мастера удаляются его клиенты
        });
    }

    private void ConfigureClientContact(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientContact>(entity =>
        {
            entity.HasKey(contact => contact.Id);
            entity.Property(contact => contact.Value).IsRequired().HasMaxLength(200);
            entity.Property(contact => contact.Type).HasConversion<int>();

            entity.HasIndex(contact => contact.OrganizationId);
            entity.HasIndex(contact => new { contact.ClientId, contact.Type }).IsUnique();

            entity.HasOne(contact => contact.Client)
                .WithMany(client => client.Contacts)
                .HasForeignKey(contact => contact.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(contact => contact.Organization)
                .WithMany()
                .HasForeignKey(contact => contact.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Price).HasColumnType("decimal(18,2)");

            entity.HasIndex(s => s.OrganizationId);

            // Название услуги должно быть уникально в пределах одного мастера
            entity.HasIndex(s => new { s.OrganizationId, s.Name }).IsUnique();

            entity.HasOne(s => s.Organization)
                .WithMany(t => t.Services)
                .HasForeignKey(s => s.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Notes).HasMaxLength(500);
            entity.Property(a => a.Status)
                .HasConversion<int>(); // enum сохраняется как int

            // Индексы для частых запросов
            entity.HasIndex(a => a.OrganizationId);
            entity.HasIndex(a => a.StartTime);
            entity.HasIndex(a => new { a.OrganizationId, a.StartTime }); // для календаря

            // Связь с Organization
            entity.HasOne(a => a.Organization)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Client
            entity.HasOne(a => a.Client)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настраиваем связь многие-ко-многим через AppointmentService
            entity.HasMany(a => a.AppointmentServices)
                .WithOne(aps => aps.Appointment)
                .HasForeignKey(aps => aps.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ограничение: EndTime >= StartTime
            entity.HasCheckConstraint("CK_Appointment_EndTime", "\"EndTime\" >= \"StartTime\"");
        });

    }

    private void ConfigureAppointmentService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.HasKey(aps => aps.Id);

            entity.Property(aps => aps.Price).HasColumnType("decimal(18,2)");
            entity.Property(aps => aps.ServiceName).HasMaxLength(100);
            entity.Property(aps => aps.Quantity).HasDefaultValue(1);

            // Индексы
            entity.HasIndex(aps => aps.OrganizationId); // обязательно для мультитенантности
            entity.HasIndex(aps => aps.AppointmentId);
            entity.HasIndex(aps => aps.ServiceId);

            // Связь с Appointment (уже есть через HasMany, но здесь можно уточнить)
            entity.HasOne(aps => aps.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(aps => aps.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Service
            entity.HasOne(aps => aps.Service)
                .WithMany(s => s.AppointmentServices)
                .HasForeignKey(aps => aps.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь с Organization
            entity.HasOne(aps => aps.Organization)
                .WithMany() // если в Organization не нужно собирать все AppointmentServices
                .HasForeignKey(aps => aps.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(schedule => schedule.Id);

            entity.HasIndex(schedule => schedule.OrganizationId);
            entity.HasIndex(schedule => new { schedule.OrganizationId, schedule.SalonId, schedule.StartTime });
            entity.HasIndex(schedule => schedule.AppointmentId).IsUnique();

            entity.HasCheckConstraint("CK_Schedule_EndTime", "\"EndTime\" > \"StartTime\"");

            entity.HasOne(schedule => schedule.Organization)
                .WithMany()
                .HasForeignKey(schedule => schedule.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(schedule => schedule.Salon)
                .WithMany()
                .HasForeignKey(schedule => schedule.SalonId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(schedule => schedule.Appointment)
                .WithOne(appointment => appointment.Schedule)
                .HasForeignKey<Schedule>(schedule => schedule.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIdentityEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(user => user.VkUserId)
            .IsUnique()
            .HasFilter("\"VkUserId\" IS NOT NULL");
            entity.Property(user => user.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(user => user.Organization)
                .WithMany(organization => organization.Users)
                .HasForeignKey(user => user.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(user => user.Salon)
                .WithMany()
                .HasForeignKey(user => user.SalonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MasterProfile>(entity =>
        {
            entity.HasKey(profile => profile.Id);
            entity.HasOne(profile => profile.User)
                .WithOne(user => user.MasterProfile)
                .HasForeignKey<MasterProfile>(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(profile => profile.Organization)
                .WithMany(organization => organization.MasterProfiles)
                .HasForeignKey(profile => profile.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(profile => profile.Salon)
                .WithMany(salon => salon.MasterProfiles)
                .HasForeignKey(profile => profile.SalonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Salon>(entity =>
        {
            entity.HasKey(salon => salon.Id);
            entity.Property(salon => salon.Name).IsRequired().HasMaxLength(150);
            entity.HasOne(salon => salon.Organization)
                .WithMany(organization => organization.Salons)
                .HasForeignKey(salon => salon.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

