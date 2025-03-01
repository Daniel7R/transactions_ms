using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Exceptions;
using Stripe.Checkout;
using System.Net.Mime;
using System.Security.Claims;

namespace PaymentsMS.API.Controllers
{
    [Route("api/v1/transactions")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ILogger<TransactionsController> _logger;
        private readonly IDonationService _donationService;
        private readonly ISaleService _saleService;
        private readonly ISessionStripe _sessionStripe;
        public TransactionsController(ILogger<TransactionsController> logger, IDonationService donationService, ISaleService saleService, ISessionStripe sessionStripe)
        {
            _logger = logger;
            _donationService = donationService;
            _saleService = saleService;
            _sessionStripe = sessionStripe;
        }

        /// <summary>
        /// Creates a sale transaction
        /// </summary>
        /// <param name="saleRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("sale")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ResponseDTO<SaleRequestDTO?>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<SaleRequestDTO?>), statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateSale([FromBody] SaleRequestDTO saleRequest)
        {
            ResponseDTO<StripeRequestDTO?> response = new ResponseDTO<StripeRequestDTO?>();
            try
            {
                var user = ExtractUserId();
                if (string.IsNullOrEmpty(user)) throw new BusinessRuleException("Invalid User");

                var sessionResult = await _saleService.MakeSaleTransaction(saleRequest, Convert.ToInt32(user));
                response.Result = sessionResult;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Can not create StripeSession, error: {ex.Message}");
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Create a donation transaction(only session
        /// </summary>
        /// <param name="donationRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("donation")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateDonation([FromBody]DonationsRequestDTO donationRequest)
        {
            var response = new ResponseDTO<DonationsRequestDTO?>();
            try
            {
                var user = ExtractUserId();
                if (string.IsNullOrEmpty(user)) throw new BusinessRuleException("Invalid User");

                var resultSession = await _donationService.MakeDonationTransaction(donationRequest,Convert.ToInt32(user));
                response.Result = (DonationsRequestDTO)resultSession;

                return Ok(response);

            } catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }

        //[ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
        [Authorize]
        [HttpPost]
        [Route("status")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> TransactionStatus([FromBody]TransactionStatusRequestDTO transactionRequest)
        {
            var response = new ResponseDTO<StatusTransactionDTO?>();
            try
            {
                var user = ExtractUserId();
                if (string.IsNullOrEmpty(user)) throw new BusinessRuleException("Invalid User");

                transactionRequest.IdUser = Convert.ToInt32(user);

                var resultValidation = await _sessionStripe.ValidateTransaction(transactionRequest);
                response.Result = resultValidation;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                response.IsSuccess = false;
                response.Message = ex.Message;
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Extract user id from token payload
        /// </summary>
        /// <returns></returns>
        private string? ExtractUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

            return userId;
        }
    }

}
