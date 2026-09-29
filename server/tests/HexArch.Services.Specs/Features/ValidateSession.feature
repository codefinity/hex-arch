Feature: Validate a session
	As the API
	I want to check that a signed token still represents a live session
	So that signing out, changing a password or being deactivated takes effect straight away

Scenario: A token carrying the current security stamp is accepted
	Given the account "nikhil@example.com" exists
	When a session for "nikhil@example.com" presents the current security stamp
	Then the request succeeds

Scenario: A token issued before the sessions were revoked is rejected
	Given the account "nikhil@example.com" exists
	When a session for "nikhil@example.com" presents an outdated security stamp
	Then the request fails with the error "The session is no longer valid."

Scenario: A token for a deactivated account is rejected
	Given the account "nikhil@example.com" was deactivated for "Fraud detected."
	When a session for "nikhil@example.com" presents the current security stamp
	Then the request fails with the error "The session is no longer valid."

Scenario: A token for an account that no longer exists is rejected
	When a session for an unknown account is validated
	Then the request fails with the error "The session is no longer valid."
