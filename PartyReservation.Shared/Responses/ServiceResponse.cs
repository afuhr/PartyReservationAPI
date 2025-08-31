namespace PartyReservation.Shared.Responses
{
    public enum ErrorCode
    {
        None = 0,
        ValidationError = 1,
        DatabaseError = 4,
        UnexpectedError = 99
    }
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Success => ErrorCode == ErrorCode.None;

        public ErrorCode ErrorCode { get; set; }  = ErrorCode.None;

        public static ServiceResponse<T> Ok(T data) =>
            new ServiceResponse<T> { Data = data };

        public static ServiceResponse<T> Fail(string message, ErrorCode errorCode) =>
            new ServiceResponse<T> { ErrorMessage = message, ErrorCode = errorCode };
    }
}
