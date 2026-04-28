namespace ClinicHub.Application.Common.Exceptions
{
    public class BadRequestException: Exception
    {
        public IDictionary<string, string[]> Errors { get; }
        public BadRequestException()
            : base()
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>() { { "", new string[] { message } } };
        }

        public BadRequestException(string[] errors)
        {
            Errors = new Dictionary<string, string[]>() { { "", errors } };
        }
        public BadRequestException(IDictionary<string, string[]> error)
        {
            Errors = error;
        }
    }
}
