using FluentValidation;
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
        }
    }
}
