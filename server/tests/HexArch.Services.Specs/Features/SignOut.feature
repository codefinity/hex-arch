Feature: Sign out
	As a signed-in user
	I want to sign out
	So that nobody else can use my account from a device I have left signed in

Background:
	Given the clock reads "2026-09-02T09:00:00Z"
	And the account "nikhil@example.com" exists

Scenario: Signing out revokes every session
	Given I am signed in to the account "nikhil@example.com"
	When I sign out
	Then the request succeeds
	And every session of "nikhil@example.com" is revoked
	And a "UserSessionsRevoked" event is published for the account "nikhil@example.com"

Scenario: The signed-in account no longer exists
	Given the signed-in account no longer exists
	When I sign out
	Then the request fails with the error "User not found."
