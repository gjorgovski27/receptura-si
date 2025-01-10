using CookingAssistantAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CookingAssistantAPI.Data
{
    public class CookingAssistantDbContext : DbContext
    {
        public CookingAssistantDbContext(DbContextOptions<CookingAssistantDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }  // Add Comments DbSet
        public DbSet<Favourite> Favourites { get; set; }  // Add Favourites DbSet
        public DbSet<News> News { get; set; } // Add News DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints for Recipe
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()  // No navigation property from User to Recipes (unidirectional relationship)
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade); // If user is deleted, recipes are also deleted

            // Configure relationships and constraints for Rating
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Recipe)
                .WithMany(rp => rp.Ratings)
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);  // Ratings are deleted if the associated Recipe is deleted

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Ratings are deleted if the associated User is deleted

            // Ensure unique combination of UserId and RecipeId for ratings (each user can rate each recipe only once)
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.UserId, r.RecipeId })
                .IsUnique();

            // Optionality constraints for Ratings
            modelBuilder.Entity<Recipe>()
                .HasMany(r => r.Ratings)
                .WithOne(rt => rt.Recipe)
                .HasForeignKey(rt => rt.RecipeId)
                .IsRequired(false);  // Ratings are optional for recipes

            modelBuilder.Entity<User>()
                .HasMany(u => u.Ratings)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .IsRequired(false);  // Ratings are optional for users

            // Configure relationships and constraints for Comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany() // No navigation property from User to Comments (unidirectional)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Comments are deleted if the associated User is deleted

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Recipe)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);  // Comments are deleted if the associated Recipe is deleted

            // Optionality constraints for Comments
            modelBuilder.Entity<Recipe>()
                .HasMany(r => r.Comments)
                .WithOne(c => c.Recipe)
                .HasForeignKey(c => c.RecipeId)
                .IsRequired(false);  // Comments are optional for recipes

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired(false);  // Comments are optional for users

            // Configure relationships and constraints for Favourites
            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.User)
                .WithMany() // No navigation property from User to Favourites (unidirectional)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Favourites are deleted if the associated User is deleted

            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.Recipe)
                .WithMany() // No navigation property from Recipe to Favourites (unidirectional)
                .HasForeignKey(f => f.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);  // Favourites are deleted if the associated Recipe is deleted

            // Ensure unique combination of UserId and RecipeId for favourites (each user can favourite a recipe only once)
            modelBuilder.Entity<Favourite>()
                .HasIndex(f => new { f.UserId, f.RecipeId })
                .IsUnique();

            // Configure News entity
            modelBuilder.Entity<News>()
                .Property(n => n.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default value for timestamp
        }
    }
}
