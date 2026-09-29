Feature: Change password
	As a signed-in user
	I want to change my password
	So that someone who learned my old password can no longer use it

Background:
	Given the clock reads "2026-09-02T09:00:00Z"
	And the account "nikhil@example.com" exists
	And I am signed in to the account "nikhil@example.com"

Scenario: The password is changed
	When I change my password from "Passw0rd!" to "N3wPassw0rd!"
	Then the request succeeds
	And the password of "nikhil@example.com" is now "N3wPassw0rd!"
	And a "UserPasswordChanged" event is published for the account "nikhil@example.com"

Scenario: Changing the password signs out every other session but keeps the caller signed in
	When I change my password from "Passw0rd!" to "N3wPassw0rd!"
	Then the request succeeds
	And every session of "nikhil@example.com" is revoked
	And a fresh token is returned for "nikhil@example.com"

Scenario: Changing the password cancels an outstanding reset code
	Given a password reset code "reset-code" was issued to "nikhil@example.com" that expires on "2026-09-02T10:00:00Z"
	When I change my password from "Passw0rd!" to "N3wPassw0rd!"
	Then the request succeeds
	And the account "nikhil@example.com" holds no password reset code

Scenario: The current password is wrong
	When I change my password from "NotMyPassword!" to "N3wPassw0rd!"
	Then the request fails with the error "The current password is incorrect."
	And the password of "nikhil@example.com" is now "Passw0rd!"
	And no events are published

Scenario Outline: The request is invalid
	When I change my password from "<current>" to "<new>"
	Then the request fails with the error "<error>"

	Examples:
		| current   | new          | error                                                          |
		|           | N3wPassw0rd! | Current password is required.                                  |
		| Passw0rd! | Sh0rt        | Password must be at least 8 characters long.                   |
		| Passw0rd! | Passw0rd!    | The new password must be different from the current password.  |
