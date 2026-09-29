Feature: Apply for a seller account
	As a customer
	I want to apply to become a seller
	So that I can list products once an administrator approves me

Background:
	Given the clock reads "2026-09-06T11:00:00Z"

Scenario: A customer applies to become a seller
	Given the account "nikhil@example.com" exists
	And I am signed in to the account "nikhil@example.com"
	When I apply to become a seller trading as "Nikhil's Books"
	Then the request succeeds
	And a pending seller application from "nikhil@example.com" trading as "Nikhil's Books" is stored
	And a "SellerApplicationSubmitted" event is published for the account "nikhil@example.com"

Scenario: A customer whose application was rejected may apply again
	Given the account "nikhil@example.com" exists
	And the account "nikhil@example.com" had a seller application rejected
	And I am signed in to the account "nikhil@example.com"
	When I apply to become a seller trading as "Nikhil's Books"
	Then the request succeeds

Scenario: A seller cannot apply again
	Given the account "nikhil@example.com" exists with the roles "Customer,Seller"
	And I am signed in to the account "nikhil@example.com"
	When I apply to become a seller trading as "Nikhil's Books"
	Then the request fails with the error "You are already a seller."

Scenario: Only one application can be pending at a time
	Given the account "nikhil@example.com" exists
	And the account "nikhil@example.com" has a pending seller application
	And I am signed in to the account "nikhil@example.com"
	When I apply to become a seller trading as "Nikhil's Books"
	Then the request fails with the error "You already have a pending seller application."
	And no events are published

Scenario: The business name is missing
	Given the account "nikhil@example.com" exists
	And I am signed in to the account "nikhil@example.com"
	When I apply to become a seller trading as ""
	Then the request fails with the error "Business name is required."
