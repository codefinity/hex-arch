Feature: Show a user
	As an administrator
	I want to view any user's details
	So that I can see what I am about to act on

Scenario: An administrator views a user
	Given the user "nikhil@example.com" has a projected view with the roles "Customer,Seller"
	When an administrator views the user "nikhil@example.com"
	Then the request succeeds
	And the user shown has the email "nikhil@example.com"
	And the user shown holds the roles "Customer,Seller"

Scenario: The user has no projected view
	When an administrator views an unknown user
	Then the request fails with the error "User not found."

Scenario: The user id is missing
	When an administrator views the user with an empty id
	Then the request fails with the error "UserId is required."
