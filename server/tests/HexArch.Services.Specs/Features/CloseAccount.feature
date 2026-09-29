Feature: Close account
	As a signed-in user
	I want to close my account
	So that my personal data is erased and the account can never be used again

Background:
	Given the clock reads "2026-09-05T15:00:00Z"
	And the account "nikhil@example.com" exists
	And the account "nikhil@example.com" has a profile with the bio "Full stack developer."
	And I am signed in to the account "nikhil@example.com"

Scenario: The account is closed and its personal data erased
	When I close my account confirming with the password "Passw0rd!"
	Then the request succeeds
	And the account "nikhil@example.com" is inactive
	And the account "nikhil@example.com" is closed and anonymised
	And the account "nikhil@example.com" has no profile
	And every session of "nikhil@example.com" is revoked
	And a "UserAccountClosed" event is published for the account "nikhil@example.com"

Scenario: Closing an account frees its email address
	When I close my account confirming with the password "Passw0rd!"
	Then the request succeeds
	And no account uses the email "nikhil@example.com"

Scenario: The password is wrong
	When I close my account confirming with the password "NotMyPassword!"
	Then the request fails with the error "The current password is incorrect."
	And the account "nikhil@example.com" is active
	And no events are published

Scenario: The last active administrator cannot close their account
	Given the account "admin@example.com" exists with the roles "Admin"
	And I am signed in to the account "admin@example.com"
	When I close my account confirming with the password "Passw0rd!"
	Then the request fails with the error "The last active administrator account cannot be closed."

Scenario: An administrator can close their account while another remains
	Given the account "admin@example.com" exists with the roles "Admin"
	And the account "other-admin@example.com" exists with the roles "Admin"
	And I am signed in to the account "admin@example.com"
	When I close my account confirming with the password "Passw0rd!"
	Then the request succeeds

Scenario: The password is missing
	When I close my account confirming with the password ""
	Then the request fails with the error "Current password is required."
