namespace dotnet_tickets.Src.Models
{
    /// <summary>
    /// Modelo de datos para un ticket.
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Identificador del ticket.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del evento.
        /// </summary>
        public required string EventName { get; set; }

        /// <summary>
        /// Precio del ticket.
        /// </summary>
        public required int Price { get; set; }
    }
}
