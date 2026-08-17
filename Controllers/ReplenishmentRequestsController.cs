using Microsoft.AspNetCore.Mvc;
using StockReplenishment.Interfaces;
using StockReplenishment.Models;

namespace StockReplenishment.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReplenishmentRequestsController(IReplenishmentRequestService requestService) : ControllerBase
{
    private string? CurrentRole => Request.Headers["X-Simulated-Role"].FirstOrDefault();

    private ObjectResult CreateError(int statusCode, string message)
    {
        return StatusCode(statusCode, new ErrorResponse { StatusCode = statusCode, Message = message });
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of replenishment requests
    /// </summary>
    /// <param name="page">The page number to retrieve</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="status">Optional filter by request status</param>
    /// <param name="priority">Optional filter by request priority</param>
    /// <returns>A paginated list of requests</returns>
    /// <response code="200">Returns the requested list of items</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReplenishmentRequest>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequests(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] RequestStatus? status = null,
        [FromQuery] RequestPriority? priority = null)
    {
        var allRequests = await requestService.GetAllRequestsAsync();

        if (status.HasValue)
            allRequests = allRequests.Where(r => r.Status == status.Value);
        
        if (priority.HasValue)
            allRequests = allRequests.Where(r => r.Priority == priority.Value);

        var totalCount = allRequests.Count();
        var pagedItems = allRequests.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var result = new PagedResult<ReplenishmentRequest>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Items = pagedItems
        };

        return Ok(result);
    }

    /// <summary>
    /// Finds a specific replenishment request by ID
    /// </summary>
    /// <param name="id">ID of the request we want to find</param>
    /// <returns>A single request object</returns>
    /// <response code="200">Returns the requested request object</response>
    /// <response code="400">If the provided ID is 0 or a negative number</response>
    /// <response code="404">If no request matches the provided ID</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReplenishmentRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequest(int id)
    {
        if (id <= 0) return CreateError(400, "ID must be a positive integer.");

        var request = await requestService.GetRequestByIdAsync(id);
        if (request == null) return CreateError(404, $"Request with ID {id} was not found.");
        
        return Ok(request);
    }

    /// <summary>
    /// Creates a new replenishment request in Draft status
    /// </summary>
    /// <param name="request">The request payload containing location and items</param>
    /// <returns>The newly created request</returns>
    /// <response code="201">Returns the newly created request</response>
    /// <response code="400">If the request payload is invalid or contains no items</response>
    /// <response code="403">If the user is not a Worker</response>
    [HttpPost]
    [ProducesResponseType(typeof(ReplenishmentRequest), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateDraft([FromBody] ReplenishmentRequest request)
    {
        if (CurrentRole != "Worker") 
            return CreateError(403, "Only Workers can create requests.");

        if (!request.Items.Any()) 
            return CreateError(400, "A request must contain at least one line item.");

        var createdRequest = await requestService.CreateDraftAsync(request);
        return CreatedAtAction(nameof(GetRequest), new { id = createdRequest.Id }, createdRequest);
    }

    /// <summary>
    /// Submits a draft request for approval and triggers external stock validation
    /// </summary>
    /// <param name="id">ID of the request to submit</param>
    /// <returns>The updated request</returns>
    /// <response code="202">Returns the submitted request indicating background processing has started</response>
    /// <response code="400">If the provided ID is invalid, or the request is not in Draft status</response>
    /// <response code="403">If the user is not a Worker</response>
    /// <response code="404">If no request matches the provided ID</response>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(ReplenishmentRequest), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitRequest(int id)
    {
        if (id <= 0) return CreateError(400, "ID must be a positive integer.");
        if (CurrentRole != "Worker") return CreateError(403, "Only Workers can submit requests.");

        var request = await requestService.SubmitRequestAsync(id);
        if (request == null) return CreateError(404, "Request not found or not in Draft status.");

        return Accepted(request);
    }

    /// <summary>
    /// Approves a submitted request
    /// </summary>
    /// <param name="id">ID of the request to approve</param>
    /// <returns>The approved request</returns>
    /// <response code="200">Returns the approved request</response>
    /// <response code="400">If the provided ID is invalid</response>
    /// <response code="403">If the user is not a Reviewer</response>
    /// <response code="404">If no request matches the provided ID or it is not Submitted</response>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(ReplenishmentRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveRequest(int id)
    {
        if (id <= 0) return CreateError(400, "ID must be a positive integer.");
        if (CurrentRole != "Reviewer") return CreateError(403, "Only Reviewers can approve requests.");

        var request = await requestService.ApproveRequestAsync(id);
        if (request == null) return CreateError(404, "Request not found or not in Submitted status.");

        return Ok(request);
    }

    /// <summary>
    /// Rejects a submitted request with a specified reason
    /// </summary>
    /// <param name="id">ID of the request to reject</param>
    /// <param name="dto">Payload containing the rejection reason</param>
    /// <returns>The rejected request</returns>
    /// <response code="200">Returns the rejected request</response>
    /// <response code="400">If the ID is invalid or the rejection reason is missing</response>
    /// <response code="403">If the user is not a Reviewer</response>
    /// <response code="404">If no request matches the provided ID or it is not Submitted</response>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(typeof(ReplenishmentRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRequest(int id, [FromBody] RejectRequestDto dto)
    {
        if (id <= 0) return CreateError(400, "ID must be a positive integer.");
        if (CurrentRole != "Reviewer") return CreateError(403, "Only Reviewers can reject requests.");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            return CreateError(400, "A rejection reason is required.");

        var request = await requestService.RejectRequestAsync(id, dto.Reason);
        if (request == null) return CreateError(404, "Request not found or not in Submitted status.");

        return Ok(request);
    }

    /// <summary>
    /// Marks an approved request as fulfilled
    /// </summary>
    /// <param name="id">ID of the request to fulfill</param>
    /// <returns>The fulfilled request</returns>
    /// <response code="200">Returns the fulfilled request</response>
    /// <response code="400">If the provided ID is invalid</response>
    /// <response code="403">If the user is not a Worker</response>
    /// <response code="404">If no request matches the provided ID or it is not Approved</response>
    [HttpPost("{id}/fulfill")]
    [ProducesResponseType(typeof(ReplenishmentRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FulfillRequest(int id)
    {
        if (id <= 0) return CreateError(400, "ID must be a positive integer.");
        if (CurrentRole != "Worker") return CreateError(403, "Only Workers can fulfill requests.");

        var request = await requestService.FulfillRequestAsync(id);
        if (request == null) return CreateError(404, "Request not found or not in Approved status.");

        return Ok(request);
    }
}