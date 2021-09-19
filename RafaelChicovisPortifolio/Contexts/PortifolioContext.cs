using Microsoft.EntityFrameworkCore;

namespace RafaelChicovisPortifolio.Contexts
{
    public class PortifolioContext : DbContext
    {
        public PortifolioContext(DbContextOptions<PortifolioContext> options) : base(options)
        {
        }
    }
}