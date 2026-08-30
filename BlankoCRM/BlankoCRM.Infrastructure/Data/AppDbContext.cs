using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlankoCRM.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentService> AppointmentServices { get; set; }
    public DbSet<Schedule> Schedules { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Глобальный фильтр для всех сущностей с TenantId.
        // Фильтр применяется ТОЛЬКО если TenantContext.CurrentTenantId не null.
        // Это защищает неавторизованные запросы (регистрация, логин) от некорректной фильтрации.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(IMultiTenant).IsAssignableFrom(e.ClrType)))
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProperty = Expression.Property(parameter, nameof(IMultiTenant.TenantId));

            // СТАТИЧЕСКОЕ СВОЙСТВО — без захвата экземпляра
            var currentTenantId = Expression.Property(null, typeof(TenantContext), nameof(TenantContext.CurrentTenantId));

            var hasValue = Expression.Property(currentTenantId, "HasValue");
            var value = Expression.Property(currentTenantId, "Value");
            var comparison = Expression.Equal(tenantIdProperty, value);
            var andAlso = Expression.AndAlso(hasValue, comparison);

            var lambda = Expression.Lambda(andAlso, parameter);
            //modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            modelBuilder.Entity<Client>().HasQueryFilter(e => e.TenantId == TenantContext.CurrentTenantId);
        }

        // Настройка таблиц, связей, индексов
        ConfigureTenant(modelBuilder);
        ConfigureClient(modelBuilder);
        ConfigureService(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureAppointmentService(modelBuilder);
    }

    private void ConfigureTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Email).IsUnique(); // Email уникален
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Email).IsRequired().HasMaxLength(100);
            entity.Property(t => t.PasswordHash).IsRequired();

            // Индексы для часто используемых полей
            entity.HasIndex(t => t.TelegramChatId);
        });
    }

    private void ConfigureClient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Telegram).HasMaxLength(100);

            // Индексы для быстрого поиска
            entity.HasIndex(c => c.TenantId);
            entity.HasIndex(c => c.Phone);
            entity.HasIndex(c => c.Telegram);

            // Составной уникальный индекс: телефон должен быть уникален в пределах одного мастера
            entity.HasIndex(c => new { c.TenantId, c.Phone }).IsUnique();

            // Связь с Tenant
            entity.HasOne(c => c.Tenant)
                .WithMany(t => t.Clients)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении мастера удаляются его клиенты
        });
    }

    private void ConfigureService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Price).HasColumnType("decimal(18,2)");

            entity.HasIndex(s => s.TenantId);

            // Название услуги должно быть уникально в пределах одного мастера
            entity.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();

            entity.HasOne(s => s.Tenant)
                .WithMany(t => t.Services)
                .HasForeignKey(s => s.TenantId)
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
            entity.HasIndex(a => a.TenantId);
            entity.HasIndex(a => a.StartTime);
            entity.HasIndex(a => new { a.TenantId, a.StartTime }); // для календаря

            // Связь с Tenant
            entity.HasOne(a => a.Tenant)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TenantId)
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
            entity.HasIndex(aps => aps.TenantId); // обязательно для мультитенантности
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

            // Связь с Tenant
            entity.HasOne(aps => aps.Tenant)
                .WithMany() // если в Tenant не нужно собирать все AppointmentServices
                .HasForeignKey(aps => aps.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>(entity =>
            {

            }
        );
    }
}
