using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SharedComponents.PostgreSQL;

public partial class EdubaiContext : DbContext
{
    public EdubaiContext()
    {
    }

    public EdubaiContext(DbContextOptions<EdubaiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserCredential> UserCredentials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(ConnectionString.ConnectionStringValue);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("user_credentials_pkey");

            entity.ToTable("user_credentials", tb => tb.HasComment("Table containing user data"));

            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.EmailIsVerified)
                .HasComment("true if email verification process was succesful")
                .HasColumnName("email_is_verified");
            entity.Property(e => e.EmailVerificationToken)
                .HasComment("token to be sent via email in case of verification or password reset")
                .HasColumnName("email_verification_token");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PasswordResetToken)
                .HasComment("temporary token for password reset request")
                .HasColumnName("password_reset_token");
            entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");
            entity.Property(e => e.Role).HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
