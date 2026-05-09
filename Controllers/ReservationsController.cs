using APBD_PJATK_Cw3_s29844.Models;
using Microsoft.AspNetCore.Mvc;
using APBD_PJATK_Cw3_s29844.Data;

namespace APBD_PJATK_Cw3_s29844.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Reservation>> GetReservations(
        [FromQuery] DateOnly? date,
        [FromQuery] string? status,
        [FromQuery] int? roomId)
    {
        IEnumerable<Reservation> reservations = Database.Reservations;

        if (date.HasValue)
        {
            reservations = reservations.Where(reservation => reservation.Date == date.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            reservations = reservations.Where(reservation =>
                reservation.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (roomId.HasValue)
        {
            reservations = reservations.Where(reservation => reservation.RoomId == roomId.Value);
        }

        return Ok(reservations);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservation> GetReservationById([FromRoute] int id)
    {
        var reservation = Database.Reservations
            .FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound($"Rezerwacja o id {id} nie istnieje.");
        }

        return Ok(reservation);
    }

    [HttpPost]
    public ActionResult<Reservation> CreateReservation([FromBody] Reservation reservation)
    {
        var room = Database.Rooms
            .FirstOrDefault(room => room.Id == reservation.RoomId);

        if (room is null)
        {
            return BadRequest("Nie można dodać rezerwacji dla sali, która nie istnieje.");
        }

        if (!room.IsActive)
        {
            return Conflict("Nie można dodać rezerwacji dla sali, która jest nieaktywna.");
        }

        NormalizeReservation(reservation);

        if (string.IsNullOrWhiteSpace(reservation.OrganizerName) || string.IsNullOrWhiteSpace(reservation.Topic))
        {
            return BadRequest("Organizator i temat rezerwacji nie mogą być puste.");
        }

        if (HasTimeConflict(reservation))
        {
            return Conflict("Istnieje już rezerwacja tej sali w podanym terminie.");
        }

        reservation.Id = Database.GetNextReservationId();

        Database.Reservations.Add(reservation);

        return CreatedAtAction(
            nameof(GetReservationById),
            new { id = reservation.Id },
            reservation
        );
    }

    [HttpPut("{id:int}")]
    public ActionResult<Reservation> UpdateReservation(
        [FromRoute] int id,
        [FromBody] Reservation updatedReservation)
    {
        var reservation = Database.Reservations
            .FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound($"Rezerwacja o id {id} nie istnieje.");
        }

        var room = Database.Rooms
            .FirstOrDefault(room => room.Id == updatedReservation.RoomId);

        if (room is null)
        {
            return BadRequest("Nie można przypisać rezerwacji do sali, która nie istnieje.");
        }

        if (!room.IsActive)
        {
            return Conflict("Nie można przypisać rezerwacji do sali, która jest nieaktywna.");
        }

        updatedReservation.Id = id;
        NormalizeReservation(updatedReservation);

        if (string.IsNullOrWhiteSpace(updatedReservation.OrganizerName) || string.IsNullOrWhiteSpace(updatedReservation.Topic))
        {
            return BadRequest("Organizator i temat rezerwacji nie mogą być puste.");
        }

        if (HasTimeConflict(updatedReservation, id))
        {
            return Conflict("Istnieje już rezerwacja tej sali w podanym terminie.");
        }

        reservation.RoomId = updatedReservation.RoomId;
        reservation.OrganizerName = updatedReservation.OrganizerName;
        reservation.Topic = updatedReservation.Topic;
        reservation.Date = updatedReservation.Date;
        reservation.StartTime = updatedReservation.StartTime;
        reservation.EndTime = updatedReservation.EndTime;
        reservation.Status = updatedReservation.Status;

        return Ok(reservation);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteReservation([FromRoute] int id)
    {
        var reservation = Database.Reservations
            .FirstOrDefault(reservation => reservation.Id == id);

        if (reservation is null)
        {
            return NotFound($"Rezerwacja o id {id} nie istnieje.");
        }

        Database.Reservations.Remove(reservation);

        return NoContent();
    }

    private static bool HasTimeConflict(Reservation reservation, int? ignoredReservationId = null)
    {
        if (reservation.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return Database.Reservations.Any(existingReservation =>
            (!ignoredReservationId.HasValue || existingReservation.Id != ignoredReservationId.Value)
            && existingReservation.RoomId == reservation.RoomId
            && existingReservation.Date == reservation.Date
            && !existingReservation.Status.Equals("cancelled", StringComparison.OrdinalIgnoreCase)
            && reservation.StartTime < existingReservation.EndTime
            && existingReservation.StartTime < reservation.EndTime
        );
    }

    private static void NormalizeReservation(Reservation reservation)
    {
        reservation.OrganizerName = reservation.OrganizerName.Trim();
        reservation.Topic = reservation.Topic.Trim();
        reservation.Status = reservation.Status.Trim().ToLowerInvariant();
    }
}