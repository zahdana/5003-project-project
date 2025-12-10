using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DatasetSharingPlatform.Api.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Tag Parent { get; set; }

        public ICollection<Tag> Children { get; set; }

        public ICollection<DatasetTag> DatasetTags { get; set; }
    }
}
