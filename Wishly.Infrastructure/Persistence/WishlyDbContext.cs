using Microsoft.EntityFrameworkCore;
using Wishly.Domain.Entities;

namespace Wishly.Infrastructure.Persistence;

public class WishlyDbContext : DbContext
{
    public WishlyDbContext(DbContextOptions<WishlyDbContext> options) : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<EtapeOrganisation> EtapesOrganisation => Set<EtapeOrganisation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<InvitationCard> InvitationCards => Set<InvitationCard>();

    public DbSet<ConnexionToken> ConnexionTokens => Set<ConnexionToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ConnexionToken>()
    .Property(t => t.UserId)
    .UseCollation("utf8mb4_unicode_ci");
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Wishlist>()
            .HasIndex(w => w.LienPublicUnique)
            .IsUnique();

        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany(u => u.Wishlists)
            .HasForeignKey(w => w.UserId);

        modelBuilder.Entity<Item>()
            .HasOne(i => i.Wishlist)
            .WithMany(w => w.Items)
            .HasForeignKey(i => i.WishlistId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Item)
            .WithOne(i => i.Reservation)
            .HasForeignKey<Reservation>(r => r.ItemId);

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.ItemId)
            .IsUnique();

        modelBuilder.Entity<InvitationCard>()
            .HasOne(ic => ic.Wishlist)
            .WithOne(w => w.InvitationCard)
            .HasForeignKey<InvitationCard>(ic => ic.WishlistId);

        modelBuilder.Entity<Organisation>(e =>
        {
            e.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);

            // Même collation que Users.Id, sinon MySQL refuse la clé étrangère
            e.Property(o => o.UserId).UseCollation("utf8mb4_unicode_ci");
        });

        modelBuilder.Entity<EtapeOrganisation>()
            .HasOne(e => e.Organisation)
            .WithMany(o => o.Etapes)
            .HasForeignKey(e => e.OrganisationId);
    }
}