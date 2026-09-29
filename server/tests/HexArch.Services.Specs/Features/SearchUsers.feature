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
		| Search | Role   | Active | SellerApplicationStatus | Page | PageSize |
		| nik    | Seller | true   | Pending                 | 3    | 10       |
	Then the request succeeds
	And the search skips 20 users and takes 10
	And the search filters on "nik", role "Seller", active "true" and seller application status "Pending"

Scenario: The total across all pages is reported
	Given the user search matches 42 users in total
	When I search users with no filters
	Then the request succeeds
	And the result reports 42 users in total

Scenario Outline: The request is invalid
	When I search users with:
		| Search | Role | Active | SellerApplicationStatus   | Page   | PageSize   |
		|        |      |        | <sellerApplicationStatus> | <page> | <pageSize> |
	Then the request fails with the error "<error>"

	Examples:
		| page | pageSize | sellerApplicationStatus | error                                                             |
		| 0    | 20       |                         | Page must be at least 1.                                          |
		| 1    | 0        |                         | Page size must be between 1 and 100.                              |
		| 1    | 101      |                         | Page size must be between 1 and 100.                              |
		| 1    | 20       | Maybe                   | Seller application status must be Pending, Approved or Rejected.  |
		| 1    | 20       | 1                       | Seller application status must be Pending, Approved or Rejected.  |
