Feature: User registration
	As a visitor to HexArch
	I want to register an account with my name, email, password and mobile number
	So that I can sign in and shop

Background:
	Given the default "Customer" role is configured
	And the system clock is fixed at "2026-08-20T10:30:00Z"

Scenario: A visitor registers with valid details
	When I register with the following details:
		| Name         | Email               | Password     | MobileNo   |
		| Nikhil Patel | nikhil@example.com  | Sup3rSecret! | 9876543210 |
	Then the registration succeeds
	And the new account is returned with an identifier
	And an account exists for "nikhil@example.com"
	And that account is active
	And that account is registered on "2026-08-20T10:30:00Z"
	And that account is granted the "Customer" role

Scenario: The password is never stored in plain text
	When I register with the following details:
		| Name         | Email               | Password     | MobileNo   |
		| Nikhil Patel | nikhil@example.com  | Sup3rSecret! | 9876543210 |
	Then the registration succeeds
	And the stored password for "nikhil@example.com" is not "Sup3rSecret!"
	And the stored password for "nikhil@example.com" is salted

Scenario: Registering announces the new account to the rest of the system
	When I register with the following details:
		| Name         | Email               | Password     | MobileNo   |
		| Nikhil Patel | nikhil@example.com  | Sup3rSecret! | 9876543210 |
	Then the registration succeeds
	And a "UserRegistered" event is published for the new account
	And that event occurred on "2026-08-20T10:30:00Z"

Scenario: The email address is already registered
	Given an account already exists for "nikhil@example.com"
	When I register with the following details:
		| Name         | Email               | Password     | MobileNo   |
		| Nikhil Patel | nikhil@example.com  | Sup3rSecret! | 9876543210 |
	Then the registration fails with the error "A user with this email is already registered."
	And no new account is stored
	And no event is published

Scenario: The default role has not been configured
	Given the default "Customer" role is not configured
	When I register with the following details:
		| Name         | Email               | Password     | MobileNo   |
		| Nikhil Patel | nikhil@example.com  | Sup3rSecret! | 9876543210 |
	Then the registration fails with the error "Default role 'Customer' is not configured."
	And no new account is stored
	And no event is published

Scenario Outline: A required detail is missing or invalid
	When I register with the following details:
		| Name   | Email   | Password   | MobileNo   |
		| <name> | <email> | <password> | <mobileNo> |
	Then the registration fails with the error "<error>"
	And no new account is stored
	And no event is published

	Examples:
		| name         | email              | password     | mobileNo   | error                                          |
		|              | nikhil@example.com | Sup3rSecret! | 9876543210 | Name is required.                              |
		| Nikhil Patel |                    | Sup3rSecret! | 9876543210 | A valid email is required.                     |
		| Nikhil Patel | not-an-email       | Sup3rSecret! | 9876543210 | A valid email is required.                     |
		| Nikhil Patel | nikhil@example.com |              | 9876543210 | Password must be at least 8 characters long.   |
		| Nikhil Patel | nikhil@example.com | Sh0rt        | 9876543210 | Password must be at least 8 characters long.   |
		| Nikhil Patel | nikhil@example.com | Sup3rSecret! |            | Mobile number is required.                     |

Scenario: Every invalid detail is reported at once
	When I register with the following details:
		| Name | Email | Password | MobileNo |
		|      |       |          |          |
	Then the registration fails with these errors:
		| Error                                          |
		| Name is required.                              |
		| A valid email is required.                     |
		| Password must be at least 8 characters long.   |
		| Mobile number is required.                     |
