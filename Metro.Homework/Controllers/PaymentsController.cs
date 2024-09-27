using Metro.Homework.Contexts;
using Metro.Homework.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Metro.Homework.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ApplicationDbContext context,
            ILogger<PaymentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaymentsAsync()
        {
            try
            {
				return Ok(await _context.Payments.ToListAsync());
			}
            catch (Exception ex)
            {
				_logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(long? id)
        {
            try
            {
				if (id == null)
				{
					return NotFound();
				}

				var payment = await _context.Payments
					.FirstOrDefaultAsync(m => m.PaymentId == id);

				if (payment == null)
				{
					return NotFound();
				}

				return Ok(payment);
			}
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("PaymentId,TransactionId,Amount,PaymentDate,Status")] Payment payment)
        {
            try
            {
                ValidatePayment(payment);

				if (PaymentExists(payment.PaymentId))
				{
					ModelState.AddModelError(nameof(Payment.PaymentId), "Payment already exists");
				}

				if (ModelState.IsValid)
				{
					_context.Add(payment);
					await _context.SaveChangesAsync();
					return CreatedAtAction(nameof(Create), new { id = payment.PaymentId }, payment);
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
        public async Task<IActionResult> Edit(long id, [Bind("PaymentId,TransactionId,Amount,PaymentDate,Status")] Payment payment)
        {
            try
            {
				if (id != payment.PaymentId || PaymentExists(id) == false)
				{
					return NotFound();
				}

				ValidatePayment(payment);

				if (ModelState.IsValid)
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                    return Ok(payment);
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
				var payment = await _context.Payments.FindAsync(id);
				if (payment == null)
                {
                    return NotFound();
                }

				_context.Payments.Remove(payment);
				await _context.SaveChangesAsync();
				return Ok();
			}
            catch (Exception ex)
            {
				_logger.LogError(ex.ToString());
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
        }

        private void ValidatePayment(Payment payment)
        {
            if (payment.PaymentId <= 0)
            {
                ModelState.AddModelError(nameof(Payment.PaymentId), "Provide a valid payment id");
            }

			if (payment.TransactionId <= 0)
			{
				ModelState.AddModelError(nameof(Payment.TransactionId), "Provide a valid TransactionId id");
			}

			if (payment.Amount <= 0)
			{
				ModelState.AddModelError(nameof(Payment.Amount), "Provide a valid Amount");
			}
		}

        private bool PaymentExists(long id)
        {
            return _context.Payments.Any(e => e.PaymentId == id);
        }
    }
}
