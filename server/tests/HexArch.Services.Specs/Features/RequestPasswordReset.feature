Feature: Request a password reset
	As a user who has forgotten their password
	I want a reset code emailed to me
	So that I can choose a new password without signing in

Background:
	Given the clock reads "2026-09-03T12:00:00Z"

Scenario: A registered user is emailed a reset code
	Given the account "nikhil@example.com" exists
	When a password reset is requested for "nikhil@example.com"
	Then the request succeeds
	And the account "nikhil@example.com" holds a password reset code that expires on "2026-09-03T13:00:00Z"
	And the reset code is emailed to "nikhil@example.com"

Scenario: An unknown email looks exactly like a known one
	When a password reset is requested for "nobody@example.com"
	Then the request succeeds
	And no email is sent

Scenario: A deactivated account is not sent a reset code
	Given the account "nikhil@example.com" was deactivated for "Fraud detected."
	When a password reset is requested for "nikhil@example.com"
	Then the request succeeds
	And the account "nikhil@example.com" holds no password reset code
	And no email is sent

Scenario: A mail server failure looks exactly like success
	Given the account "nikhil@example.com" exists
	And the mail server is unavailable
	When a password reset is requested for "nikhil@example.com"
	Then the request succeeds

Scenario: The email is not valid
	When a password reset is requested for "not-an-email"
	Then the request fails with the error "A valid email is required."
