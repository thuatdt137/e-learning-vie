using e_learning_vie.Commons;
using e_learning_vie.DTOs.Room;
using e_learning_vie.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace e_learning_vie.Controllers.RoomManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly SchoolManagementContext _context;
        public RoomsController(SchoolManagementContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetRooms()
        {
            var rooms = _context.Rooms.Select(r => new RoomDTO
            {
                RoomId = r.RoomId,
                Name = r.Name,
                Description = r.Description
            }).ToList();
            return Ok(ApiResponse<object>.Success("Success", rooms));
        }
        [HttpGet("{id}")]
        public IActionResult GetRoomById(int id)
        {
            var room = _context.Rooms.Where(r => r.RoomId == id).Select(r => new RoomDTO
            {
                RoomId = r.RoomId,
                Name = r.Name,
                Description = r.Description
            }).FirstOrDefault();
            if (room == null)
            {
                return NotFound(ApiResponse<object>.Fail("Room not found."));
            }
            return Ok(ApiResponse<object>.Success("Success", room));
        }
        [HttpPost]
        public IActionResult CreateRoom([FromBody] RoomDTO roomDTO)
        {
            if (roomDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid room data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new System.Collections.Generic.List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var room = new Room
            {
                Name = roomDTO.Name,
                Description = roomDTO.Description
            };
            var result = _context.Rooms.Add(room);
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Room created successfully.", result.Entity));
        }
        [HttpPut("{id}")]
        public IActionResult UpdateRoom(int id, [FromBody] RoomDTO roomDTO)
        {
            if (roomDTO == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid room data."));
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value != null && ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors != null ? kvp.Value.Errors.Select(e => e.ErrorMessage).ToList() : new System.Collections.Generic.List<string>()
                    );
                return BadRequest(ApiResponse<object>.Fail("Invalid data", errors));
            }
            var room = _context.Rooms.Find(id);
            if (room == null)
            {
                return NotFound(ApiResponse<object>.Fail("Room not found."));
            }
            room.Name = roomDTO.Name;
            room.Description = roomDTO.Description;
            _context.SaveChanges();
            return Ok(ApiResponse<object>.Success("Room updated successfully.", room));
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteRoom(int id)
        {
            var room = _context.Rooms.Find(id);
            if (room == null)
            {
                return NotFound(ApiResponse<object>.Fail("Room not found."));
            }

            try
            {
                _context.Rooms.Remove(room);
                _context.SaveChanges();
                return Ok(ApiResponse<object>.Success("Room deleted successfully."));
            }
            catch (DbUpdateException)
            {
                return BadRequest(ApiResponse<object>.Fail("Cannot delete room because it is referenced by other records."));
            }
        }

    }
}
