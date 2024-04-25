using File_Encryption_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace File_Encryption_backend.Data
{
	public class ApplicationDbContext:DbContext
	{

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		public DbSet<Document> documents { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Document>().ToTable("Document")
				.HasKey(e => e.Id);

		}
	}
}
