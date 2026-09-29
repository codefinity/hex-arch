Feature: Request email verification
	As a signed-in user
	I want a verification code emailed to me
	So that I can prove the address on my account is mine

Background:
	Given the clock reads "2026-09-04T10:00:00Z"

Scenario: A user with an unverified email is sent a code
	Given the account "nikhil@example.com" exists with an unverified email
	And I am signed in to the account "nikhil@example.com"
	When I ask for an email verification code
	Then the request succeeds
	And the account "nikhil@example.com" holds an email verification code that expires on "2026-09-05T10:00:00Z"
	And the verification code is emailed to "nikhil@example.com"

Scenario: A pending email change sends the code to the new address
	Given the account "nikhil@example.com" exists
	And the account "nikhil@example.com" has a pending email change to "new@example.com"
	And I am signed in to the account "nikhil@example.com"
	When I ask for an email verification code
	Then the request succeeds
	And the verification code is emailed to "new@example.com"

Scenario: An already verified email needs no code
	Given the account "nikhil@example.com" exists
	And I am signed in to the account "nikhil@example.com"
	When I ask for an email verification code
	Then the request fails with the error "The email address is already verified."
	And no email is sent

Scenario: The signed-in account no longer exists
	Given the signed-in account no longer exists
	When I ask for an email verification code
	Then the request fails with the error "User not found."
