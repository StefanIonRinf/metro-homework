using Metro.Homework.Models;
using Microsoft.EntityFrameworkCore;

namespace Metro.Homework.Contexts
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Customer> Customers { get; set; } = default!;
	    public DbSet<Transaction> Transactions { get; set; } = default!;
		public DbSet<Article> Articles { get; set; } = default!;
		public DbSet<Payment> Payments { get; set; } = default!;
		public DbSet<TransactionArticle> TransactionArticles { get; set; } = default!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Payment>()
				.Property(p => p.Status)
				.HasConversion<string>();

			modelBuilder.Entity<TransactionArticle>()
			.HasKey(ta => new { ta.TransactionId, ta.ArticleId });

			//modelBuilder.Entity<TransactionArticle>()
			//	.HasOne(ta => ta.Transaction)
			//	.WithMany(t => t.TransactionArticles)
			//	.HasForeignKey(ta => ta.TransactionId);

			//modelBuilder.Entity<TransactionArticle>()
			//	.HasOne(ta => ta.Article)
			//	.WithMany(a => a.TransactionArticles)
			//	.HasForeignKey(ta => ta.ArticleId);

			base.OnModelCreating(modelBuilder);
		}
	}
}
