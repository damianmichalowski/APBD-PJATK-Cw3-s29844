using APBD_PJATK_Cw3_s29844.Data;
using APBD_PJATK_Cw3_s29844.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD_PJATK_Cw3_s29844.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Room>> GetRooms(
        [FromQuery] int? minCapacity,
        [FromQuery] bool? hasProjector,
        [FromQuery] bool? activeOnly)
    {
        IEnumerable<Room> rooms = InMemoryDatabase.Rooms;

        if (minCapacity.HasValue)
        {
            rooms = rooms.Where(room => room.Capacity >= minCapacity.Value);
        }

        if (hasProjector.HasValue)
        {
            rooms = rooms.Where(room => room.HasProjector == hasProjector.Value);
        }

        if (activeOnly == true)
        {
            rooms = rooms.Where(room => room.IsActive);
        }

        return Ok(rooms);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Room> GetRoomById([FromRoute] int id)
    {
        var room = InMemoryDatabase.Rooms.FirstOrDefault(room => room.Id == id);

        if (room is null)
        {
            return NotFound($"Sala o id {id} nie istnieje.");
        }

        return Ok(room);
    }

    [HttpGet("building/{buildingCode}")]
    public ActionResult<IEnumerable<Room>> GetRoomsByBuildingCode([FromRoute] string buildingCode)
    {
        var rooms = InMemoryDatabase.Rooms
            .Where(room => room.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(rooms);
    }

    [HttpPost]
    public ActionResult<Room> CreateRoom([FromBody] Room room)
    {
        room.Id = InMemoryDatabase.GetNextRoomId();
        NormalizeRoom(room);

        InMemoryDatabase.Rooms.Add(room);

        return CreatedAtAction(
            nameof(GetRoomById),
            new { id = room.Id },
            room
        );
    }

    [HttpPut("{id:int}")]
    public ActionResult<Room> UpdateRoom([FromRoute] int id, [FromBody] Room updatedRoom)
    {
        var room = InMemoryDatabase.Rooms.FirstOrDefault(room => room.Id == id);

        if (room is null)
        {
            return NotFound($"Sala o id {id} nie istnieje.");
        }

        updatedRoom.Id = id;
        NormalizeRoom(updatedRoom);

        room.Name = updatedRoom.Name;
        room.BuildingCode = updatedRoom.BuildingCode;
        room.Floor = updatedRoom.Floor;
        room.Capacity = updatedRoom.Capacity;
        room.HasProjector = updatedRoom.HasProjector;
        room.IsActive = updatedRoom.IsActive;

        return Ok(room);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteRoom([FromRoute] int id)
    {
        var room = InMemoryDatabase.Rooms.FirstOrDefault(room => room.Id == id);

        if (room is null)
        {
            return NotFound($"Sala o id {id} nie istnieje.");
        }

        var hasReservations = InMemoryDatabase.Reservations
            .Any(reservation => reservation.RoomId == id);

        if (hasReservations)
        {
            return Conflict("Nie można usunąć sali, ponieważ istnieją dla niej rezerwacje.");
        }

        InMemoryDatabase.Rooms.Remove(room);

        return NoContent();
    }

    private static void NormalizeRoom(Room room)
    {
        room.Name = room.Name.Trim();
        room.BuildingCode = room.BuildingCode.Trim().ToUpperInvariant();
    }
}