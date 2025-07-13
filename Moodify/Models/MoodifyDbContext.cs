using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Moodify.Models;

public partial class MoodifyDbContext : DbContext
{
    public MoodifyDbContext()
    {
    }

    public MoodifyDbContext(DbContextOptions<MoodifyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<ArtistMusic> ArtistMusics { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Music> Musics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=BLUEWISE;Database=Modify;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.ArtistId).HasName("PK__artist__6CD0400193E1869D");

            entity.ToTable("artist");

            entity.Property(e => e.ArtistId)
                .ValueGeneratedNever()
                .HasColumnName("artist_id");
            entity.Property(e => e.ArtistName)
                .HasMaxLength(255)
                .HasColumnName("artist_name");
        });

        modelBuilder.Entity<ArtistMusic>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("artist_Music");

            entity.Property(e => e.ArtistId).HasColumnName("artist_id");
            entity.Property(e => e.MusicId).HasColumnName("music_id");

            entity.HasOne(d => d.Artist).WithMany()
                .HasForeignKey(d => d.ArtistId)
                .HasConstraintName("FK__artist_Mu__artis__440B1D61");

            entity.HasOne(d => d.Music).WithMany()
                .HasForeignKey(d => d.MusicId)
                .HasConstraintName("FK__artist_Mu__music__44FF419A");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("favorites");

            entity.Property(e => e.Musicid).HasColumnName("musicid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Music).WithMany()
                .HasForeignKey(d => d.Musicid)
                .HasConstraintName("FK__favorites__music__4316F928");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("FK__favorites__useri__4222D4EF");
        });

        modelBuilder.Entity<History>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__history__3213E83FF51100BD");

            entity.ToTable("history");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ListenedAt)
                .HasColumnType("datetime")
                .HasColumnName("listened_at");
            entity.Property(e => e.MusicId).HasColumnName("music_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Music).WithMany(p => p.Histories)
                .HasForeignKey(d => d.MusicId)
                .HasConstraintName("FK__history__music_i__412EB0B6");

            entity.HasOne(d => d.User).WithMany(p => p.Histories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__history__user_id__403A8C7D");
        });

        modelBuilder.Entity<Music>(entity =>
        {
            entity.HasKey(e => e.MusicId).HasName("PK__music__B1C42D0B1BF0B15B");

            entity.ToTable("music");

            entity.Property(e => e.MusicId)
                .ValueGeneratedNever()
                .HasColumnName("music_id");
            entity.Property(e => e.Count).HasColumnName("count");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83FDF92C8CB");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ__user__AB6E61642919079F").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Photo)
                .HasMaxLength(1)
                .HasColumnName("photo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
