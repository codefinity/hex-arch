Feature: User deactivation
	As the fraud detection system
	I want a user to be deactivated when fraud is detected on their account
	So that a compromised or fraudulent account can no longer sign in

Scenario: An active account is deactivated
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And the account for "nikhil@example.com" is no longer active

Scenario: Deactivating an account that is already inactive is a no-op
	Given the account "nikhil@example.com" is currently inactive
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation succeeds
	And no account is stored again

Scenario: The account does not exist
	Given there is no account for the reported user
	When the account is deactivated with reason "Fraud detected."
	Then the deactivation fails with the error "User not found."

Scenario: The reason is missing
	Given the account "nikhil@example.com" is currently active
	When the account is deactivated with reason ""
	Then the deactivation fails with the error "Reason is required."
