using Microsoft.AspNetCore.Http;
using Moodify.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moodify.BAL.Interfaces
{
	public  interface  IMusicService
	{
		Task<IEnumerable<SendMusicDto>> GetMusicByCategoryAsync(string category, int pageNumber, int pageSize);
		Task<IEnumerable<SendMusicDto>> GetAllMusicAsync(int pageNumber, int pageSize);
		Task<SendMusicDto?> GetMusicByIdAsync(int id);
		Task<IEnumerable<object>> SearchLocalMusicAsync(string query);
		Task AddMusicAsync(addmusicDTO dto);
		Task<object> HandleFrameUploadAsync(IFormFile file);
	}
}
