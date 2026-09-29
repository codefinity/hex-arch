Feature: User sign in
	As a registered user
	I want to sign in with my email and password
	So that I can access my account

Background:
	Given the sign-in token expires on "2026-08-21T10:30:00Z"
	And the sign-in clock is fixed at "2026-08-20T10:30:00Z"
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

Scenario: Too many wrong passwords in a row lock the account
	When I sign in with email "nikhil@example.com" and the wrong password 5 times
	Then the account "nikhil@example.com" is locked until "2026-08-20T10:45:00Z"
	And a "UserLockedOut" event is published for "nikhil@example.com"

Scenario: Fewer wrong passwords than the limit do not lock the account
	When I sign in with email "nikhil@example.com" and the wrong password 4 times
	Then the account "nikhil@example.com" is not locked
	And no sign-in event is published

Scenario: A locked account cannot sign in even with the correct password
	Given the account "nikhil@example.com" is locked until "2026-08-20T10:45:00Z"
	When I sign in with email "nikhil@example.com" and password "Sup3rSecret!"
	Then the sign in fails with the error "This account is temporarily locked. Try again later."
	And no token is returned

Scenario: A locked account can sign in again once the lock has expired
	Given the account "nikhil@example.com" is locked until "2026-08-20T10:00:00Z"
	When I sign in with email "nikhil@example.com" and password "Sup3rSecret!"
	Then the sign in succeeds
	And the account "nikhil@example.com" is not locked

Scenario: A successful sign in forgets earlier failed attempts
	Given the account "nikhil@example.com" has 4 failed sign-in attempts
	When I sign in with email "nikhil@example.com" and password "Sup3rSecret!"
	Then the sign in succeeds
	And the account "nikhil@example.com" has 0 failed sign-in attempts

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
