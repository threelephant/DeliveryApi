using Delivery.Contracts.Account;
using FluentValidation;

namespace Delivery.Validators.Account
{
    public class LoginModelValidator : AbstractValidator<LoginModel>
    {
        public LoginModelValidator()
        {
            RuleFor(x => x.username)
                .NotEmpty();
            RuleFor(x => x.password)
                .NotEmpty();
        }
    }
}