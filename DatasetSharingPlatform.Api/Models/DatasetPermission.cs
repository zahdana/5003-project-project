using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatasetSharingPlatform.Api.Models
{
    public class DatasetPermission
    {
        public int Id { get; set; }

        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }

}
