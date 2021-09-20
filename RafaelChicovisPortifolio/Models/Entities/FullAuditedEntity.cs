using System;

namespace RafaelChicovisPortifolio.Models.Entities
{
    public abstract class FullAuditedEntity
    {
        public virtual bool IsDeleted { get; set; }
        public virtual Guid? DeleterId { get; set; }
        public virtual DateTime? DeletionTime { get; set; }
        public virtual DateTime? LastModificationTime { get; set; }
        public virtual Guid? LastModifierId { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual Guid? CreatorId { get; set; }
        public virtual Guid Id { get; set; }
    }
}