using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Metro.Homework.Models
{
	public class Customer
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long CustomerId { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string Phone { get; set; }
    }
}
