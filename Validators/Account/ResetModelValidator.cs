using Delivery.Contracts.Account;
using FluentValidation;

namespace Delivery.Validators.Account
{
    public class ResetModelValidator : AbstractValidator<ResetModel>
    {
        public ResetModelValidator()
        {
            RuleFor(x => x.username)
                .NotEmpty();

            RuleFor(x => x.old_password)
                .NotEmpty();
            
            RuleFor(x => x.new_password)
                .NotEmpty()
                .MinimumLength(6);
            
            RuleFor(x => x.confirm_new_password)
                .Equal(x => x.new_password);
        }
    }
}