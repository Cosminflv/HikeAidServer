namespace PacePalAPI.Utils
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public OperationResult(bool success, T data, string message = "", IEnumerable<string> errors = null)
        {
            Success = success;
            Data = data;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public OperationResult(bool success, string message, IEnumerable<string> errors = null)
        {
            Success = success;
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }
}
