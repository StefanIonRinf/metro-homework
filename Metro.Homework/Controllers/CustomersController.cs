using Metro.Homework.Contexts;
using Metro.Homework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Metro.Homework.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        private readonly string phoneNumberPattern = @"^(\+\d{1,3}[- ]?)?\(?\d{1,4}?\)?[- ]?\d{1,4}[- ]?\d{1,9}$";
        private readonly string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

		public CustomersController(ApplicationDbContext context,
            ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomersAsync()
        {
            try
            {
				return Ok(await _context.Customers.ToListAsync());
			}
            catch (Exception ex)
            {
				_logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int? id)
        {
            try
            {
				if (id == null)
				{
					return NotFound();
				}

				var customer = await _context.Customers
					.FirstOrDefaultAsync(m => m.CustomerId == id);

				if (customer == null)
				{
					return NotFound();
				}

				return Ok(customer);
			}
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("CustomerId,Name")] Customer customer)
        {
            try
            {
                ValidateCustomer(customer);
                if (CustomerExists(customer.CustomerId))
                {
					ModelState.AddModelError(nameof(Customer.CustomerId), "Customer already exists");
				}

				if (ModelState.IsValid && CustomerExists(customer.CustomerId) == false)
				{
					_context.Customers.Add(customer);
					await _context.SaveChangesAsync();
					return CreatedAtAction(nameof(Create), new { id = customer.CustomerId }, customer);
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
        public async Task<IActionResult> Edit(long id, [Bind("CustomerId,Name")] Customer customer)
        {
            try
            {
				if (id != customer.CustomerId || CustomerExists(id) == false)
				{
					return NotFound();
				}

				ValidateCustomer(customer);

				if (ModelState.IsValid)
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    return Ok(customer);
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
				var customer = await _context.Customers.FindAsync(id);
				if (customer == null)
                {
                    return NotFound();
                }

				_context.Customers.Remove(customer);
				await _context.SaveChangesAsync();
				return Ok();
			}
            catch (Exception ex)
            {
				_logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        private bool CustomerExists(long id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

		private void ValidateCustomer(Customer customer)
		{
			if (Regex.IsMatch(customer.Phone, phoneNumberPattern) == false)
			{
				ModelState.AddModelError("Phone number", "Provide a valid phone number");
			}

			if (Regex.IsMatch(customer.Email, emailPattern) == false)
			{
				ModelState.AddModelError("Email", "Provide a valid email address");
			}
		}
	}
}
