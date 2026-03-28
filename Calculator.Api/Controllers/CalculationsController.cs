using Calculator.Api.Contracts;
using Calculator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Route("api/calculations")]
public sealed class CalculationsController(ICalculationService calculationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Calculate([FromBody] CalculationRequest request)
    {
        try
        {
            var response = await calculationService.ExecuteAsync(request);
            return Ok(response);
        }
        catch (CalculationValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("recent")]
    public async Task<IActionResult> Recent([FromQuery] int? take)
    {
        try
        {
            var items = await calculationService.GetRecentAsync(take);
            return Ok(items);
        }
        catch (HistoryUnavailableException ex)
        {
            return Problem(
                title: "History unavailable",
                detail: ex.Message,
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}

