Feature: Update account details
	As a signed-in user
	I want to change my name and mobile number
	So that my account holds my current details

Background:
	Given the clock reads "2026-09-02T09:00:00Z"
	And the account "nikhil@example.com" exists
	And I am signed in to the account "nikhil@example.com"

Scenario: A signed-in user updates their details
	When I change my name to "Nikhil Patel" and my mobile number to "9876543210"
	Then the request succeeds
	And the account "nikhil@example.com" is named "Nikhil Patel" with the mobile number "9876543210"
	And a "UserAccountDetailsUpdated" event is published for the account "nikhil@example.com"

Scenario Outline: A required detail is missing
	When I change my name to "<name>" and my mobile number to "<mobileNo>"
	Then the request fails with the error "<error>"
	And no events are published

	Examples:
		| name         | mobileNo   | error                       |
		|              | 9876543210 | Name is required.           |
		| Nikhil Patel |            | Mobile number is required.  |

Scenario: The details are too long
	When I change my name to one of 201 characters and my mobile number to one of 21 characters
	Then the request fails with these errors:
		| Error                                          |
		| Name must not exceed 200 characters.           |
		| Mobile number must not exceed 20 characters.   |

Scenario: The signed-in account no longer exists
	Given the signed-in account no longer exists
	When I change my name to "Nikhil Patel" and my mobile number to "9876543210"
	Then the request fails with the error "User not found."
