using Metro.Homework.Contexts;
using Metro.Homework.Controllers;
using Metro.Homework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Metro.Homework.Tests.Controllers
{
	[TestFixture]
	public class ArticlesControllerTests
	{
		private ApplicationDbContext _context;
		private ILogger<ArticlesController> _logger;
		private ArticlesController _controller;

		[SetUp]
		public void Setup()
		{
			_context = new ApplicationDbContext(GetOptions());
			_logger = Substitute.For<ILogger<ArticlesController>>();

			_controller = new ArticlesController(_context, _logger);
		}

		[TearDown]
		public void TearDown() 
		{
			try
			{
				_context.Articles.RemoveRange(_context.Articles);
				_context.SaveChanges();
				_context.Dispose();
				_controller.Dispose();
			}
			catch (Exception ex)
			{
				// Will throw error whem context is mocked
			}
		} 

		[Test]
		public async Task GetArticlesAsync_ExceptionThrown_500CodeReturned() 
		{
			// Arrange
			// Use mocked context for this case because we need to mock_context.Articles.ToListAsync() and NSubstitute can't mock real objects
			_context = Substitute.For<ApplicationDbContext>(GetOptions());
			_context.Articles.ToListAsync().Throws(new Exception());

			_controller = new ArticlesController(_context, _logger);

			// Act
			var result = await _controller.GetArticlesAsync();

			// Assert
			_logger.ReceivedWithAnyArgs().LogError("");
			Assert.That((result as StatusCodeResult).StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
		}

		[Test]
		public async Task GetArticlesAsync_NoArticles_EmptyListReturned()
		{
			// Act
			var result = await _controller.GetArticlesAsync();

			// Assert
			Assert.That(result, Is.InstanceOf<OkObjectResult>());
			Assert.That((result as OkObjectResult).Value, Is.Empty);
		}

		[Test]
		public async Task GetArticlesAsync_OneArticles_ListReturned()
		{
			// Arrange
			_context.Articles.Add(GetArticle());
			_context.SaveChanges();

			// Act
			var result = await _controller.GetArticlesAsync();

			// Assert
			Assert.That(result, Is.InstanceOf<OkObjectResult>());
			Assert.That((result as OkObjectResult).Value, Is.InstanceOf<List<Article>>());
			Assert.That(((result as OkObjectResult).Value as List<Article>).Count, Is.EqualTo(1));
			Assert.That(((result as OkObjectResult).Value as List<Article>).First().ArticleId, Is.EqualTo(1));
		}

		[Test]
		public async Task GetArticleById_ReturnsNotFound_WhenIdIsNull()
		{
			// Act
			var result = await _controller.GetArticleById(null);

			// Assert
			Assert.That(result, Is.InstanceOf<NotFoundResult>());
		}

		[Test]
		public async Task GetArticleById_ReturnsNotFound_WhenArticleDoesNotExist()
		{
			// Act
			var result = await _controller.GetArticleById(1);

			// Assert
			Assert.That(result, Is.InstanceOf<NotFoundResult>());
		}

		[Test]
		public async Task GetArticleById_ReturnsOk_WhenArticleExists()
		{
			// Arrange
			var article = GetArticle();
			_context.Articles.Add(article);
			_context.SaveChanges();

			// Act
			var result = await _controller.GetArticleById(1);

			// Assert
			Assert.That(result, Is.InstanceOf<OkObjectResult>());
			Assert.That((result as OkObjectResult).Value as Article, Is.EqualTo(article));
		}

		[Test]
		public async Task Create_ReturnsBadRequest_WhenModelStateIsInvalid()
		{
			// Arrange
			_controller.ModelState.AddModelError("Title", "Required");

			var article = new Article { ArticleId = 1, Title = "", Price = 10, Inventory = 100 };

			// Act
			var result = await _controller.Create(article);

			// Assert
			Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
		}

		[Test]
		public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
		{
			// Arrange
			var article = GetArticle();

			// Act
			var result = await _controller.Create(article);

			// Assert
			Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
			Assert.That((result as CreatedAtActionResult).Value as Article, Is.EqualTo(article));
		}

		[Test]
		public async Task Edit_ReturnsNotFound_WhenIdDoesNotMatch()
		{
			// Arrange
			var article = GetArticle();

			// Act
			var result = await _controller.Edit(1, article);

			// Assert
			Assert.That(result, Is.InstanceOf<NotFoundResult>());
		}

		[Test]
		public async Task Edit_ReturnsBadRequest_WhenModelStateIsInvalid()
		{
			// Arrange
			_controller.ModelState.AddModelError("Title", "Required");

			var article = GetArticle();
			_context.Articles.Add(article);
			_context.SaveChanges();

			// Act
			var result = await _controller.Edit(1, article);

			// Assert
			Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
		}

		[Test]
		public async Task Delete_ReturnsNotFound_WhenArticleDoesNotExist()
		{
			// Act
			var result = await _controller.Delete(1);

			// Assert
			Assert.That(result, Is.InstanceOf<NotFoundResult>());
		}

		[Test]
		public async Task Delete_ReturnsOk_WhenArticleIsDeleted()
		{
			// Arrange
			var article = GetArticle();
			_context.Articles.Add(article);
			_context.SaveChanges();

			// Act
			var result = await _controller.Delete(article.ArticleId);

			// Assert
			Assert.That(result, Is.InstanceOf<OkResult>());
		}

		private DbContextOptions<ApplicationDbContext> GetOptions()
		{
			return new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDatabase")
			.Options;
		}

		private Article GetArticle(int articleId = 1,
			string title = "title",
			decimal price = 1,
			int inventory = 1)
		{
			return new Article()
			{
				ArticleId = articleId,
				Title = title,
				Price = price,
				Inventory = inventory
			};
		}
	}
}
