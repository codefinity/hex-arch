Feature: Reject a seller application
	As an administrator
	I want to reject a seller application
	So that an unsuitable applicant does not become a seller

Background:
	Given the clock reads "2026-09-06T14:00:00Z"
	And the account "admin@example.com" exists with the roles "Admin"
	And I am signed in to the account "admin@example.com"
	And the account "nikhil@example.com" exists

Scenario: An application is rejected
	Given the account "nikhil@example.com" has a pending seller application
	When the latest seller application from "nikhil@example.com" is rejected because "Business details are incomplete."
	Then the request succeeds
	And the latest seller application from "nikhil@example.com" was "Rejected" by "admin@example.com"
	And the latest seller application from "nikhil@example.com" notes "Business details are incomplete."
	And the account "nikhil@example.com" holds the roles "Customer"
	And a "SellerApplicationRejected" event is published for the account "nikhil@example.com"

Scenario: An application that has already been decided cannot be rejected
	Given the account "nikhil@example.com" had a seller application rejected
	When the latest seller application from "nikhil@example.com" is rejected because "Still incomplete."
	Then the request fails with the error "This seller application has already been decided."

Scenario: The reason is missing
	Given the account "nikhil@example.com" has a pending seller application
	When the latest seller application from "nikhil@example.com" is rejected because ""
	Then the request fails with the error "Reason is required."

Scenario: The application does not exist
	When an unknown seller application is rejected because "Business details are incomplete."
	Then the request fails with the error "Seller application not found."
