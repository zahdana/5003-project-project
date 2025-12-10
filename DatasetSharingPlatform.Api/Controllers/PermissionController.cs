using DatasetSharingPlatform.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System;
using DatasetSharingPlatform.Api.Data;
using DatasetSharingPlatform.Api.DTOs;
using DatasetSharingPlatform.Api.Helpers;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PermissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. 用户请求权限
    [HttpPost("request")]
    public async Task<IActionResult> RequestPermission([FromBody] PermissionRequestDto dto)
    {
        var userId = JwtHelper.GetUserId(User);
        Console.WriteLine($"权限请求：用户ID = {userId}, 数据集ID = {dto.DatasetId}");
        // 检查是否已存在请求
        bool alreadyRequested = await _context.DownloadPermissionRequests.AnyAsync(r =>
        r.RequesterId == userId &&
        r.DatasetId == dto.DatasetId &&
        (r.Status == "Pending" || r.Status == "Accepted"));


        if (alreadyRequested)
            return BadRequest(new { message = "您已提交过请求" });

        var request = new DownloadPermissionRequest
        {
            RequesterId = userId,
            DatasetId = dto.DatasetId,
            Status = "Pending"
        };

        _context.DownloadPermissionRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(new { message = "请求已提交" });
    }

    [HttpGet("request-status")]
    public async Task<IActionResult> GetPermissionRequestStatus([FromQuery] int datasetId)
    {
        var userId = JwtHelper.GetUserId(User);

        var request = await _context.DownloadPermissionRequests
            .Where(r => r.RequesterId == userId && r.DatasetId == datasetId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        var status = request?.Status ?? "None"; // 如果没有记录，返回 None
        return Ok(new { status });
    }


    // 2. 当前用户的所有权限请求
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = JwtHelper.GetUserId(User);  // 获取当前用户ID

        // 获取用户发出的所有权限请求
        var requests = await _context.DownloadPermissionRequests
            .Where(r => r.RequesterId == userId)
            .Select(r => new
            {
                r.DatasetId,
                DatasetName = r.Dataset.Name,
                DatasetType = r.Dataset.Type,
                OwnerUsername = r.Dataset.User.Username,
                Status = r.Status,
            })
            .ToListAsync();

        return Ok(requests);
    }

    // 3. 上传者收到的权限请求
  
    [HttpGet("received-requests")]
    public async Task<IActionResult> GetReceivedPermissionRequests()
    {
        var userId = JwtHelper.GetUserId(User);
        Console.WriteLine(User);
        var requests = await _context.DownloadPermissionRequests
            .Where(r => r.Dataset.UserId == userId && r.Dataset.DownloadPermission == "restricted")
            .Include(r => r.Dataset)
            .Include(r => r.Requester)
            .Select(r => new
            {
                RequestId = r.Id,
                DatasetId = r.Dataset.Id,
                DatasetName = r.Dataset.Name,
                DatasetType = r.Dataset.Type,
                RequesterUsername = r.Requester.Username,
                Status = r.Status
            })
            .ToListAsync();

        return Ok(requests);
    }


    // 4. 上传者处理请求
    [HttpPost("handle-request")]
    public async Task<IActionResult> HandlePermissionRequest([FromBody] HandlePermissionRequestDto dto)
    {
        var userId = JwtHelper.GetUserId(User);

        var request = await _context.DownloadPermissionRequests
            .Include(r => r.Dataset)
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.Dataset.UserId == userId);

        if (request == null)
            return NotFound(new { message = "权限请求不存在或您无权操作" });

        if (dto.Approve)
        {
            // 检查是否已存在权限，避免重复插入
            bool alreadyGranted = await _context.DatasetPermissions
                .AnyAsync(p => p.DatasetId == request.DatasetId && p.UserId == request.RequesterId);

            if (!alreadyGranted)
            {
                _context.DatasetPermissions.Add(new DatasetPermission
                {
                    DatasetId = request.DatasetId,
                    UserId = request.RequesterId
                });
            }

            request.Status = "Accepted";  // 更新状态
        }
        else
        {
            request.Status = "Rejected";  // 更新状态，不删除
        }


        await _context.SaveChangesAsync();

        return Ok(new { message = dto.Approve ? "已同意权限请求" : "已拒绝权限请求" });
    }

    
 
    // 5. 判断是否有下载权限（可在前端判断时调用）

    [Authorize]
    [HttpGet("check-download-permission")]
    public async Task<IActionResult> CheckDownloadPermission(int datasetId)
    {
        var userId = JwtHelper.GetUserId(User);  // 获取当前用户ID

        var dataset = await _context.Datasets
            .FirstOrDefaultAsync(d => d.Id == datasetId);

        if (dataset == null)
            return NotFound(new { message = "数据集不存在" });

        // 1. 数据集权限为 public
        if (dataset.DownloadPermission == "public")
        {
            return Ok(new { canDownload = true, hasRequested = false });
        }

        // 2. 数据集权限为 private
        if (dataset.DownloadPermission == "private")
        {
            if (dataset.UserId == userId)  // 上传者可以下载
            {
                return Ok(new { canDownload = true, hasRequested = false });
            }
            else
            {
                return Ok(new { canDownload = false, message = "该数据集已被设为私有，无法下载", hasRequested = false });
            }
        }

        // 3. 数据集权限为 restricted
        if (dataset.DownloadPermission == "restricted")
        {
            // ✅ 【新增判断】：如果是上传者本人，也应有权限
            if (dataset.UserId == userId)
            {
                return Ok(new { canDownload = true, hasRequested = false });
            }

            // 不是上传者，再判断是否已授权
            var hasPermission = await _context.DatasetPermissions
                .AnyAsync(p => p.DatasetId == datasetId && p.UserId == userId);

            if (hasPermission)
            {
                return Ok(new { canDownload = true, hasRequested = false });
            }
            else
            {
                // 检查是否已提交权限请求
                var hasRequested = await _context.DownloadPermissionRequests
                 .AnyAsync(r => r.RequesterId == userId && r.DatasetId == datasetId && r.Status == "Pending");

                if (hasRequested)
                {
                    return Ok(new { canDownload = false, message = "该数据集已被设为限制，您已提交权限请求，等待上传者处理", hasRequested = true });
                }

                return Ok(new { canDownload = false, message = "该数据集已被设为限制，您未得到授权，无法下载", hasRequested = false });
            }
        }

        // fallback 返回值
        return Ok(new { canDownload = false, hasRequested = false });
    }

    //获取当前用户的“已通过+restricted”数据集及授权用户列表
    [HttpGet("my-restricted-datasets")]
    [Authorize]
    public async Task<IActionResult> GetMyRestrictedDatasets()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var datasets = await _context.Datasets
            .Where(d => d.UserId == userId && d.Status == "已通过" && d.DownloadPermission == "restricted")
            .Include(d => d.Permissions)
                .ThenInclude(p => p.User)
            .ToListAsync();

        var result = datasets.Select(d => new
        {
            d.Id,
            d.Name,
            d.Type,
            d.UploadTime,
            AuthorizedUsers = d.Permissions.Select(p => new { p.UserId, p.User.Username }).ToList()
        });

        return Ok(result);
    }
    //添加用户授权（上传者给某人授权下载）
    [HttpPost("add-permission")]
    [Authorize]
    public async Task<IActionResult> AddPermission([FromBody] AddPermissionDto dto)
    {
        var uploaderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var dataset = await _context.Datasets
            .FirstOrDefaultAsync(d => d.Id == dto.DatasetId && d.UserId == uploaderId);

        if (dataset == null || dataset.Status != "已通过" || dataset.DownloadPermission != "restricted")
            return BadRequest("无效的数据集");

        var user = await _context.Users.FindAsync(dto.UserId);
        if (user == null)
            return BadRequest("用户不存在");

        var alreadyExists = await _context.DatasetPermissions
            .AnyAsync(p => p.DatasetId == dto.DatasetId && p.UserId == dto.UserId);

        if (alreadyExists)
            return BadRequest("该用户已获得授权");

        var permission = new DatasetPermission
        {
            DatasetId = dto.DatasetId,
            UserId = dto.UserId
        };

        _context.DatasetPermissions.Add(permission);
        await _context.SaveChangesAsync();

        return Ok("授权成功");
    }

    //取消授权
    [HttpPost("remove-permission")]
    public async Task<IActionResult> RemovePermission([FromBody] RemovePermissionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        // 验证当前用户是否为数据集上传者
        var dataset = await _context.Datasets
            .FirstOrDefaultAsync(d => d.Id == dto.DatasetId && d.UserId.ToString() == userId);

        if (dataset == null)
            return Forbid("您无权删除此数据集的授权。");

        var permission = await _context.DatasetPermissions
            .FirstOrDefaultAsync(p => p.DatasetId == dto.DatasetId && p.UserId == dto.UserId);

        if (permission == null)
            return NotFound("授权记录不存在");

        _context.DatasetPermissions.Remove(permission);
        await _context.SaveChangesAsync();

        return Ok("授权已取消");
    }

}
