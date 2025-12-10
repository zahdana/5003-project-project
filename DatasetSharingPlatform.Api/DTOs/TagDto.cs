using System.Collections.Generic;

namespace DatasetSharingPlatform.Api.DTOs
{
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TagDto> Children { get; set; } = new List<TagDto>();
    }
}
