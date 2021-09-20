using Microsoft.EntityFrameworkCore;
using RafaelChicovisPortifolio.Models.Administrations.Entities;

namespace RafaelChicovisPortifolio.Contexts
{
    public class PortifolioContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public PortifolioContext(DbContextOptions<PortifolioContext> options) : base(options)
        {
        }
    }
}