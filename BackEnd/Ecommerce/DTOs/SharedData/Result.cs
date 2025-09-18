using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SharedData
{
  
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T Value { get; private set; }
        public List<string>? Errors { get; private set; } = new List<string>();


        private Result(bool isSuccess, T value, List<string>? errors)
        {
            IsSuccess = isSuccess;
            Value = value;
            Errors = errors ?? new List<string>();

        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);

        public static Result<T> Failure(params string[] errors) => new Result<T>(false, default, errors.ToList());
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public List<string> Errors { get; }

        protected Result(bool isSuccess, List<string> errors)
        {
            if (isSuccess && errors != null && errors.Any()) throw new InvalidOperationException();
            if (!isSuccess && (errors == null || !errors.Any())) throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Errors = errors ?? new List<string>();
        }

        public static Result Success() => new Result(true, null);
        public static Result Failure(List<string> errors) => new Result(false, errors);
        public static Result Failure(params string[] errors) => new Result(false, errors.ToList());
    }
}
