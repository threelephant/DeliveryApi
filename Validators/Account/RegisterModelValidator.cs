using Delivery.Contracts.Account;
using FluentValidation;

namespace Delivery.Validators.Account
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.username)
                .NotEmpty();
            
            RuleFor(x => x.password)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.confirmPassword)
                .Equal(x => x.password)
                .WithMessage("Пароли не совпадают");
        }
    }
}