namespace DatasetSharingPlatform.Api.Models
{
    public class DatasetTag
    {
        public int DatasetId { get; set; }
        public int TagId { get; set; }

        public Dataset Dataset { get; set; }
        public Tag Tag { get; set; }
    }
}
