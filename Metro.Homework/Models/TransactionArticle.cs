using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Metro.Homework.Models
{
	public class TransactionArticle
	{
		[JsonIgnore]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long TransactionArticleId { get; set; }
		[JsonIgnore]
		public long TransactionId { get; set; }

		public long ArticleId { get; set; }

		public int ArticleCount { get; set; }
	}
}
