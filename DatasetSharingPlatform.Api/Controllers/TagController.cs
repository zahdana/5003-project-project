using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatasetSharingPlatform.Api.Data;
using DatasetSharingPlatform.Api.Models;
using DatasetSharingPlatform.Api.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatasetSharingPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TagController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 获取标签树结构：GET /api/tag/tree
        [HttpGet("tree")]
        public async Task<IActionResult> GetTagTree()
        {
            // 查询所有标签并包含其子标签
            var allTags = await _context.Tags
                .Include(t => t.Children)
                .ToListAsync();

            // 构建树结构（从根节点开始）
            var tagTree = BuildTagTree(allTags, null);

            return Ok(tagTree);
        }

        // 递归构建标签树
        private List<TagDto> BuildTagTree(List<Tag> allTags, int? parentId)
        {
            return allTags
                .Where(t => t.ParentId == parentId)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Children = BuildTagTree(allTags, t.Id)
                })
                .ToList();
        }

        /// 获取某个数据集关联的所有标签
        [HttpGet("dataset/{datasetId}/tags")]
        public async Task<IActionResult> GetTagsByDatasetId(int datasetId)
        {
            var tags = await _context.DatasetTags
                .Where(dt => dt.DatasetId == datasetId)
                .Include(dt => dt.Tag)
                .Select(dt => new { dt.Tag.Id, dt.Tag.Name })
                .ToListAsync();

            return Ok(tags);
        }

    }

}
