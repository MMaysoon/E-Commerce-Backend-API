using System;
using System.Collections.Generic;

namespace E_commerce.Application.Common
{
   
    public class Result
    {
        // 1. Properties
        public bool IsSuccess { get; }
        public IReadOnlyList<Error> Errors { get; }


        // 2. Constructor
        protected Result(
            bool isSuccess,
            IReadOnlyList<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            if (isSuccess && errors.Count > 0)
                throw new ArgumentException("A successful result cannot contain errors.", nameof(errors));

            if (!isSuccess && errors.Count == 0)
                throw new ArgumentException("A failed result must contain at least one error.", nameof(errors));

            IsSuccess = isSuccess;
            Errors = Array.AsReadOnly(errors.ToArray());
        }


        // 3. Method: Success without data
        public static Result Ok()
        {
            return new Result( true, Array.Empty<Error>());
        }

        // 4. Method: Failure with one error
        public static Result Fail(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return new Result(false, new[] { error });
        }


        // 5. Method: Failure with multiple errors
        public static Result Fail(IReadOnlyList<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return new Result(false,errors);
        }
    }


    
    // Result<TValue>
    public class Result<TValue> : Result
    {
        // 1. Field: Stores the returned data
        private readonly TValue _value;


        // 2. Property: Returns data if success
        public TValue Data =>
            IsSuccess
                ? _value
                : throw new InvalidOperationException(
                    "Cannot access data when the result is a failure.");


        // 3. Constructor: Success with data
        private Result(TValue value): base(true, Array.Empty<Error>())
        {
            _value = value;
        }


        // 4. Constructor: Failure with one error
        private Result(Error error): base(false, new[] { error })
        {
            _value = default!;
        }


        // 5. Constructor: Failure with multiple errors
        private Result(IReadOnlyList<Error> errors): base(false, errors)
        {
            _value = default!;
        }


        // 6. Method: Success with data
        public static Result<TValue> Ok(TValue value)
        {
            return new Result<TValue>(value);
        }


        // 7. Method: Failure with one error
        public static new Result<TValue> Fail(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return new Result<TValue>(error);
        }


        // 8. Method: Failure with multiple errors
        public static new Result<TValue> Fail(
            IReadOnlyList<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return new Result<TValue>(errors);
        }


        // 9. Implicit conversion: TValue → Result<TValue>
        public static implicit operator Result<TValue>(TValue value)
        {
            return Ok(value);
        }


        // 10. Implicit conversion: Error → Result<TValue>
        public static implicit operator Result<TValue>(Error error)
        {
            return Fail(error);
        }
    }
}