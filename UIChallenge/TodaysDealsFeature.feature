Feature: Today's Deals
	In order to find great deals at reasonable prices
	As an Amazon.com customer
	I want to be able to view Today's Deals in a variety of ways

@UIChallenge
Scenario: Sort by Price (Low to High)
	Given I have navigated to 'http://www.amazon.com/gp/goldbox' using Chrome
	When I click on the Sort By dropdown and select 'Price - Low to High'
	Then all products on each page should have names and images as well as prices that are formatted and sorted correctly

Scenario: Sort by Price (High to Low)
	Given I have navigated to 'http://www.amazon.com/gp/goldbox' using Chrome
	When I click on the Sort By dropdown and select 'Price - High to Low'
	Then all products on each page should have names and images as well as prices that are formatted and sorted correctly
	
	
# The above gherkin is pretty wordy, but checking for names / images / prices on each page before navigating to the next
# allows the tests to fail fast if a problem is detected and better ensures that names / images / prices correlate to one another
# which may not be the case if the tests looped through all pages 3 times to check for names / images / prices while running a separate 'Then' statement for each.
# This gherkin will definitely be an area where I'll focus more attention when I get more time.