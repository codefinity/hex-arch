Feature: User sign in
	As a registered user
	I want to sign in with my email and password
	So that I can access my account

Background:
	Given the sign-in token expires on "2026-08-21T10:30:00Z"
	And an active account exists for "nikhil@example.com" with password "Sup3rSecret!"

Scenario: A registered user signs in with correct credentials
	When I sign in with email "nikhil@example.com" and password "Sup3rSecret!"
	Then the sign in succeeds
	And a token is returned
	And the token expires on "2026-08-21T10:30:00Z"

Scenario: The password is incorrect
	When I sign in with email "nikhil@example.com" and password "WrongPassword!"
	Then the sign in fails with the error "Invalid email or password."
	And no token is returned

Scenario: The email is not registered
	When I sign in with email "unknown@example.com" and password "Sup3rSecret!"
	Then the sign in fails with the error "Invalid email or password."
	And no token is returned

Scenario: The account is inactive
	Given an inactive account exists for "inactive@example.com" with password "Sup3rSecret!"
	When I sign in with email "inactive@example.com" and password "Sup3rSecret!"
	Then the sign in fails with the error "This account is inactive."
	And no token is returned

Scenario Outline: A required detail is missing or invalid
	When I sign in with email "<email>" and password "<password>"
	Then the sign in fails with the error "<error>"

	Examples:
		| email               | password     | error                       |
		|                     | Sup3rSecret! | A valid email is required.  |
		| not-an-email        | Sup3rSecret! | A valid email is required.  |
		| nikhil@example.com  |              | Password is required.      |

Scenario: Every invalid detail is reported at once
	When I sign in with email "" and password ""
	Then the sign in fails with these errors:
		| Error                       |
		| A valid email is required.  |
		| Password is required.       |
