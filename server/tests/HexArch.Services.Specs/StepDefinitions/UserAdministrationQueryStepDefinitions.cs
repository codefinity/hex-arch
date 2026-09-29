using HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    // SearchUsers and ShowUser: the administrator's read side.
    [Binding]
    public class UserAdministrationQueryStepDefinitions
    {
        private readonly AccountContext context;
        private SearchUsersResult? searchResult;
        private ShowUserResult? showResult;

        public UserAdministrationQueryStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [Given(@"the user search matches (\d+) users in total")]
        public void GivenTheUserSearchMatchesUsersInTotal(int total) =>
            context.UserSearch.Page = new UserSearchPage(Array.Empty<UserSummaryReadModel>(), total);

        [Given(@"the user ""([^""]*)"" has a projected view with the roles ""([^""]*)""")]
        public void GivenTheUserHasAProjectedViewWithTheRoles(string email, string roles)
        {
            var userId = Guid.NewGuid();
            context.AccountIds[email] = userId;
            context.UserProfiles.Seed(new UserProfileReadModel(
                userId, "Existing User", email, "0000000000", true, DateTime.UtcNow,
                AccountStepDefinitions.ParseList(roles).Select(name => new RoleReadModel(Guid.NewGuid(), name)).ToArray(),
                null, null, null, null, null, DateTime.UtcNow,
                EmailVerified: true,
                DeactivationReason: null,
                DeactivatedOn: null,
                ClosedOn: null));
        }

        [When(@"I search users with no filters")]
        public Task WhenISearchUsersWithNoFilters() => Search(new SearchUsersQuery());

        [When(@"I search users with:")]
        public Task WhenISearchUsersWith(DataTable table)
        {
            var row = table.Rows[0];
            return Search(new SearchUsersQuery(
                NullIfEmpty(row["Search"]),
                NullIfEmpty(row["Role"]),
                string.IsNullOrEmpty(row["Active"]) ? null : bool.Parse(row["Active"]),
                int.Parse(row["Page"]),
                int.Parse(row["PageSize"])));
        }

        [When(@"I search users for text of (\d+) characters")]
        public Task WhenISearchUsersForTextOfCharacters(int length) =>
            Search(new SearchUsersQuery(Search: new string('a', length)));

        [When(@"an administrator views the user ""([^""]*)""")]
        public Task WhenAnAdministratorViewsTheUser(string email) => Show(context.AccountIds[email]);

        [When(@"an administrator views an unknown user")]
        public Task WhenAnAdministratorViewsAnUnknownUser() => Show(Guid.NewGuid());

        [When(@"an administrator views the user with an empty id")]
        public Task WhenAnAdministratorViewsTheUserWithAnEmptyId() => Show(Guid.Empty);

        [Then(@"the search skips (\d+) users and takes (\d+)")]
        public void ThenTheSearchSkipsUsersAndTakes(int offset, int limit)
        {
            var criteria = context.UserSearch.LastCriteria.ShouldNotBeNull();
            criteria.Offset.ShouldBe(offset);
            criteria.Limit.ShouldBe(limit);
        }

        [Then(@"the search filters on nothing")]
        public void ThenTheSearchFiltersOnNothing()
        {
            var criteria = context.UserSearch.LastCriteria.ShouldNotBeNull();
            criteria.Search.ShouldBeNull();
            criteria.Role.ShouldBeNull();
            criteria.Active.ShouldBeNull();
        }

        [Then(@"the search filters on ""([^""]*)"", role ""([^""]*)"" and active ""([^""]*)""")]
        public void ThenTheSearchFiltersOn(string search, string role, string active)
        {
            var criteria = context.UserSearch.LastCriteria.ShouldNotBeNull();
            criteria.Search.ShouldBe(search);
            criteria.Role.ShouldBe(role);
            criteria.Active.ShouldBe(bool.Parse(active));
        }

        [Then(@"the result reports (\d+) users in total")]
        public void ThenTheResultReportsUsersInTotal(int total)
        {
            searchResult.ShouldNotBeNull();
            searchResult.TotalCount.ShouldBe(total);
        }

        [Then(@"the user shown has the email ""([^""]*)""")]
        public void ThenTheUserShownHasTheEmail(string email) => RequireShownUser().Email.ShouldBe(email);

        [Then(@"the user shown holds the roles ""([^""]*)""")]
        public void ThenTheUserShownHoldsTheRoles(string roles) =>
            RequireShownUser().Roles.Select(role => role.Name).ShouldBe(AccountStepDefinitions.ParseList(roles), ignoreOrder: true);

        private async Task Search(SearchUsersQuery query)
        {
            searchResult = await context.SearchUsers.Handle(query, CancellationToken.None);
            context.RecordOutcome(searchResult.Success, searchResult.Errors);
        }

        private async Task Show(Guid userId)
        {
            showResult = await context.ShowUser.Handle(new ShowUserQuery(userId), CancellationToken.None);
            context.RecordOutcome(showResult.Success, showResult.Errors);
        }

        private UserProfileReadModel RequireShownUser()
        {
            showResult.ShouldNotBeNull();
            return showResult.User.ShouldNotBeNull();
        }

        private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;
    }
}
