using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, Role, long>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed roles
        builder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", ConcurrencyStamp = "4bca9d29-f413-471f-85bd-56af2106f6e8" },
            new Role { Id = 2, Name = "Staff", ConcurrencyStamp = "4bca9d29-f413-471f-85bd-56af2106f6e9" },
            new Role { Id = 3, Name = "Customer", ConcurrencyStamp = "4bca9d29-f413-471f-85bd-56af2106f6e0" }
        );
    }

    public DbSet<Staff> Staff { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesItem> SalesItems { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<PartRequest> PartRequests { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }

}