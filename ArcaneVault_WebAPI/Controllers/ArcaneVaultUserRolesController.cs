using ArcaneVault_WebAPI.Data;
using ArcaneVault_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArcaneVaultUserRolesController : ControllerBase
    {
        private readonly ArcaneVaultContext _context;

        public ArcaneVaultUserRolesController(ArcaneVaultContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArcaneVaultUserRole>>> GetArcaneVaultUserRoles()
        {
            return await _context.ArcaneVaultUserRoles.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ArcaneVaultUserRole>> PostArcaneVaultUserRole(ArcaneVaultUserRole role)
        {
            _context.ArcaneVaultUserRoles.Add(role);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArcaneVaultUserRoles), role);
        }
    }
}