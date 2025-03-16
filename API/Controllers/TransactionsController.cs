using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using PaymentsMS.Application.DTOs.commons;
using PaymentsMS.Application.DTOs.Request;
using PaymentsMS.Application.DTOs.Response;
using PaymentsMS.Application.Interfaces;
using PaymentsMS.Domain.Enums;
using PaymentsMS.Domain.Exceptions;
using Stripe;
using Stripe.Checkout;
using System.Net.Mime;
using System.Security.Claims;

namespace PaymentsMS.API.Controllers
{
    [Route("api/v1/transactions")]
    [ApiController]
    [Authorize]
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
        /// Creates a participant sale transaction on Stripe
        /// </summary>
        /// <param name="saleRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("participant/sale")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ResponseDTO<StripeRequestDTO?>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<StripeRequestDTO?>), statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateParticipantSale([FromBody] SaleParticipantRequestDTO saleRequest)
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
            catch (BusinessRuleException businessException)
            {
                _logger.LogError($"Can not create StripeSession, error: {businessException.Message}");
                //response.IsSuccess = false;
                response.Message = businessException.Message;
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Creates a viewer sale transaction on Stripe
        /// </summary>
        /// <param name="viewerSale"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("viewer/sale")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ResponseDTO<StripeRequestDTO?>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<StripeRequestDTO?>),statusCode: StatusCodes.Status400BadRequest)]
        [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateViewerSale([FromBody] SaleViewerRequestDTO viewerSale)
        {
            ResponseDTO<StripeRequestDTO?> response = new();
            try
            {
                string user = ExtractUserId();
                if (string.IsNullOrEmpty(user)) throw new BusinessRuleException("Invalid User");

                var sessionResult = await _saleService.MakeSaleTransaction(viewerSale, Convert.ToInt32(user));
                response.Result = sessionResult;
                return Ok(response);

            }
            catch(BusinessRuleException businessException)
            {

                _logger.LogError(businessException.Message);
                response.Message = businessException.Message;

                return BadRequest(response);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                response.Message = ex.Message;

                return BadRequest(response);
            }
        }

        /// <summary>
        /// Create a donation transaction
        /// </summary>
        /// <param name="donationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("donation")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ResponseDTO<StripeRequestDTO?>), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<StripeRequestDTO?>), statusCode: StatusCodes.Status400BadRequest)]
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
            }
            catch (StripeException se)
            {
                _logger.LogError($"{se.Message}");
                response.Message = se.Message;
                return BadRequest(response);
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }

        /// <summary>
        /// Validates the transaction status, for creating confirmation on transactions db
        /// </summary>
        /// <param name="transactionRequest"></param>
        /// <returns></returns>
        /// <exception cref="BusinessRuleException"></exception>
        [HttpPost]
        [Route("status")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(200, Type =typeof(ResponseDTO<StatusTransactionDTO>))]
        [ProducesResponseType(400, Type = typeof(ResponseDTO<StatusTransactionDTO?>))]
        [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TransactionStatus([FromBody]TransactionStatusRequestDTO transactionRequest)
        {
            var response = new ResponseDTO<StatusTransactionDTO?>();
            try
            {
                var user = ExtractUserId();
                if (string.IsNullOrEmpty(user)) throw new BusinessRuleException("Invalid User");

                transactionRequest.IdUser = Convert.ToInt32(user);
                StatusTransactionDTO resultValidation;

                switch (transactionRequest.TransactionType)
                {
                    case TransactionType.SALE:
                        resultValidation = await _saleService.ValidateSale(transactionRequest);
                        break;
                    case TransactionType.DONATION:
                        resultValidation = await _donationService.ValidateDonation(transactionRequest);
                        break;
                    //it would have as many transactions type as it can
                    default:
                        resultValidation = new StatusTransactionDTO();
                        break;
                }

                response.Result = resultValidation;
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
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
