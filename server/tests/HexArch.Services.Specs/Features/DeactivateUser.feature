Feature: User deactivation
	As the fraud detection system
	I want a user to be deactivated when fraud is detected on their account
	So that a compromised or fraudulent account can no longer sign in

Background:
	Given the deactivation clock is fixed at "2026-09-01T08:00:00Z"

Scenario: An active account is deactivated
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And the account for "nikhil@example.com" is no longer active

Scenario: Deactivation records why and when it happened
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And the account for "nikhil@example.com" was deactivated for "Fraud detected." on "2026-09-01T08:00:00Z"

Scenario: Deactivation signs the account out of every existing session
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And every existing session for "nikhil@example.com" has been revoked

Scenario: Deactivation announces the change to the rest of the system
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And a "UserDeactivated" event is published with reason "Fraud detected."

Scenario: Deactivating an account that is already inactive is a no-op
	Given the account "nikhil@example.com" is currently inactive
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And no account is stored again
	And no deactivation event is published

Scenario: The account does not exist
	Given there is no account for the reported user
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation fails with the error "User not found."

Scenario: The reason is missing
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason ""
	Then the deactivation fails with the error "Reason is required."

Scenario: The reason is too long
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with a reason of 201 characters
	Then the deactivation fails with the error "Reason must not exceed 200 characters."
