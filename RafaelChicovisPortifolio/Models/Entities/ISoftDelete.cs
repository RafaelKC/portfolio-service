namespace RafaelChicovisPortifolio.Models.Entities
{
    public interface ISoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}