using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Metro.Homework.Models
{
	public class Article
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ArticleId { get; set; }
		public string Title { get; set; }
		public string? Description { get; set; }
		public decimal Price { get; set; }
		public int Inventory { get; set; }
	}
}
