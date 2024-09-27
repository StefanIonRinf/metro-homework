using Metro.Homework.Contexts;
using Metro.Homework.Controllers;
using Metro.Homework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metro.Homework.Tests.Controllers
{
	[TestFixture]
	public class TransactionsControllerTests
	{
		private ApplicationDbContext _context;
		private ILogger<TransactionsController> _logger;
		private TransactionsController _controller;

		[SetUp]
		public void Setup()
		{
			_context = new ApplicationDbContext(GetOptions());
			_logger = Substitute.For<ILogger<TransactionsController>>();

			_controller = new TransactionsController(_context, _logger);
		}

		[TearDown]
		public void TearDown()
		{
			try
			{
				_context.Articles.RemoveRange(_context.Articles);
				_context.Payments.RemoveRange(_context.Payments);
				_context.Transactions.RemoveRange(_context.Transactions);
				_context.Customers.RemoveRange(_context.Customers);
				_context.SaveChanges();

				_context.Dispose();
				_controller.Dispose();
			}
			catch (Exception ex)
			{
				// Will throw error when context is mocked
			}
		}

		[Test]
		public async Task CreateTransaction_ReturnsBadRequest_WhenNullArticles()
		{
			// Arrange
			var transaction = new Transaction { TransactionArticles = null };

			// Act
			var result = await _controller.CreateTransaction(transaction);

			// Assert
			Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
			Assert.That((result as BadRequestObjectResult).Value, Is.EqualTo("Transaction must include at least one article."));
			
		}

		[Test]
		public async Task CreateTransaction_ReturnsBadRequest_WhenNoArticles()
		{
			var transaction = GetTransaction(transactionId: 1, transactionArticles: GetTransactionArticles());

			var result = await _controller.CreateTransaction(transaction);

			Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
			Assert.That((result as BadRequestObjectResult).Value, Is.EqualTo("Transaction must include at least one article."));

		}

		[Test]
		public async Task CreateTransaction_ReturnsBadRequest_WhenTransactionIdExists()
		{
			var transactionArticles = GetTransactionArticles();
			transactionArticles.Add(new TransactionArticle() { ArticleId = 1, ArticleCount = 1 });

			var transaction = GetTransaction(1, 1, transactionArticles: transactionArticles);

			_context.Transactions.Add(transaction);
			_context.SaveChanges();
			
			var result = await _controller.CreateTransaction(transaction);

			Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
			Assert.That((result as BadRequestObjectResult).Value, Is.EqualTo($"TransactionId: {transaction.TransactionId} already exists"));
		}

		[Test]
		public async Task CreateTransaction_ReturnsNotFound_WhenCustomerDoesNotExist()
		{
			var transactionArticles = GetTransactionArticles();
			transactionArticles.Add(new TransactionArticle() { ArticleId = 1, ArticleCount = 1 });

			var transaction = GetTransaction(1, 1, transactionArticles: transactionArticles);

			var result = await _controller.CreateTransaction(transaction);

			Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
			Assert.That((result as NotFoundObjectResult).Value, Is.EqualTo($"Customer {transaction.CustomerId} not found"));
		}

		[Test]
		public async Task CreateTransaction_ReturnsNotFound_WhenArticleDoesNotExist()
		{
			var transactionArticles = GetTransactionArticles();
			transactionArticles.Add(new TransactionArticle() { ArticleId = 1, ArticleCount = 1 });

			var transaction = GetTransaction(1, 1, transactionArticles: transactionArticles);

			_context.Customers.Add(GetCustomer());
			_context.SaveChanges();

			var result = await _controller.CreateTransaction(transaction);

			Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
			Assert.That((result as NotFoundObjectResult).Value, Is.EqualTo($"Article {1} not found."));
		}

		[Test]
		public async Task CreateTransaction_ReturnsBadRequest_WhenNotEnoughInventory()
		{
			var transactionArticles = GetTransactionArticles();
			transactionArticles.Add(new TransactionArticle() { ArticleId = 1, ArticleCount = 1 });

			var transaction = GetTransaction(1, 1, transactionArticles: transactionArticles);

			_context.Customers.Add(GetCustomer());
			_context.Articles.Add(GetArticle());

			var result = await _controller.CreateTransaction(transaction);

			Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
			Assert.That((result as BadRequestObjectResult).Value, Is.EqualTo($"Not enough article {1}."));
		}

		[Test]
		public async Task CreateTransaction_CreatesTransaction_WhenSuccessful()
		{
			var transactionArticles = GetTransactionArticles();
			transactionArticles.Add(new TransactionArticle() { ArticleId = 1, ArticleCount = 1 });

			var transaction = GetTransaction(1, 1, transactionArticles: transactionArticles);

			_context.Customers.Add(GetCustomer());
			_context.Articles.Add(GetArticle(inventory: 2));

			var result = await _controller.CreateTransaction(transaction);

			Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
			Assert.That((result as CreatedAtActionResult).RouteValues["id"], Is.EqualTo(transaction.TransactionId));
		}

		private Transaction GetTransaction(long customerId = 1,
			long transactionId = 1,
			ICollection<Payment> payments = null,
			ICollection<TransactionArticle> transactionArticles = null)
		{
			return new Transaction()
			{
				CustomerId = customerId,
				TransactionId = transactionId,
				Payments = payments,
				TransactionArticles = transactionArticles
			};
		}

		private ICollection<Payment> GetPayments()
		{
			return new List<Payment>();
		}

		private ICollection<TransactionArticle> GetTransactionArticles()
		{
			return new List<TransactionArticle>();
		}

		private Customer GetCustomer(long customerId = 1)
		{
			return new Customer()
			{
				CustomerId = customerId,
				Email = "test",
				Name = "test",
				Phone = "test"
			};
		}

		private Article GetArticle(int articleId = 1,
			string title = "title",
			decimal price = 1,
			int inventory = 0)
		{
			return new Article()
			{
				ArticleId = articleId,
				Title = title,
				Price = price,
				Inventory = inventory
			};
		}

		private DbContextOptions<ApplicationDbContext> GetOptions()
		{
			return new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDatabase")
				.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
			.Options;
		}
	}
}
