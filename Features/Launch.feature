@parallelizable
Feature: Launch the google application


@Google
Scenario: Launch Website and Quit the Driver - TC_TMX2005
	Given Open Google WebSite
	When Maximize the window
	Then Quit the driver

@Google
Scenario: Enter Valid Credentials - TC_TMX2006
	Given Open Google WebSite
	When Minimize the window
	Then Quit the driver
