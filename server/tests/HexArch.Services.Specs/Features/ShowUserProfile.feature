Feature: Show user profile
	As a registered user
	I want to view my profile
	So that I can see the account and profile details stored about me

Scenario: A signed-in user views their profile
	Given I am signed in as a user with a projected profile:
		| Name       | Email              | MobileNo   | Bio                   | Address           | DateOfBirth | AvatarUrl                       | Roles          |
		| Nikhil Pat | nikhil@example.com | 9999999999 | Full stack developer. | 221B Baker Street | 1990-05-15  | https://example.com/avatar.png  | Admin,Customer |
	When I view my profile
	Then the profile is shown successfully
	And the shown profile's name is "Nikhil Pat"
	And the shown profile's email is "nikhil@example.com"
	And the shown profile's mobile number is "9999999999"
	And the shown profile's bio is "Full stack developer."
	And the shown profile's address is "221B Baker Street"
	And the shown profile's date of birth is "1990-05-15"
	And the shown profile's avatar URL is "https://example.com/avatar.png"
	And the shown profile's roles are "Admin,Customer"

Scenario: A user who has never filled in their profile details views their profile
	Given I am signed in as a user with a projected profile:
		| Name       | Email              | MobileNo   | Bio | Address | DateOfBirth | AvatarUrl | Roles    |
		| Nikhil Pat | nikhil@example.com | 9999999999 |     |         |             |           | Customer |
	When I view my profile
	Then the profile is shown successfully
	And the shown profile's bio is empty
	And the shown profile's address is empty
	And the shown profile's date of birth is empty
	And the shown profile's avatar URL is empty

Scenario: The signed-in account has no projected profile yet
	Given I am signed in with no projected profile
	When I view my profile
	Then the profile is not found with the error "User not found."
