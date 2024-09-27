using Metro.Homework.Contexts;
using Metro.Homework.Models;
using Microsoft.AspNetCore.Mvc;

namespace Metro.Homework.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ApplicationDbContext context,
            ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

		[HttpPost]
		public async Task<IActionResult> CreateTransaction(Transaction transaction)
		{
			if (transaction.TransactionArticles == null || !transaction.TransactionArticles.Any())
			{
				return BadRequest("Transaction must include at least one article.");
			}

			if (await _context.Transactions.FindAsync(transaction.TransactionId) != null)
			{
				return BadRequest($"TransactionId: {transaction.TransactionId} already exists");
			}

			if (await _context.Customers.FindAsync(transaction.CustomerId) == null)
			{
				return NotFound($"Customer {transaction.CustomerId} not found");
			}

			using var transactionDb = await _context.Database.BeginTransactionAsync();

			try
			{
				foreach (var ta in transaction.TransactionArticles)
				{
					var article = await _context.Articles.FindAsync(ta.ArticleId);
					if (article == null)
					{
						return NotFound($"Article {ta.ArticleId} not found.");
					}

					if (article.Inventory - ta.ArticleCount < 0)
					{
						return BadRequest($"Not enough article {ta.ArticleId}.");
					}

					article.Inventory -= ta.ArticleCount;
				}

				_context.Transactions.Add(transaction);
				
				await _context.SaveChangesAsync();
				await transactionDb.CommitAsync();
				return CreatedAtAction(nameof(CreateTransaction), new { id = transaction.TransactionId }, transaction);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());

				await transactionDb.RollbackAsync();
				return StatusCode(StatusCodes.Status500InternalServerError, ex);
			}
		}
	}
}
