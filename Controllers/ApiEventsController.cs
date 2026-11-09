using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Race_System_2_.Data;
using Race_System_2_.Models;

namespace Race_System_2_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiEventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ApiEventsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/ApiEvents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: api/ApiEvents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            if (@event == null)
            {
                return NotFound();
            }

            return @event;
        }

        // PUT: api/ApiEvents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, Event @event)
        {
            if (id != @event.EventID)
            {
                return BadRequest();
            }

            _context.Entry(@event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ApiEvents
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(
            [FromForm] string eventName,
            [FromForm] string description,
            [FromForm] DateTime eventDate,
            [FromForm] string eventLocation,
            [FromForm] double distance,
            [FromForm] string eventType,
            [FromForm] string organizerName,
            IFormFile bannerImage)
        {
            string bannerUrl = string.Empty;

            if (bannerImage != null && bannerImage.Length > 0)
            {
                // Read Azure Storage connection string from appsettings.json
                var connectionString = _configuration["AzureStorageConnectionString"] ?? _configuration.GetConnectionString("AzureStorage");
                var containerName = "event-banners";

                if (!string.IsNullOrEmpty(connectionString))
                {
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    // Ensure the container exists and allows public blob read access
                    await containerClient.CreateIfNotExistsAsync();

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(bannerImage.FileName);
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    using (var stream = bannerImage.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    bannerUrl = blobClient.Uri.ToString();
                }
            }

            var @event = new Event
            {
                EventName = eventName,
                Description = description,
                EventDate = eventDate,
                EventLocation = eventLocation,
                Distance = distance,
                EventType = eventType,
                OrganizerName = organizerName,
                BannerUrl = bannerUrl
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEvent", new { id = @event.EventID }, @event);
        }

        // DELETE: api/ApiEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventID == id);
        }
    }
}