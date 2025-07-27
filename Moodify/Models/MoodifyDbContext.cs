using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Moodify.Models;

public partial class MoodifyDbContext : IdentityDbContext<User>
{
	public MoodifyDbContext(DbContextOptions<MoodifyDbContext> options)
		: base(options)
	{
	}

	public virtual DbSet<Artist> Artists { get; set; }
	public virtual DbSet<ArtistMusic> ArtistMusics { get; set; }
	public virtual DbSet<Favorite> Favorites { get; set; }
	public virtual DbSet<History> Histories { get; set; }
	public virtual DbSet<Music> Musics { get; set; }
	public virtual DbSet<FriendReq> FriendReqs { get; set; }
	public virtual DbSet<Friends> Friends { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Artist table
		modelBuilder.Entity<Artist>(entity =>
		{
			entity.ToTable("artist");
			entity.HasKey(e => e.ArtistId);
			entity.Property(e => e.ArtistId).HasColumnName("artist_id").ValueGeneratedNever();
			entity.Property(e => e.ArtistName).HasColumnName("artist_name").HasMaxLength(255);
		});
		modelBuilder.Entity<FriendReq>()
			.HasOne(fr => fr.Sender)
			.WithMany(u => u.SentFriendRequests)
			.HasForeignKey(fr => fr.sendid)
			.OnDelete(DeleteBehavior.Restrict); // Prevent cascade issues if user is deleted

		modelBuilder.Entity<FriendReq>()
			.HasOne(fr => fr.Receiver)
			.WithMany(u => u.ReceivedFriendRequests)
			.HasForeignKey(fr => fr.receiveid)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Friends>()
			.HasOne(f => f.user)
			.WithMany(u => u.Friends)
			.HasForeignKey(f => f.userid);
		// ArtistMusic junction
		modelBuilder.Entity<ArtistMusic>()
			.HasKey(am => new { am.ArtistId, am.MusicId });

		modelBuilder.Entity<ArtistMusic>()
			.HasOne(am => am.Artist)
			.WithMany(a => a.ArtistMusics)
			.HasForeignKey(am => am.ArtistId);

		modelBuilder.Entity<ArtistMusic>()
			.HasOne(am => am.Music)
			.WithMany(m => m.ArtistMusics)
			.HasForeignKey(am => am.MusicId);

		// Favorites
		modelBuilder.Entity<Favorite>(entity =>
		{
			entity.ToTable("favorites");
			entity.HasKey(f => new { f.Userid, f.Musicid });

			entity.Property(f => f.Userid).HasColumnName("userid");
			entity.Property(f => f.Musicid).HasColumnName("musicid");

			entity.HasOne(f => f.User)
				  .WithMany(u => u.Favorite)
				  .HasForeignKey(f => f.Userid)
				  .HasConstraintName("FK_favorites_user");

			entity.HasOne(f => f.Music)
				  .WithMany(m => m.Favorites)
				  .HasForeignKey(f => f.Musicid)
				  .HasConstraintName("FK_favorites_music");
		});

		// History
		modelBuilder.Entity<History>(entity =>
		{
			entity.ToTable("history");
			entity.HasKey(h => h.Id);
			entity.Property(h => h.Id).HasColumnName("id");
			entity.Property(h => h.ListenedAt).HasColumnName("listened_at").HasColumnType("datetime");
			entity.Property(h => h.UserId).HasColumnName("user_id");
			entity.Property(h => h.MusicId).HasColumnName("music_id");

			entity.HasOne(h => h.User)
				  .WithMany(u => u.Histories)
				  .HasForeignKey(h => h.UserId)
				  .HasConstraintName("FK__history__user_id");

			entity.HasOne(h => h.Music)
				  .WithMany(m => m.Histories)
				  .HasForeignKey(h => h.MusicId)
				  .HasConstraintName("FK__history__music_id");
		});

		// Music
		modelBuilder.Entity<Music>(entity =>
		{
			entity.ToTable("music");
			entity.HasKey(m => m.MusicId);
			entity.Property(m => m.MusicId).HasColumnName("music_id").ValueGeneratedNever();
			entity.Property(m => m.Count).HasColumnName("count");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
