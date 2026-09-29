Feature: Assign a role
	As an administrator
	I want to grant a role to a user
	So that they can do what that role allows

Background:
	Given the clock reads "2026-09-02T09:00:00Z"
	And the account "nikhil@example.com" exists

Scenario: A role is granted
	When the "Seller" role is assigned to "nikhil@example.com"
	Then the request succeeds
	And the account "nikhil@example.com" holds the roles "Customer,Seller"
	And a "UserRolesChanged" event is published for the account "nikhil@example.com"

Scenario: Granting a role does not sign the user out
	When the "Seller" role is assigned to "nikhil@example.com"
	Then the request succeeds
	And the sessions of "nikhil@example.com" are left alone

Scenario: Granting a role the user already holds is a no-op
	When the "Customer" role is assigned to "nikhil@example.com"
	Then the request succeeds
	And the account "nikhil@example.com" holds the roles "Customer"
	And no events are published

Scenario: The role does not exist
	When the "Moderator" role is assigned to "nikhil@example.com"
	Then the request fails with the error "Role 'Moderator' does not exist."

Scenario: The account has been closed
	Given the account "closed@example.com" was closed
	When the "Seller" role is assigned to "closed@example.com"
	Then the request fails with the error "This account has been closed."

Scenario: The account does not exist
	When the "Seller" role is assigned to an unknown account
	Then the request fails with the error "User not found."

Scenario: The role name is missing
	When the "" role is assigned to "nikhil@example.com"
	Then the request fails with the error "Role name is required."
