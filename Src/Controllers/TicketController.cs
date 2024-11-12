using System.Text.Json;
using dotnet_tickets.Src.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_tickets.Src.Controllers
{
    /// <summary>
    /// Controlador para la gestión de tickets.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        /// <summary>
        /// Clave para la cookie que almacena la lista de tickets.
        /// </summary>
        private const string TicketsCookieKey = "TicketsList";

        /// <summary>
        /// Clave para la cookie que almacena el GUID del usuario.
        /// </summary>
        private const string UserCookieKey = "UserGUID";

        [HttpGet]
        public IActionResult GetTickets()
        {
            // Obtener o crear un GUID para el usuario
            var userGuid = GetOrCreateUserGuid();

            // Obtener los tickets del usuario mediante su GUID
            var tickets = GetTicketsFromCookies(userGuid);

            return Ok(tickets);
        }

        [HttpPost]
        public IActionResult AddTicket([FromBody] Ticket newTicket)
        {
            // Obtener o crear un GUID para el usuario
            var userGuid = GetOrCreateUserGuid();

            // Obtener los tickets del usuario mediante su GUID
            var tickets = GetTicketsFromCookies(userGuid);

            // Asignar un ID al nuevo ticket
            newTicket.Id = tickets.Count > 0 ? tickets.Max(t => t.Id) + 1 : 1;

            // Agregar el nuevo ticket a la lista de tickets
            tickets.Add(newTicket);

            // Guardar la lista de tickets en las cookies
            SaveTicketsToCookies(userGuid, tickets);

            return Ok(newTicket);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTicket(int id, [FromBody] Ticket updatedTicket)
        {
            // Obtener o crear un GUID para el usuario
            var userGuid = GetOrCreateUserGuid();

            // Obtener los tickets del usuario mediante su GUID
            var tickets = GetTicketsFromCookies(userGuid);

            // Buscar el ticket a actualizar
            var existingTicket = tickets.FirstOrDefault(t => t.Id == id);

            // Si no se encuentra el ticket, devolver un error
            if (existingTicket == null)
            {
                return NotFound("Ticket not found");
            }

            // Actualizar los datos del ticket
            existingTicket.EventName = updatedTicket.EventName;
            existingTicket.Price = updatedTicket.Price;

            // Guardar la lista de tickets en las cookies
            SaveTicketsToCookies(userGuid, tickets);

            return Ok(existingTicket);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTicket(int id)
        {
            // Obtener o crear un GUID para el usuario
            var userGuid = GetOrCreateUserGuid();

            // Obtener los tickets del usuario mediante su GUID
            var tickets = GetTicketsFromCookies(userGuid);

            // Buscar el ticket a eliminar
            var ticketToRemove = tickets.FirstOrDefault(t => t.Id == id);

            // Si no se encuentra el ticket, devolver un error
            if (ticketToRemove == null)
            {
                return NotFound("Ticket not found");
            }

            // Eliminar el ticket de la lista de tickets
            tickets.Remove(ticketToRemove);

            // Guardar la lista de tickets en las cookies
            SaveTicketsToCookies(userGuid, tickets);

            return NoContent();
        }

        /// <summary>
        /// Obtiene o crea un GUID para el usuario actual.
        /// </summary>
        /// <returns>
        /// El GUID del usuario.
        /// </returns>
        private string GetOrCreateUserGuid()
        {
            var userGuid = Request.Cookies[UserCookieKey];

            if (string.IsNullOrEmpty(userGuid))
            {
                userGuid = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false,
                    Expires = DateTime.Now.AddDays(7)
                };
                Response.Cookies.Append(UserCookieKey, userGuid, cookieOptions);
            }

            return userGuid;
        }

        /// <summary>
        /// Obtiene los tickets del usuario mediante su GUID.
        /// </summary>
        /// <param name="userGuid">
        /// El GUID del usuario.
        /// </param>
        /// <returns>
        /// La lista de tickets del usuario.
        /// </returns>
        private List<Ticket> GetTicketsFromCookies(string userGuid)
        {
            var cookieValue = Request.Cookies[TicketsCookieKey + "_" + userGuid];
            if (!string.IsNullOrEmpty(cookieValue))
            {
                return JsonSerializer.Deserialize<List<Ticket>>(cookieValue) ?? [];
            }
            return [];
        }

        /// <summary>
        /// Guarda la lista de tickets en las cookies.
        /// </summary>
        /// <param name="userGuid">
        /// El GUID del usuario.
        /// </param>
        /// <param name="tickets">
        /// La lista de tickets a guardar.
        /// </param>
        private void SaveTicketsToCookies(string userGuid, List<Ticket> tickets)
        {
            var serializedTickets = JsonSerializer.Serialize(tickets);

            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = false,
                Secure = false,
                Expires = DateTime.Now.AddDays(7)
            };

            Response.Cookies.Append(
                TicketsCookieKey + "_" + userGuid,
                serializedTickets,
                cookieOptions
            );
        }
    }
}
