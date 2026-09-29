Feature: Verify email
	As a signed-in user
	I want to enter the code I was emailed
	So that my email address is marked as verified

Background:
	Given the clock reads "2026-09-04T10:00:00Z"
	And the account "nikhil@example.com" exists with an unverified email
	And I am signed in to the account "nikhil@example.com"
	And an email verification code "verify-code" was issued to "nikhil@example.com" that expires on "2026-09-05T10:00:00Z"

Scenario: The email is verified with a valid code
	When I verify my email with the code "verify-code"
	Then the request succeeds
	And the email of "nikhil@example.com" is verified
	And the account "nikhil@example.com" holds no email verification code
	And a "UserEmailVerified" event is published for the account "nikhil@example.com"

Scenario: Verifying completes a pending email change
	Given the account "nikhil@example.com" has a pending email change to "new@example.com"
	When I verify my email with the code "verify-code"
	Then the request succeeds
	And the email address of "nikhil@example.com" is now "new@example.com"
	And the account "nikhil@example.com" has no pending email change

Scenario: The new address was registered by someone else in the meantime
	Given the account "nikhil@example.com" has a pending email change to "taken@example.com"
	And the account "taken@example.com" exists
	When I verify my email with the code "verify-code"
	Then the request fails with the error "A user with this email is already registered."
	And the email address of "nikhil@example.com" is now "nikhil@example.com"

Scenario: The code is wrong
	When I verify my email with the code "guessed-code"
	Then the request fails with the error "The verification token is invalid or has expired."
	And the email of "nikhil@example.com" is not verified
	And no events are published

Scenario: The code has expired
	Given the clock reads "2026-09-05T10:00:00Z"
	When I verify my email with the code "verify-code"
	Then the request fails with the error "The verification token is invalid or has expired."

Scenario: The code is missing
	When I verify my email with the code ""
	Then the request fails with the error "Verification token is required."
