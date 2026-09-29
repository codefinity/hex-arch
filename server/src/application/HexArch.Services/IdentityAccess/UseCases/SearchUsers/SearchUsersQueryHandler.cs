using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers;
using HexArch.Services.IdentityAccess.Ports.Output.Queries;

namespace HexArch.Services.IdentityAccess.UseCases.SearchUsers
{
    public class SearchUsersQueryHandler : ISearchUsersQueryHandler
    {
        private readonly IValidator<SearchUsersQuery> validator;
        private readonly IUserSearchQuery userSearchQuery;

        public SearchUsersQueryHandler(IValidator<SearchUsersQuery> validator, IUserSearchQuery userSearchQuery)
        {
            this.validator = validator;
            this.userSearchQuery = userSearchQuery;
        }

        public async Task<SearchUsersResult> Handle(SearchUsersQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return SearchUsersResult.Failed(errors);
            }

            // Reads the projected rows only, like ShowUserProfile: a change whose projection has not
            // landed yet is not visible here until reconciliation repairs it.
            var criteria = new UserSearchCriteria(
                string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
                string.IsNullOrWhiteSpace(query.Role) ? null : query.Role,
                query.Active,
                Offset: (query.Page - 1) * query.PageSize,
                Limit: query.PageSize);

            var page = await userSearchQuery.SearchUsers(criteria, cancellationToken);

            return SearchUsersResult.Succeeded(page, query.Page, query.PageSize);
        }
    }
}
