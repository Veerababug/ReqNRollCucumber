@parallelizable
Feature: Quit the google application


@Google
Scenario: Launch Website and Quit the Driver - TC_TMX2005
	Given Open Google WebSite
	When Maximize the window
	Then Quit the driver

@Google
Scenario: Enter Valid Credentials
	Given Open Google WebSite
	When Minimize the window in Google
	Then Quit the driver
