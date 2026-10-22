using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // GET /api/orders/check-stock
    [HttpGet("check-stock")]
    public async Task<IActionResult> CheckStock()
    {
        try
        {
            // Se usa el nombre lógico del servicio, no una IP: dentro de la red de Docker
            // "inventory-service" lo resuelve el DNS interno de Docker a la IP actual del contenedor.
            var client = new HttpClient();
            var response = await client.GetAsync("http://inventory-service:8080/api/inventory/P001");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    error = $"InventoryService respondió con error {(int)response.StatusCode}.",
                    detail = content
                });
            }

            return Content(content, "application/json");
        }
        catch (HttpRequestException ex)
        {
            // Fuera de Docker el nombre "inventory-service" no resuelve (o el servicio no está levantado).
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "No se pudo contactar a inventory-service. El nombre lógico solo resuelve dentro de la red de Docker (Docker Compose).",
                detail = ex.Message
            });
        }
    }
}
