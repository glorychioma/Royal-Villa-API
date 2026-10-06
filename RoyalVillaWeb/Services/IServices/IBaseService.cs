using RoyalVilla.DTO;
using RoyalVillaWeb.Models;

namespace RoyalVillaWeb.Services.IServices
{
    public interface IBaseService
    {
        ApiResponseDTO<object> ResponseModel { get; set; }
        Task<T> SendAsync<T>(ApiRequest apiRequest);
    }
}
