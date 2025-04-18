@parallelizable @browser:Chrome
Feature: Launch the google application


@Sanity @browser:Chrome
Scenario: Launch Website and Quit the Driver - TC_TMX2005
	Given Open Google WebSite
	When Maximize the window
	Then Quit the driver

@Regression @browser:edge
Scenario: Enter Valid Credentials - TC_TMX2006
	Given Open Google WebSite
	When Minimize the window
	Then Quit the driver