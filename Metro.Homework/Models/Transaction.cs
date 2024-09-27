using System.ComponentModel.DataAnnotations.Schema;

namespace Metro.Homework.Models
{
	public class Transaction
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long TransactionId { get; set; }
		public long CustomerId { get; set; }
		public ICollection<TransactionArticle> TransactionArticles { get; set; }
		public ICollection<Payment> Payments { get; set; }
	}
}
