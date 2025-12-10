using DatasetSharingPlatform.Api.Data;
using DatasetSharingPlatform.Api.DTOs;
using DatasetSharingPlatform.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DatasetSharingPlatform.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DatasetController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IHostEnvironment _hostEnvironment;
		private readonly ILogger<DatasetController> _logger;

		public DatasetController(ApplicationDbContext context, IHostEnvironment hostEnvironment, ILogger<DatasetController> logger)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
			_logger = logger;
		}

		// 上传数据集
		[HttpPost("upload")]
		public async Task<IActionResult> UploadDataset([FromForm] DatasetUploadDto dto)
		{
			_logger.LogInformation("上传请求到达，文件名称：{FileName}", dto.File?.FileName);

			try
			{
				// 从 JWT 中提取用户 ID
				var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
				if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
				{
					_logger.LogWarning("未找到有效的用户信息");
					return Unauthorized(new { message = "用户身份无效，请重新登录" });
				}

				// 输出提取的 UserId 到控制台日志
				_logger.LogInformation("从 JWT 中提取到的 UserId: {UserId}", userId);

				if (dto.File == null || dto.File.Length == 0)
				{
					_logger.LogWarning("上传失败：文件为空");
					return BadRequest(new { message = "文件不能为空" });
				}

				// 物理目录：ContentRootPath + "Uploads"
				var uploadsFolderName = "Uploads";
				var uploadsPhysicalDir = Path.Combine(_hostEnvironment.ContentRootPath, uploadsFolderName);
				if (!Directory.Exists(uploadsPhysicalDir))
				{
					Directory.CreateDirectory(uploadsPhysicalDir); // 确保文件夹存在
				}

			
				var relativePath = Path.Combine(uploadsFolderName, dto.File.FileName);   // 保存到数据库
				var physicalPath = Path.Combine(_hostEnvironment.ContentRootPath, relativePath); // 实际磁盘路径

				// 保存文件到物理路径
				using (var fileStream = new FileStream(physicalPath, FileMode.Create))
				{
					await dto.File.CopyToAsync(fileStream);
				}

				_logger.LogInformation("文件上传成功，物理路径：{PhysicalPath}，相对路径：{RelativePath}", physicalPath, relativePath);

				// 创建数据集记录（只存相对路径）
				var dataset = new Dataset
				{
					Name = dto.Name,
					Description = dto.Description,
					Type = dto.Type,
					DownloadPermission = dto.DownloadPermission,
					FilePath = relativePath,          // 数据库里不再存绝对路径
					Status = "待审核",                // 初始状态为待审核
					UploadTime = DateTime.UtcNow,
					UserId = userId                   // 使用从 JWT 提取的 UserId
				};

				_context.Datasets.Add(dataset);
				await _context.SaveChangesAsync();

				// 添加标签关联
				if (dto.TagIds != null && dto.TagIds.Count > 0)
				{
					foreach (var tagId in dto.TagIds)
					{
						_context.DatasetTags.Add(new DatasetTag
						{
							DatasetId = dataset.Id,
							TagId = tagId
						});
					}
					await _context.SaveChangesAsync();
				}
				return Ok(new { message = "数据集上传成功，等待审核" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "文件上传失败: {Message}", ex.Message);
				return StatusCode(500, new { message = "上传过程中发生错误，请稍后重试" });
			}
		}

		//根据数据集ID获取详情
		[HttpGet("{id}")]
		public async Task<IActionResult> GetDatasetById(int id)
		{
			var dataset = await _context.Datasets.FindAsync(id);
			if (dataset == null)
			{
				return NotFound(new { message = "数据集未找到" });
			}

			return Ok(dataset);  // 返回数据集详情
		}

		// 获取用户上传的所有数据集
		[HttpGet("my-datasets/{userId}")]
		public async Task<IActionResult> GetMyDatasets(int userId)
		{
			var datasets = await _context.Datasets
				.Where(d => d.UserId == userId && d.Status == "已通过")
				.ToListAsync();

			if (datasets == null || !datasets.Any())
			{
				return NotFound(new { message = "没有找到您的数据集" });
			}

			return Ok(datasets);  // 返回该用户的所有数据集
		}

		// 数据集下载接口
		[HttpGet("download/{datasetId}")]
		[Authorize]
		public async Task<IActionResult> DownloadDataset(int datasetId)
		{
			// 获取数据集记录
			var dataset = await _context.Datasets
				.Where(d => d.Id == datasetId)
				.FirstOrDefaultAsync();

			if (dataset == null)
			{
				_logger.LogWarning("下载失败：未找到数据集 {DatasetId}", datasetId);
				return NotFound(new { message = "数据集未找到" });
			}

			if (string.IsNullOrWhiteSpace(dataset.FilePath))
			{
				_logger.LogWarning("下载失败：数据集 {DatasetId} 的 FilePath 为空", datasetId);
				return NotFound(new { message = "数据集文件不存在" });
			}

		
			// FilePath 为相对路径（Uploads/xxx.csv）
			string physicalPath;

			if (Path.IsPathRooted(dataset.FilePath))
			{

				physicalPath = dataset.FilePath;

				if (!System.IO.File.Exists(physicalPath))
				{
	
					var fileName = Path.GetFileName(dataset.FilePath);
					var fallback = Path.Combine(_hostEnvironment.ContentRootPath, "Uploads", fileName);

					if (System.IO.File.Exists(fallback))
					{
						_logger.LogInformation("旧数据集 {DatasetId} 的绝对路径失效，使用兜底路径：{Fallback}", datasetId, fallback);
						physicalPath = fallback;
					}
					else
					{
						_logger.LogWarning("下载失败：旧式绝对路径和兜底路径均不存在。旧路径：{OldPath}，兜底：{Fallback}", dataset.FilePath, fallback);
						return NotFound(new { message = "数据集文件不存在" });
					}
				}
			}
			else
			{
				physicalPath = Path.Combine(_hostEnvironment.ContentRootPath, dataset.FilePath);

				if (!System.IO.File.Exists(physicalPath))
				{
					_logger.LogWarning("下载失败：相对路径文件不存在 {FilePath} => {PhysicalPath}", dataset.FilePath, physicalPath);
					return NotFound(new { message = "数据集文件不存在" });
				}
			}

			// 记录下载行为
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim != null)
			{
				int userId = int.Parse(userIdClaim.Value);

				var downloadRecord = new DatasetDownloadRecord
				{
					UserId = userId,
					DatasetId = datasetId,
					DownloadTime = DateTime.UtcNow
				};

				_context.DatasetDownloadRecords.Add(downloadRecord);
				await _context.SaveChangesAsync();
			}
			else
			{
				_logger.LogWarning("无法记录下载：未找到用户ID");
			}

			// 读取文件
			var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

			// 根据扩展名选择合适的 Content-Type
			var ext = Path.GetExtension(physicalPath).ToLower();
			var contentType = ext switch
			{
				".csv" => "text/csv",
				".txt" => "text/plain",
				".zip" => "application/zip",
				_ => "application/octet-stream"
			};

			var downloadName = Path.GetFileName(physicalPath);

			// 返回文件下载
			return File(fileBytes, contentType, downloadName);
		}

		//返回所有待审核数据集
		[HttpGet("pending")]
		public async Task<IActionResult> GetPendingDatasets()
		{
			var pendingDatasets = await _context.Datasets
				.Where(d => d.Status == "待审核")
				.Include(d => d.User)
				.Select(d => new
				{
					d.Id,
					d.Name,
					d.Type,
					d.UploadTime,
					UploaderUsername = d.User.Username
				})
				.ToListAsync();

			return Ok(pendingDatasets);
		}

		// 审核数据集
		[HttpPost("audit/{datasetId}")]
		public async Task<IActionResult> AuditDataset(int datasetId, [FromBody] AuditDatasetDto dto)
		{
			var dataset = await _context.Datasets.FindAsync(datasetId);

			if (dataset == null)
			{
				return NotFound(new { message = "数据集未找到" });
			}

			// 更新数据集状态
			dataset.Status = dto.Action == "approve" ? "已通过" : "已拒绝";  // 修正错误，检查Action字段
			_context.Datasets.Update(dataset);

			// 添加审核记录
			var auditLog = new AuditLog
			{
				DatasetId = datasetId,
				ReviewerName = "管理员",  // 可以根据需要修改为动态获取管理员名称
				Action = dto.Action == "approve" ? "通过" : "拒绝",
				Timestamp = DateTime.UtcNow,
				Comment = dto.Comment
			};
			_context.AuditLogs.Add(auditLog);

			await _context.SaveChangesAsync();

			return Ok(new { message = "审核成功" });
		}

		//返回审核记录
		[HttpGet("audit-records")]
		public async Task<IActionResult> GetAuditRecords()
		{
			var auditLogs = await _context.AuditLogs
				.Include(a => a.Dataset) // 包含数据集信息
				.ToListAsync();

			return Ok(auditLogs);
		}

		// 获取并筛选已通过的数据集列表
		[HttpGet("approved")]
		public async Task<IActionResult> GetApprovedDatasets(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10,
		[FromQuery] string type = "all",
		[FromQuery] string query = "",
		[FromQuery] string permission = "all",
		[FromQuery] string dateRange = "all",
		[FromQuery] string uploader = "",
		[FromQuery] List<int> tagIds = null
		)
		{
			var skip = (page - 1) * pageSize;

			var datasetsQuery = _context.Datasets
				.Include(ds => ds.User)
				.Include(ds => ds.DatasetTags)
					 .ThenInclude(dt => dt.Tag)
				.Where(ds => ds.Status == "已通过");

			// 筛选类型
			if (type != "all")
			{
				datasetsQuery = datasetsQuery.Where(ds => ds.Type == type);
			}

			// 关键词搜索
			if (!string.IsNullOrEmpty(query))
			{
				datasetsQuery = datasetsQuery.Where(ds => ds.Name.Contains(query));
			}

			// 权限筛选
			if (permission != "all")
			{
				datasetsQuery = datasetsQuery.Where(ds => ds.DownloadPermission == permission);
			}

			// 上传时间范围筛选
			if (dateRange == "7")
			{
				var weekAgo = DateTime.Now.AddDays(-7);
				datasetsQuery = datasetsQuery.Where(ds => ds.UploadTime >= weekAgo);
			}
			else if (dateRange == "30")
			{
				var monthAgo = DateTime.Now.AddDays(-30);
				datasetsQuery = datasetsQuery.Where(ds => ds.UploadTime >= monthAgo);
			}

			// 上传者用户名模糊搜索
			if (!string.IsNullOrEmpty(uploader))
			{
				datasetsQuery = datasetsQuery.Where(ds => ds.User.Username.Contains(uploader));
			}
			// 标签 “或” 关系筛选
			if (tagIds != null && tagIds.Any())
			{
				datasetsQuery = datasetsQuery.Where(ds =>
					ds.DatasetTags.Any(dt => tagIds.Contains(dt.TagId))
				);
			}
			Console.WriteLine("接收到的标签 ID：" + (tagIds != null ? string.Join(",", tagIds) : "null"));
			// 总数和分页
			var totalCount = await datasetsQuery.CountAsync();
			var datasets = await datasetsQuery
				.OrderByDescending(ds => ds.UploadTime)
				.Skip(skip)
				.Take(pageSize)
				.Select(ds => new
				{
					ds.Id,
					ds.Name,
					ds.Type,
					ds.DownloadPermission,
					UploadTime = ds.UploadTime.ToString("yyyy-MM-dd"),
					Username = ds.User.Username,
					Tags = ds.DatasetTags
						.Select(dt => new { dt.Tag.Id, dt.Tag.Name })
						.ToList()
				})
				.ToListAsync();

			var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			return Ok(new
			{
				items = datasets,
				totalPages
			});
		}

		//筛选当前登录用户的数据集
		[HttpGet("user/self")]
		[Authorize]
		public async Task<IActionResult> GetMyDatasets([FromQuery] DatasetFilterDto filter)
		{
			var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
			if (userIdClaim == null)
			{
				return Unauthorized("用户未登录");
			}

			int userId = int.Parse(userIdClaim.Value);

			var query = _context.Datasets
				.Where(d => d.UserId == userId);

			if (!string.IsNullOrEmpty(filter.Type))
				query = query.Where(d => d.Type == filter.Type);

			if (!string.IsNullOrEmpty(filter.Status))
				query = query.Where(d => d.Status == filter.Status);

			if (!string.IsNullOrEmpty(filter.NameKeyword))
				query = query.Where(d => d.Name.Contains(filter.NameKeyword));

			var totalCount = await query.CountAsync();

			var datasets = await query
				.OrderByDescending(d => d.UploadTime)
				.Skip((filter.Page - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.Select(d => new
				{
					d.Id,
					d.Name,
					d.Description,
					d.Type,
					d.Status,
					d.UploadTime
				})
				.ToListAsync();

			return Ok(new { TotalCount = totalCount, Items = datasets });
		}

		// 添加评论到指定数据集
		[HttpPost("{datasetId}/add-comments")]
		[Authorize]
		public async Task<IActionResult> AddCommentToDataset(int datasetId, [FromBody] AddCommentDto dto)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			if (datasetId != dto.DatasetId)
				return BadRequest("数据集 ID 不匹配");

			var comment = new Comment
			{
				DatasetId = dto.DatasetId,
				UserId = userId,
				Content = dto.Content,
				CreatedAt = DateTime.Now
			};

			_context.Comments.Add(comment);
			await _context.SaveChangesAsync();

			return Ok(new { message = "评论已添加" });
		}

		// 获取指定数据集的所有评论
		[HttpGet("{datasetId}/comments")]
		public async Task<ActionResult<List<CommentDto>>> GetCommentsForDataset(int datasetId)
		{
			var comments = await _context.Comments
				.Where(c => c.DatasetId == datasetId)
				.OrderByDescending(c => c.CreatedAt)
				.Include(c => c.User)
				.Select(c => new CommentDto
				{
					Id = c.Id,
					Content = c.Content,
					CreatedAt = c.CreatedAt,
					Username = c.User.Username,
					UserId = c.UserId
				})
				.ToListAsync();

			return Ok(comments);
		}

		// 删除指定评论（仅限作者本人）
		[HttpDelete("comments/{commentId}")]
		[Authorize]
		public async Task<IActionResult> DeleteCommentFromDataset(int commentId)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			var comment = await _context.Comments.FindAsync(commentId);
			if (comment == null)
				return NotFound();

			if (comment.UserId != userId)
				return Forbid();

			_context.Comments.Remove(comment);
			await _context.SaveChangesAsync();

			return Ok(new { message = "评论已删除" });
		}
	}
}
