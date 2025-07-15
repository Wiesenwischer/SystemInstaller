Feature: User Logout Functionality
  As a user of the SystemInstaller application
  I want to be able to logout completely
  So that my session is terminated and I cannot access protected resources

  Background:
    Given the SystemInstaller application is running on localhost:5000
    And Keycloak is running on localhost:8082
    And the application uses OIDC authentication via Gateway

  Scenario: Complete Logout Flow - User should not be automatically re-authenticated
    Given I am not logged in
    When I navigate to "http://localhost:5000"
    Then I should be redirected to the Keycloak login page
    And I should see the login form

    When I login with valid credentials
      | username | testuser |
      | password | password |
    # Note: If testuser doesn't exist, use any valid Keycloak user
    Then I should be redirected to the SystemInstaller home page
    And I should see the dashboard
    And I should be authenticated

    When I click the logout button
    Then I should be redirected to a logout confirmation page
    And my local session should be terminated
    And my Keycloak session should be terminated

    When I refresh the page (F5)
    Then I should see the login page again
    And I should NOT be automatically logged back in
    And I should NOT see the dashboard

    When I navigate to "http://localhost:5000" again
    Then I should be redirected to the Keycloak login page
    And I should be required to enter credentials again

  Scenario: Logout with Session Persistence Check
    Given I am logged in as "testuser"
    And I am on the dashboard page
    When I open a new tab
    And I navigate to "http://localhost:5000" in the new tab
    Then I should see the dashboard (session still active)

    When I go back to the first tab
    And I click logout
    Then I should be logged out

    When I go to the second tab
    And I refresh the page
    Then I should see the login page (session terminated everywhere)

  Scenario: API Access After Logout
    Given I am logged in as "testuser"
    When I logout
    And I try to access "/auth/user" endpoint
    Then I should receive a 401 Unauthorized response

    When I try to access "/api/protected" endpoint  
    Then I should receive a 401 Unauthorized response

  Scenario: Logout Debug Information
    Given I am logged in as "testuser"
    When I logout
    Then the gateway logs should contain:
      | message |
      | "=== LOGOUT ENDPOINT CALLED ===" |
      | "User authenticated: true" |
      | "Local sign out completed" |
      | "Redirecting to Keycloak logout" |
