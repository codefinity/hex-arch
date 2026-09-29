using FluentValidation;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers;

namespace HexArch.Services.IdentityAccess.UseCases.SearchUsers
{
    public class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
    {
        public const int MaxPageSize = 100;

        public SearchUsersQueryValidator()
        {
            RuleFor(query => query.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be at least 1.");

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, MaxPageSize)
                .WithMessage($"Page size must be between 1 and {MaxPageSize}.");

            RuleFor(query => query.Search)
                .MaximumLength(200)
                .WithMessage("Search must not exceed 200 characters.");

            RuleFor(query => query.SellerApplicationStatus)
                // Names only: Enum.TryParse would also accept numeric strings such as "1".
                .Must(status => Enum.GetNames<SellerApplicationStatus>().Contains(status))
                .When(query => !string.IsNullOrWhiteSpace(query.SellerApplicationStatus))
                .WithMessage("Seller application status must be Pending, Approved or Rejected.");
        }
    }
}
