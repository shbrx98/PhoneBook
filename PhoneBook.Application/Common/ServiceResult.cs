namespace PhoneBook.Application.DTOs
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static ServiceResult Ok(string message = "عملیات با موفقیت انجام شد")
            => new() { Success = true, Message = message };

        public static ServiceResult Fail(string error)
            => new() { Success = false, Errors = new List<string> { error } };

        public static ServiceResult Fail(List<string> errors)
            => new() { Success = false, Errors = errors };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "عملیات با موفقیت انجام شد")
            => new() { Success = true, Data = data, Message = message };

        public new static ServiceResult<T> Fail(string error)
            => new() { Success = false, Errors = new List<string> { error } };

        public new static ServiceResult<T> Fail(List<string> errors)
            => new() { Success = false, Errors = errors };
    }
}