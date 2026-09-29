Feature: Revoke a user's sessions
	As an administrator
	I want to sign a user out of every device
	So that a compromised account can be contained without deactivating it

Background:
	Given the clock reads "2026-09-02T09:00:00Z"
	And the account "nikhil@example.com" exists

Scenario: An administrator signs a user out everywhere
	When an administrator revokes every session of "nikhil@example.com"
	Then the request succeeds
	And every session of "nikhil@example.com" is revoked
	And the account "nikhil@example.com" is active
	And a "UserSessionsRevoked" event is published for the account "nikhil@example.com"

Scenario: The account does not exist
	When an administrator revokes every session of an unknown account
	Then the request fails with the error "User not found."
