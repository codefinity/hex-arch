Feature: Revoke a role
	As an administrator
	I want to take a role away from a user
	So that they can no longer do what that role allows

Background:
	Given the clock reads "2026-09-02T09:00:00Z"

Scenario: A role is revoked
	Given the account "nikhil@example.com" exists with the roles "Customer,Seller"
	When the "Seller" role is revoked from "nikhil@example.com"
	Then the request succeeds
	And the account "nikhil@example.com" holds the roles "Customer"
	And a "UserRolesChanged" event is published for the account "nikhil@example.com"

Scenario: Revoking a role signs the user out, since their tokens still claim it
	Given the account "nikhil@example.com" exists with the roles "Customer,Seller"
	When the "Seller" role is revoked from "nikhil@example.com"
	Then the request succeeds
	And every session of "nikhil@example.com" is revoked

Scenario: Revoking a role the user does not hold is a no-op
	Given the account "nikhil@example.com" exists
	When the "Seller" role is revoked from "nikhil@example.com"
	Then the request succeeds
	And the sessions of "nikhil@example.com" are left alone
	And no events are published

Scenario: A user must keep at least one role
	Given the account "nikhil@example.com" exists
	When the "Customer" role is revoked from "nikhil@example.com"
	Then the request fails with the error "A user must keep at least one role."

Scenario: The last active administrator keeps the Admin role
	Given the account "admin@example.com" exists with the roles "Admin,Customer"
	When the "Admin" role is revoked from "admin@example.com"
	Then the request fails with the error "The last active administrator cannot lose the Admin role."
	And the account "admin@example.com" holds the roles "Admin,Customer"

Scenario: An administrator can lose the Admin role while another remains
	Given the account "admin@example.com" exists with the roles "Admin,Customer"
	And the account "other-admin@example.com" exists with the roles "Admin"
	When the "Admin" role is revoked from "admin@example.com"
	Then the request succeeds
	And the account "admin@example.com" holds the roles "Customer"

Scenario: The account does not exist
	When the "Seller" role is revoked from an unknown account
	Then the request fails with the error "User not found."
