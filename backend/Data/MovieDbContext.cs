using Microsoft.EntityFrameworkCore;
using Live_Movies.Models;

namespace Live_Movies.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {

        }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<CastMember> CastMembers { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<MovieCast> MovieCasts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Configure many-to-many relationship for Movie-Genre
            modelBuilder.Entity<MovieGenre>()
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId);

            modelBuilder.Entity<MovieGenre>()
                .HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId);

            //Configure Many-to-Many relationship for Movie-Cast

            modelBuilder.Entity<MovieCast>()
                .HasKey(mc => new { mc.MovieId, mc.CastMemberId });

            modelBuilder.Entity<MovieCast>()
                .HasOne(mc=> mc.Movie)
                .WithMany(m=> m.MovieCasts)
                .HasForeignKey(mc=> mc.MovieId);

            modelBuilder.Entity<MovieCast>()
                .HasOne(mc => mc.CastMember)
                .WithMany(c => c.MovieCasts)
                .HasForeignKey(mc => mc.CastMemberId);

            //Configure Unique Constraints

            modelBuilder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<CastMember>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Seed initial data
            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Action" },
                new Genre { Id = 2, Name = "Comedy" },
                new Genre { Id = 3, Name = "Drama" },
                new Genre { Id = 4, Name = "Thriller" },
                new Genre { Id = 5, Name = "Horror" },
                new Genre { Id = 6, Name = "Romance" },
                new Genre { Id = 7, Name = "Crime" },
                new Genre { Id = 8, Name = "Sci-Fi" },
                new Genre { Id = 9, Name = "Sports" },
                new Genre { Id = 10, Name = "Musical" },
                new Genre { Id = 11, Name = "Family" }
            );

            //Seed initial admin data

            modelBuilder.Entity<User>().HasData(
                new User {
                    Id = 1,
                    Email = "admin@movieapp.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    IsActive = true,
                    //CreatedAt = DateTime.UtcNow
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

        }
    }
}
