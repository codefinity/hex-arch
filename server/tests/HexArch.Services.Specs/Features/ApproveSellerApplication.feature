Feature: Approve a seller application
	As an administrator
	I want to approve a seller application
	So that the applicant becomes a seller

Background:
	Given the clock reads "2026-09-06T14:00:00Z"
	And the account "admin@example.com" exists with the roles "Admin"
	And I am signed in to the account "admin@example.com"

Scenario: An application is approved
	Given the account "nikhil@example.com" exists
	And the account "nikhil@example.com" has a pending seller application
	When the latest seller application from "nikhil@example.com" is approved
	Then the request succeeds
	And the account "nikhil@example.com" holds the roles "Customer,Seller"
	And the latest seller application from "nikhil@example.com" was "Approved" by "admin@example.com"
	And a "SellerApplicationApproved" event is published for the account "nikhil@example.com"
	And a "UserRolesChanged" event is published for the account "nikhil@example.com"

Scenario: An application that has already been decided cannot be approved
	Given the account "nikhil@example.com" exists
	And the account "nikhil@example.com" had a seller application rejected
	When the latest seller application from "nikhil@example.com" is approved
	Then the request fails with the error "This seller application has already been decided."
	And the account "nikhil@example.com" holds the roles "Customer"

Scenario: The applicant has been deactivated since applying
	Given the account "nikhil@example.com" was deactivated for "Fraud detected."
	And the account "nikhil@example.com" has a pending seller application
	When the latest seller application from "nikhil@example.com" is approved
	Then the request fails with the error "The applicant's account is not active."

Scenario: The application does not exist
	When an unknown seller application is approved
	Then the request fails with the error "Seller application not found."
