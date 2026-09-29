Feature: Reset password
	As a user who has forgotten their password
	I want to use the code I was emailed to choose a new password
	So that I can get back into my account

Background:
	Given the clock reads "2026-09-03T12:30:00Z"
	And the account "nikhil@example.com" exists
	And a password reset code "reset-code" was issued to "nikhil@example.com" that expires on "2026-09-03T13:00:00Z"

Scenario: The password is reset with a valid code
	When "nikhil@example.com" resets their password to "N3wPassw0rd!" using the code "reset-code"
	Then the request succeeds
	And the password of "nikhil@example.com" is now "N3wPassw0rd!"
	And the account "nikhil@example.com" holds no password reset code
	And every session of "nikhil@example.com" is revoked
	And a "UserPasswordChanged" event is published for the account "nikhil@example.com"

Scenario: Resetting the password lifts a sign-in lockout
	Given the account "nikhil@example.com" has been locked out by failed sign-ins
	When "nikhil@example.com" resets their password to "N3wPassw0rd!" using the code "reset-code"
	Then the request succeeds
	And the account "nikhil@example.com" is no longer locked out

Scenario: A code can only be used once
	When "nikhil@example.com" resets their password to "N3wPassw0rd!" using the code "reset-code"
	And "nikhil@example.com" resets their password to "An0therPassw0rd!" using the code "reset-code"
	Then the request fails with the error "The reset token is invalid or has expired."
	And the password of "nikhil@example.com" is now "N3wPassw0rd!"

Scenario: The code is wrong
	When "nikhil@example.com" resets their password to "N3wPassw0rd!" using the code "guessed-code"
	Then the request fails with the error "The reset token is invalid or has expired."
	And the password of "nikhil@example.com" is now "Passw0rd!"
	And no events are published

Scenario: The code has expired
	Given the clock reads "2026-09-03T13:00:00Z"
	When "nikhil@example.com" resets their password to "N3wPassw0rd!" using the code "reset-code"
	Then the request fails with the error "The reset token is invalid or has expired."

Scenario: The email is not registered
	When "nobody@example.com" resets their password to "N3wPassw0rd!" using the code "reset-code"
	Then the request fails with the error "The reset token is invalid or has expired."

Scenario: Every invalid detail is reported at once
	When "" resets their password to "" using the code ""
	Then the request fails with these errors:
		| Error                                        |
		| A valid email is required.                   |
		| Reset token is required.                     |
		| Password must be at least 8 characters long. |
