using System.Net;
namespace RoyalVilla.DTO
{
    public class ApiResponseDTO<TData>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; } = string.Empty;
        public TData? Data { get; set; }        
        public object? Errors { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public static ApiResponseDTO<TData> Create(bool success, int statusCode, string message, object? errors = null, TData? data = default)
        {
            return new ApiResponseDTO<TData>()
            {
                Success = success,
                StatusCode = statusCode,
                Message = message,
                Errors = errors,
                Data = data
            };
        }

        public static ApiResponseDTO<TData> Okay(string message, TData data) => Create(true, 200, message, data:data);
        public static ApiResponseDTO<TData> BadRequst(object error) => Create(false, 400, "Bad Request", error);
        public static ApiResponseDTO<TData> NotFound(object error) => Create(false, 404, "Not Found", error);
        public static ApiResponseDTO<TData> Conflict(object error) => Create(false, 409, "Conflict");
        public static ApiResponseDTO<TData> CreatedAt(string message, TData data) => Create(true, 201, message, data:data);
        public static ApiResponseDTO<TData> NoContent(string message) => Create(true, 209, message);
    }

}
