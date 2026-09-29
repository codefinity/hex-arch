Feature: User reactivation
	As an administrator, or the payment context withdrawing a fraud alert
	I want a deactivated account to be reactivated
	So that a user who was suspended by mistake can use their account again

Background:
	Given the clock reads "2026-09-02T09:00:00Z"

Scenario: A deactivated account is reactivated
	Given the account "nikhil@example.com" was deactivated for "Suspicious activity."
	When the account "nikhil@example.com" is reactivated because "Investigation closed."
	Then the request succeeds
	And the account "nikhil@example.com" is active
	And the account "nikhil@example.com" no longer records a deactivation
	And a "UserReactivated" event is published for the account "nikhil@example.com"

Scenario: Reactivating an account that is already active is a no-op
	Given the account "nikhil@example.com" exists
	When the account "nikhil@example.com" is reactivated because "Investigation closed."
	Then the request succeeds
	And no events are published

Scenario: A fraud clearance reactivates an account deactivated for fraud
	Given the account "nikhil@example.com" was deactivated for "Fraud detected."
	When the account "nikhil@example.com" is reactivated because "Fraud cleared." only if it was deactivated for "Fraud detected."
	Then the request succeeds
	And the account "nikhil@example.com" is active

Scenario: A fraud clearance leaves an account deactivated for another reason alone
	Given the account "nikhil@example.com" was deactivated for "Abusive behaviour."
	When the account "nikhil@example.com" is reactivated because "Fraud cleared." only if it was deactivated for "Fraud detected."
	Then the request succeeds
	And the account "nikhil@example.com" is inactive
	And no events are published

Scenario: A closed account cannot be reactivated
	Given the account "nikhil@example.com" was closed
	When the account "nikhil@example.com" is reactivated because "Changed my mind."
	Then the request fails with the error "This account has been closed."

Scenario: The account does not exist
	When an unknown account is reactivated because "Investigation closed."
	Then the request fails with the error "User not found."

Scenario: The reason is missing
	Given the account "nikhil@example.com" was deactivated for "Suspicious activity."
	When the account "nikhil@example.com" is reactivated because ""
	Then the request fails with the error "Reason is required."
