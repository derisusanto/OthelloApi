using System.Collections.Generic;

namespace OthelloAPI.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = "";
        public T Data { get; private set; }

        private ServiceResult(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ServiceResult<T> Ok(T data) => new(true, "", data);
        public static ServiceResult<T> Fail(string message) => new(false, message, default);
    }
}
