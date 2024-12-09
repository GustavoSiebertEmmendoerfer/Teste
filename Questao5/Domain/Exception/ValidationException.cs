using FluentValidation.Results;

namespace Questao5.Domain.Exception
{
    public class ValidationException : System.Exception
    {
        public ValidationException(string errorType, string message) : base(message)
        {
            ErrorType = errorType;
            Message = message;
        }

        public ValidationException(List<ValidationFailure> validationErrors)
        {
            Message = string.Join(", ", validationErrors.Select(x => x.ErrorMessage));
        }

        public string? ErrorType { get; set; }
        public string Message { get; set; }
    }
}
