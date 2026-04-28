using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Common.Models
{
    public class Result
    {
        public bool Succeeded { get; set; }
        public object? MainResult { get; set; }
        public object? Data { get; set; }
        public string[] Errors { get; set; }

        internal Result(bool succeeded, object? mainResult, object? data, IEnumerable<string> errors)
        {
            MainResult = mainResult;
            Succeeded = succeeded;
            Errors = errors.ToArray();
            Data = data;
        }
    }

    public static class IdentityResultMap
    {

        public static Result Success(this IdentityResult mainResult, object data)
        {
            return new Result(true, mainResult, data, System.Array.Empty<string>());
        }

        public static Result Success(this IdentityResult mainResult)
        {
            return new Result(true, mainResult, null, System.Array.Empty<string>());
        }

        public static Result Failure(this IdentityResult mainResult, IEnumerable<string> errors)
        {
            return new Result(false, mainResult, null, errors);
        }
    }

    public static class SginInResultMap
    {

        public static Result Success(this SignInResult mainResult, object data)
        {
            return new Result(true, mainResult, data, System.Array.Empty<string>());
        }

        public static Result Success(this SignInResult mainResult)
        {
            return new Result(true, mainResult, null, System.Array.Empty<string>());
        }

        public static Result Failure(this SignInResult mainResult, IEnumerable<string> errors)
        {
            return new Result(false, mainResult, null, errors);
        }
    }
}
