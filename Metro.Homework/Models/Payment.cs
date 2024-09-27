using Metro.Homework.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Metro.Homework.Models
{
	public class Payment
	{
		[JsonIgnore]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long PaymentId { get; set; }
		public long TransactionId { get; set; }
		public decimal Amount { get; set; }
		public PaymentStatus Status { get; set; }
	}
}
