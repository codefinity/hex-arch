Feature: Update profile
	As a registered user
	I want to update my profile details
	So that my account reflects accurate information about me

Background:
	Given the profile clock is fixed at "2026-08-20T10:30:00Z"
	And a user account exists for "nikhil@example.com"
	And I am signed in as "nikhil@example.com"

Scenario: A signed-in user sets their profile for the first time
	When I update my profile with the following details:
		| Bio                   | Address           | DateOfBirth | AvatarUrl                       |
		| Full stack developer. | 221B Baker Street | 1990-05-15  | https://example.com/avatar.png  |
	Then the profile update succeeds
	And my profile bio is "Full stack developer."
	And my profile address is "221B Baker Street"
	And my profile date of birth is "1990-05-15"
	And my profile avatar URL is "https://example.com/avatar.png"
	And my profile was last updated on "2026-08-20T10:30:00Z"

Scenario: A signed-in user updates their existing profile
	Given my profile already has the bio "Old bio"
	When I update my profile with the following details:
		| Bio         | Address | DateOfBirth | AvatarUrl |
		| Updated bio |         |             |           |
	Then the profile update succeeds
	And my profile bio is "Updated bio"

Scenario: Updating the profile announces the change to the rest of the system
	When I update my profile with the following details:
		| Bio     | Address | DateOfBirth | AvatarUrl |
		| New bio |         |             |           |
	Then the profile update succeeds
	And a "UserProfileUpdated" event is published for my account
	And the profile update event occurred on "2026-08-20T10:30:00Z"

Scenario: The signed-in user's account no longer exists
	Given my account has been removed
	When I update my profile with the following details:
		| Bio     | Address | DateOfBirth | AvatarUrl |
		| New bio |         |             |           |
	Then the profile update fails with the error "User not found."
	And no profile event is published

Scenario: The avatar URL is not a valid absolute URL
	When I update my profile with the following details:
		| Bio | Address | DateOfBirth | AvatarUrl |
		|     |         |             | not-a-url |
	Then the profile update fails with the error "Avatar URL must be a valid absolute URL."

Scenario: The date of birth is in the future
	When I update my profile with the following details:
		| Bio | Address | DateOfBirth | AvatarUrl |
		|     |         | 2099-01-01  |           |
	Then the profile update fails with the error "Date of birth must be in the past."

Scenario: The bio exceeds the maximum length
	When I update my profile with a bio of 501 characters
	Then the profile update fails with the error "Bio must not exceed 500 characters."

Scenario: The address exceeds the maximum length
	When I update my profile with an address of 301 characters
	Then the profile update fails with the error "Address must not exceed 300 characters."

Scenario: Every invalid detail is reported at once
	When I update my profile with a bio of 501 characters, an address of 301 characters, an avatar URL of "not-a-url", and a date of birth of "2099-01-01"
	Then the profile update fails with these errors:
		| Error                                      |
		| Bio must not exceed 500 characters.        |
		| Address must not exceed 300 characters.    |
		| Avatar URL must be a valid absolute URL.   |
		| Date of birth must be in the past.         |
