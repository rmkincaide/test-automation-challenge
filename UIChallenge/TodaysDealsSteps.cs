using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;

namespace AmazonChallenge
{
    [Binding]
    public class TodaysDealsSteps
    {
        public static TimeSpan defaultTimeout = TimeSpan.FromSeconds(30);
        private ChromeDriver driver;
        private string sortOrderSelected; // would probably adjust this if implementing all sort dropdown choices

        [Given(@"I have navigated to '(.*)' using Chrome")]
        public void GivenIHaveNavigatedToUsingChrome(string p0)
        {
            // fail early if gherkin passes in an invalid URL
            Uri uriResult;
            bool uriIsValid = Uri.TryCreate(p0, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;

            if (!uriIsValid)
                Assert.Fail(p0 + " is not a valid URL.");

            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = defaultTimeout;
            driver.Navigate().GoToUrl(p0);
            WaitForPageToBeReady();
        }

        [When(@"I click on the Sort By dropdown and select '(.*)'")]
        public void WhenIClickOnTheSortByDropdown(string p0)
        {
            var elements = driver.FindElementsByName("sortOptions");

            Assert.IsTrue(elements.Count == 1, "Expected to find 1 sortOptions element but found " + elements.Count);

            var sortDropdownSelection = new SelectElement(elements[0]);
            sortDropdownSelection.SelectByText(p0);
            sortOrderSelected = p0;
        }

        [When(@"I select Low to High")]
        public void WhenISelectLowToHigh()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"all products on each page should have names and images as well as prices that are formatted and sorted correctly")]
        public void AllProductsOnEachPageShouldHaveNamesAndImagesAsWellAsPricesThatAreFormattedAndSortedCorrectly()
        {
            bool sortLowToHigh = false;

            switch (sortOrderSelected)
            {
                case "Price - Low to High":
                {
                    sortLowToHigh = true;
                    break;
                }
                case "Price - High to Low":
                {
                    break;
                }
                default:
                {
                    Assert.Fail("Incorrect 'Sort By' dropdown selection");
                    break;
                }
            }

            // find the center container first to avoid parsing the deal containers at the bottom of the page that aren't part of Today's Deals
            var centerContainer = driver.FindElementsByXPath("//*[@id=\"a-page\"]/div[6]");
            Assert.IsTrue(centerContainer.Count == 1, "Expected to find 1 centerContainer element but found " + centerContainer.Count);

            int pageNum = 0;
            decimal previousPriceDetected;

            if (sortLowToHigh)
            {
                previousPriceDetected = 0;
            }
            else
            {
                previousPriceDetected = 9999999; // not ideal (does Amazon have a max price for products they sell?)
            }

            bool morePagesToNavigate = true;

            // this RegEx pattern is used to make sure prices are represented as currency amounts w/
            // cents mandatory and optional thousands separators; mandatory two-digit fraction
            // credit to https://stackoverflow.com/questions/354044/what-is-the-best-u-s-currency-regex
            string currencyRegex = @"^[+-]?[0-9]{1,3}(?:,?[0-9]{3})*\.[0-9]{2}$";

            while (morePagesToNavigate)
            {
                pageNum++;

                // deal container elements are organized in sets of 3
                WaitUntilElementExists(By.ClassName("dealContainer"));
                var dealContainers = centerContainer[0].FindElements(By.ClassName("dealContainer"));

                Console.WriteLine("Num Deal Container Sets Detected on Page {0} = {1}", pageNum, dealContainers.Count/3);
                
                for (int i = 0; i < dealContainers.Count; i += 3)
                {
                    try
                    {
                        var productNameElementLocator = By.CssSelector("#dealTitle > span");
                        WaitUntilElementExists(productNameElementLocator, 10);
                        var productName = dealContainers[i].FindElement(productNameElementLocator).Text;
                        Assert.IsTrue(!String.IsNullOrEmpty(productName), String.Format("Missing Product Name Detected on Page " + pageNum));

                        var productImageElementLocator = By.CssSelector("#dealImage > div > div > div:nth-child(1) > img");
                        WaitUntilElementExists(productImageElementLocator);
                        var productImage = dealContainers[i].FindElement(productImageElementLocator);
                        Assert.IsTrue(productImage.TagName == "img" && !productImage.Size.IsEmpty, "No Image Detected for Product " + productName);

                        var productPriceElementLocator = By.CssSelector("div > div:nth-child(2) > div > div:nth-child(3) > div > span");
                        WaitUntilElementExists(productPriceElementLocator);
                        var price = dealContainers[i].FindElement(productPriceElementLocator);

                        // only pull the min price for  comparison (some prices display as ranges (e.g. "$2.81 - $25.50"))
                        string minPrice = price.Text.Substring(1).Split(' ')[0];

                        Assert.IsTrue(Regex.IsMatch(minPrice, currencyRegex), "Price Incorrectly Formatted: " + minPrice);

                        decimal newPriceDetected = Decimal.Parse(minPrice);

                        // this if/else code should get cleaned up (https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)
                        if (sortLowToHigh)
                        {
                            Assert.IsTrue(newPriceDetected >= previousPriceDetected,
                                String.Format("New price of {0} ({1}) is less than previous price of {2}", newPriceDetected, productName, previousPriceDetected));

                            Console.WriteLine("New High Price Detected: {0} = {1}", productName, newPriceDetected);
                            previousPriceDetected = newPriceDetected;
                        }
                        else // sort high to low
                        {
                            Assert.IsTrue(newPriceDetected <= previousPriceDetected,
                                String.Format("New price of {0} ({1}) is greater than previous price of {2}", newPriceDetected, productName, previousPriceDetected));

                            Console.WriteLine("New Low Price Detected: {0} = {1}", productName, newPriceDetected);
                            previousPriceDetected = newPriceDetected;
                        }
                        
                    }
                    catch (NoSuchElementException)
                    {
                        // if a price isn't found, the product might need to be added to the cart in order to view the price
                        // I'd typically discuss this with a Product Owner / Dev Team to understand this edge case better,
                        // determine whether or not it's already covered by unit tests, etc. before investing time in automating it;
                        // since it's not explicitly listed in the requirements for the UI Challenge I won't implement it initially
                        var hiddenPrice = dealContainers[i].FindElement(By.CssSelector("div > div:nth-child(2) > div > div:nth-child(4) > div > span"));
                        Assert.AreEqual("Add to cart to see price", hiddenPrice.Text, "No price or 'Add to cart to see price' message is displayed");
                        Console.WriteLine("Skipping Price Check for Product (need to add product to cart in order to see price)");   
                    }
                }

                var nextButton = centerContainer[0].FindElements(By.PartialLinkText("→"));
                Assert.IsTrue(nextButton.Count <= 1, "Expected to find 1 centerContainer element but found " + centerContainer.Count);

                if (nextButton.Count == 1)
                {
                    nextButton[0].Click();
                    WaitForPageToBeReady();
                }
                else
                {
                    morePagesToNavigate = false;
                }
            }
        }

        private IWebElement WaitUntilElementExists(By elementLocator, int timeout = 5)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(ExpectedConditions.ElementExists(elementLocator));
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void WaitForPageToBeReady()
        {
            // this isn't working consistently :(
            new WebDriverWait(driver, defaultTimeout).Until(
                d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            // I strongly dislike doing this, but right now disabling the Thread.Sleep will cause the tests to occasionally page through 
            // Today's Deals too quickly before asserting against the page's data (in particular, images don't always load in time)
            // In situations like this I'd look to collaborate with dev teams to understand Amazon.com's inner workings and design patterns better,
            // as well as investigate further with tools like fiddler / wireshark
            System.Threading.Thread.Sleep(1500);
        }
    }
}
