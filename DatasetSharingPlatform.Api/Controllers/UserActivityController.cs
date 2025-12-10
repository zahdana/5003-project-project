using DatasetSharingPlatform.Api.Data;
using DatasetSharingPlatform.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DatasetSharingPlatform.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserActivityController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("record-view")]
        [Authorize]
        public async Task<IActionResult> RecordView([FromBody] int datasetId)
        {
            if (datasetId <= 0)
                return BadRequest(new { message = "无效的数据集ID" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { message = "无效的用户身份" });

            int userId = int.Parse(userIdClaim.Value);

            var existingRecord = await _context.DatasetViewRecords
                .FirstOrDefaultAsync(r => r.UserId == userId && r.DatasetId == datasetId);

            if (existingRecord == null)
            {
                _context.DatasetViewRecords.Add(new DatasetViewRecord
                {
                    UserId = userId,
                    DatasetId = datasetId,
                    ViewTime = DateTime.Now,
                    ViewCount = 1  // 第一次浏览，计数为1
                });
                
            }
            else
            {
                // 有记录，更新 ViewTime
                existingRecord.ViewTime = DateTime.Now;
                existingRecord.ViewCount += 1;
                _context.DatasetViewRecords.Update(existingRecord);
            }
            await _context.SaveChangesAsync();

            return Ok();
        }


        [Authorize]
        [HttpGet("view-records")]
        public async Task<IActionResult> GetViewRecords([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("分页参数必须大于 0。");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("未找到有效的用户身份信息。");
            }

            var query = _context.DatasetViewRecords
                .Where(r => r.UserId == userId)
                .Include(r => r.Dataset)
                    .ThenInclude(d => d.User);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return Ok(new
                {
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Records = new List<object>()
                });
            }

            var records = await query
                .OrderByDescending(r => r.ViewTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    DatasetId = r.Dataset.Id,
                    DatasetName = r.Dataset.Name,
                    DatasetType = r.Dataset.Type,
                    DatasetPermission = r.Dataset.DownloadPermission,
                    UploaderUsername = r.Dataset.User.Username,
                    ViewTime = r.ViewTime
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Records = records
            });
        }




        [Authorize]
        [HttpGet("download-records")]
        public async Task<IActionResult> GetDownloadRecords([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("未找到用户身份信息。");
            }

            var query = _context.DatasetDownloadRecords
                .Where(r => r.UserId == int.Parse(userId))
                .Include(r => r.Dataset)
                    .ThenInclude(d => d.User);

            var totalCount = await query.CountAsync();

            var records = await query
                .OrderByDescending(r => r.DownloadTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    DatasetName = r.Dataset.Name,
                    DatasetType = r.Dataset.Type,
                    DatasetPermission = r.Dataset.DownloadPermission,
                    UploaderUsername = r.Dataset.User.Username,
                    DownloadTime = r.DownloadTime
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Records = records
            });
        }

        // 获取数据集活动统计信息
        [HttpGet("dataset-activity-stats")]
        public async Task<IActionResult> GetDatasetActivityStats([FromQuery] string datasetIds)
        {
            if (string.IsNullOrEmpty(datasetIds))
            {
                return BadRequest("参数 datasetIds 不能为空");
            }

            // 将 datasetIds 按逗号分割成数组
            var ids = datasetIds.Split(',').Select(idStr =>
            {
                if (int.TryParse(idStr, out int id))
                    return (int?)id;
                return null;
            }).Where(id => id.HasValue).Select(id => id.Value).ToList();

            if (ids.Count == 0)
            {
                return BadRequest("参数格式不正确");
            }

            // 查询浏览记录
            var viewData = await _context.DatasetViewRecords
                .Where(r => ids.Contains(r.DatasetId))
                .GroupBy(r => r.DatasetId)
                .Select(g => new
                {
                    DatasetId = g.Key,
                    ViewCount = g.Sum(r => r.ViewCount)
                })
                .ToListAsync();

            // 查询下载记录
            var downloadData = await _context.DatasetDownloadRecords
                .Where(r => ids.Contains(r.DatasetId))
                .GroupBy(r => r.DatasetId)
                .Select(g => new
                {
                    DatasetId = g.Key,
                    DownloadCount = g.Count()
                })
                .ToListAsync();

            // 合并浏览和下载信息
            var result = ids.Select(id => new
            {
                Id = id,
                ViewCount = viewData.FirstOrDefault(v => v.DatasetId == id)?.ViewCount ?? 0,
                DownloadCount = downloadData.FirstOrDefault(d => d.DatasetId == id)?.DownloadCount ?? 0
            }).ToList();

            return Ok(result);
        }


        /// <summary>
        /// 获取个性化推荐数据集
        /// </summary>
        /// <returns>个性化推荐的数据集列表</returns>
        [HttpGet("personalized-recommendations")]
        [Authorize] // 确保用户已授权
        public async Task<IActionResult> GetPersonalizedRecommendations()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            if (userIdClaim == null)
            {
                Console.WriteLine("未能成功获取UserId UserID为空");
                return Unauthorized("用户未登录");
            }
            Console.WriteLine($"生成推荐数据集中，UserIdClaim: {userIdClaim}");
            var userId = int.Parse(userIdClaim);
            Console.WriteLine($"生成推荐数据集中，UserId: {userId}");
            // Step 1: 获取用户的浏览记录和下载记录的 DatasetId
            var viewedDatasetIds = await _context.DatasetViewRecords
                .Where(r => r.UserId == userId)
                .Select(r => r.DatasetId)
                .Distinct()
                .ToListAsync();

            var downloadedDatasetIds = await _context.DatasetDownloadRecords
                .Where(r => r.UserId == userId)
                .Select(r => r.DatasetId)
                .Distinct()
                .ToListAsync();
            Console.WriteLine("-----------viewedDatasetIds---------------: " + string.Join(", ", viewedDatasetIds));
            Console.WriteLine("-----------downloadedDatasetIds-----------: " + string.Join(", ", downloadedDatasetIds));
            var interactedDatasetIds = viewedDatasetIds.Union(downloadedDatasetIds).Distinct().ToList();

            if (!interactedDatasetIds.Any())
            {
                // 如果用户没有任何浏览或下载记录，返回最新的通过审核且公开的数据集
                var fallbackDatasets = await _context.Datasets
                    .Where(d => d.Status == "已通过" && d.DownloadPermission == "公开")
                    .OrderByDescending(d => d.UploadTime)
                    .Take(10)
                    .Select(d => new {
                        d.Id,
                        d.Name,
                        d.Description,
                        d.Type,
                        d.UploadTime,
                        UploaderUsername = d.User.Username
                    })
                    .ToListAsync();
                Console.WriteLine("返回最新的通过审核且公开的数据集如下：");
                foreach (var dataset in fallbackDatasets)
                {
                    Console.WriteLine($"Id: {dataset.Id}, Name: {dataset.Name}, Type: {dataset.Type}, UploadTime: {dataset.UploadTime}, Uploader: {dataset.UploaderUsername}");
                }
                return Ok(fallbackDatasets);
            }

            // Step 2: 获取用户交互过的数据集的详细信息
            var interactedDatasets = await _context.Datasets
                .Where(d => interactedDatasetIds.Contains(d.Id))
                .ToListAsync();

            var preferredTypes = interactedDatasets.Select(d => d.Type).Distinct().ToList();
            var preferredKeywords = interactedDatasets
                .SelectMany(d => (d.Name + " " + d.Description).Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .GroupBy(word => word.ToLower())
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToList();
            // 打印 preferredTypes
            Console.WriteLine("用户偏好的数据集类型（preferredTypes）:");
            foreach (var type in preferredTypes)
            {
                Console.WriteLine($" - {type}");
            }

            // 打印 preferredKeywords
            Console.WriteLine("用户偏好的关键词（preferredKeywords）:");
            foreach (var keyword in preferredKeywords)
            {
                Console.WriteLine($" - {keyword}");
            }

            // Step 3: 获取符合条件的候选数据集
            var candidateDatasets = await _context.Datasets
                .Include(d => d.User)
                .Where(d => d.Status == "已通过" && !interactedDatasetIds.Contains(d.Id))  //不是用户之前已经浏览过或下载过的
                .ToListAsync();
            foreach (var dataset in candidateDatasets)
            {
                Console.WriteLine($"候选数据集 -> Id: {dataset.Id}, Name: {dataset.Name}, Type: {dataset.Type}, UploadTime: {dataset.UploadTime}");
            }
            // Step 4: 根据相似度对候选数据集进行排序
            var recommendedDatasets = candidateDatasets
                .Select(d => new
                {
                    Dataset = d,
                    Score = CalculateSimilarityScore(d, preferredTypes, preferredKeywords)
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .Select(x => new {
                    x.Dataset.Id,
                    x.Dataset.Name,
                    x.Dataset.Description,
                    x.Dataset.Type,
                    x.Dataset.UploadTime,
                    UploaderUsername = x.Dataset.User.Username
                })
                .ToList();

            return Ok(recommendedDatasets);
        }

        /// <summary>
        /// 计算数据集的相似度分数
        /// </summary>
        /// <param name="dataset">数据集对象</param>
        /// <param name="preferredTypes">用户偏好的数据集类型列表</param>
        /// <param name="preferredKeywords">用户偏好的关键词列表</param>
        /// <returns>数据集的相似度分数</returns>
        private double CalculateSimilarityScore(Dataset dataset, List<string> preferredTypes, List<string> preferredKeywords)
        {
            double score = 0.0;

            // Step 1: Type匹配
            if (preferredTypes.Contains(dataset.Type))
            {
                score += 2.0; // 如果数据集类型与用户偏好匹配，增加较高分数
            }

            // Step 2: 根据Name和Description中出现的关键词增加分数
            var text = (dataset.Name + " " + dataset.Description).ToLower();
            foreach (var keyword in preferredKeywords)
            {
                if (text.Contains(keyword))
                {
                    score += 1.0; // 每个匹配的关键词增加一定分数
                }
            }

            return score;
        }
    }

}
