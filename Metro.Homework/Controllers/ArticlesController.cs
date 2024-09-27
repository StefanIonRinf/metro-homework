using Metro.Homework.Contexts;
using Metro.Homework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Metro.Homework.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(ApplicationDbContext context,
            ILogger<ArticlesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetArticlesAsync()
        {
            try
            {
				return Ok(await _context.Articles.ToListAsync());
			}
            catch (Exception ex)
            {
				_logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleById(long? id)
        {
            try
            {
				if (id == null)
				{
					return NotFound();
				}

				var article = await _context.Articles
					.FirstOrDefaultAsync(m => m.ArticleId == id);

				if (article == null)
				{
					return NotFound();
				}

				return Ok(article);
			}
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("ArticleId,Title,Price,Inventory")] Article article)
        {
            try
            {
				ValidateArticle(article);

                if (ArticleExists(article.ArticleId))
                {
                    ModelState.AddModelError(nameof(Article.ArticleId), "Article already exists");
                }

				if (ModelState.IsValid)
				{
					_context.Add(article);
					await _context.SaveChangesAsync();
					return CreatedAtAction(nameof(Create), new { id = article.ArticleId }, article);
				}

				return BadRequest(ModelState);
			}
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        [HttpPut]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name")] Article article)
        {
            try
            {
				if (id != article.ArticleId || ArticleExists(id) == false)
				{
					return NotFound();
				}

                ValidateArticle(article);

                if (ModelState.IsValid)
                {
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                    return Ok(article);
                }
                else
                {
                    return BadRequest(ModelState);
                }
			}
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
				var article = await _context.Articles.FindAsync(id);
				if (article == null)
                {
                    return NotFound();
                }

				_context.Articles.Remove(article);
				await _context.SaveChangesAsync();
				return Ok();
			}
            catch (Exception ex)
            {
				_logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        private void ValidateArticle(Article article)
        {
            if (article.Price <= 0)
            {
                ModelState.AddModelError(nameof(Article.Price), "Provide a valid Price");
            }

            if (article.Inventory <= 0)
            {
				ModelState.AddModelError(nameof(Article.Inventory), "Provide a valid inventory");
			}
        }


		private bool ArticleExists(long id)
        {
            return _context.Articles.Any(e => e.ArticleId == id);
        }
    }
}
