using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Core.Interfaces;
using PaymentGateway.Core.Models;

namespace PaymentGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            var result = await _paymentService.ProcessPaymentAsync(request);
            if (!result.IsSuccess && result.Status == PaymentGateway.Core.Enums.PaymentStatus.Rejected)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var payment = await _paymentService.GetPaymentAsync(id);
            if (payment == null)
                return NotFound();
            return Ok(payment);
        }
    }
}