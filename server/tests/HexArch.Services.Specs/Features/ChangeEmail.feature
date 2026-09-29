Feature: Change email
	As a signed-in user
	I want to change the email address on my account
	So that I receive mail at my current address

Background:
	Given the clock reads "2026-09-04T10:00:00Z"
	And the account "nikhil@example.com" exists
	And I am signed in to the account "nikhil@example.com"

Scenario: An email change waits for the new address to be verified
	When I change my email to "new@example.com" confirming with the password "Passw0rd!"
	Then the request succeeds
	And the email address of "nikhil@example.com" is now "nikhil@example.com"
	And the account "nikhil@example.com" has a pending email change to "new@example.com"
	And the account "nikhil@example.com" holds an email verification code that expires on "2026-09-05T10:00:00Z"
	And the verification code is emailed to "new@example.com"
	And no events are published

Scenario: The password is wrong
	When I change my email to "new@example.com" confirming with the password "NotMyPassword!"
	Then the request fails with the error "The current password is incorrect."
	And no email is sent

Scenario: The new address is the current one
	When I change my email to "NIKHIL@example.com" confirming with the password "Passw0rd!"
	Then the request fails with the error "The new email address is the same as the current one."

Scenario: The new address belongs to another account
	Given the account "taken@example.com" exists
	When I change my email to "taken@example.com" confirming with the password "Passw0rd!"
	Then the request fails with the error "A user with this email is already registered."

Scenario: Every invalid detail is reported at once
	When I change my email to "not-an-email" confirming with the password ""
	Then the request fails with these errors:
		| Error                          |
		| A valid email is required.     |
		| Current password is required.  |
