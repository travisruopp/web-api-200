namespace HelpDesk.Vip.Api.VIP;

using Microsoft.AspNetCore.Mvc;
using Marten;
using System;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("vips")]
public class VIPsController : ControllerBase
{
    private readonly IDocumentSession _session;

    public VIPsController(IDocumentSession session)
    {
        _session = session;
    }

    // POST /vips
    [HttpPost]
    public async Task<IActionResult> AddVIP([FromBody] VIP vip)
    {
        if (string.IsNullOrWhiteSpace(vip.Sub) || string.IsNullOrWhiteSpace(vip.Description))
            return BadRequest(new { Message = "Sub and Description are required." });

        if (vip.Description.Length < 10 || vip.Description.Length > 500)
            return BadRequest(new { Message = "Description must be between 10 and 500 characters." });

        var existingVIP = await _session.Query<VIP>()
            .FirstOrDefaultAsync(v => v.Sub == vip.Sub);

        if (existingVIP != null)
        {
            if (!existingVIP.IsRemoved)
                return Conflict(new { Message = "VIP already exists." });

            // Reactivate the removed VIP
            existingVIP.IsRemoved = false;
            existingVIP.Description = vip.Description;
            existingVIP.AddedOn = DateTime.UtcNow;

            _session.Store(existingVIP);
            await _session.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVIP), new { id = existingVIP.Id }, existingVIP);
        }

        // Add new VIP
        _session.Store(vip);
        await _session.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVIP), new { id = vip.Id }, vip);
    }

    // GET /vips/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVIP(Guid id)
    {
        var vip = await _session.LoadAsync<VIP>(id);

        if (vip == null || vip.IsRemoved)
            return NotFound();

        return Ok(vip);
    }

    // GET /vips
    [HttpGet]
    public async Task<IActionResult> GetAllVIPs()
    {
        var vips = await _session.Query<VIP>()
            .Where(v => !v.IsRemoved)
            .ToListAsync();

        return Ok(vips);
    }

    // DELETE /vips/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveVIP(Guid id)
    {
        var vip = await _session.LoadAsync<VIP>(id);

        if (vip == null || vip.IsRemoved)
            return NoContent();

        vip.IsRemoved = true;

        _session.Store(vip);
        await _session.SaveChangesAsync();

        return NoContent();
    }
}