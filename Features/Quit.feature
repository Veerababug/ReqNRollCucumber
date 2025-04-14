@parallelizable
Feature: Quit the google application


@Sanity
Scenario: Launch Website and Quit the Driver
	Given Open Google WebSite
	When Maximize the window
	Then Quit the driver

@Exploratory
Scenario: Enter Valid Credentials
	Given Open Google WebSite
	When Minimize the window in Google
	Then Quit the driver
