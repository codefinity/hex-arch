Feature: Search users
	As an administrator
	I want to search and page through users
	So that I can find the account I need to act on

Scenario: Searching with no filters asks for the first page
	When I search users with no filters
	Then the request succeeds
	And the search skips 0 users and takes 20
	And the search filters on nothing

Scenario: Filters and paging reach the query
	When I search users with:
		| Search | Role   | Active | Page | PageSize |
		| nik    | Seller | true   | 3    | 10       |
	Then the request succeeds
	And the search skips 20 users and takes 10
	And the search filters on "nik", role "Seller" and active "true"

Scenario: The total across all pages is reported
	Given the user search matches 42 users in total
	When I search users with no filters
	Then the request succeeds
	And the result reports 42 users in total

Scenario Outline: The request is invalid
	When I search users with:
		| Search   | Role | Active | Page   | PageSize   |
		| <search> |      |        | <page> | <pageSize> |
	Then the request fails with the error "<error>"

	Examples:
		| search | page | pageSize | error                                  |
		|        | 0    | 20       | Page must be at least 1.               |
		|        | 1    | 0        | Page size must be between 1 and 100.   |
		|        | 1    | 101      | Page size must be between 1 and 100.   |

Scenario: The search text is too long
	When I search users for text of 201 characters
	Then the request fails with the error "Search must not exceed 200 characters."
