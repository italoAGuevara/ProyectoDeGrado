namespace API.Commons
{
    public class ApiResponse<T>
    {
        //public Guid TransactionId { get; set; } = Guid.NewGuid();
        //public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Message { get; set; } = "Operación exitosa";
        public T? Details { get; set; }

        public ApiResponse(string message = "Operación exitosa")
        {
            Message = message;
        }

        public ApiResponse(T? details, string message = "Operación exitosa")
        {
            Details = details;
            Message = message;
        }
    }
}
