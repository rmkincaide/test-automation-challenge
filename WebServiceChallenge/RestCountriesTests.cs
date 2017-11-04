﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace WebServiceChallenge
{
    [TestFixture]
    public class RestCountriesTests
    {
        private string api = "https://restcountries.eu/rest/v2/alpha/{0}";
        private DateTime testStart;

        [Test]
        // test all possible character combinations from aa to zz
        public void DataFormatTest_2CharacterCodes()
        {
            DataFormatTest(2);
        }

        [Test]
        // test all possible character combinations from aaa to zzz
        public void DataFormatTest_3CharacterCodes()
        {
            DataFormatTest(3);
        }

        private void DataFormatTest(int lengthOfAlphaCodeToCheck)
        {
            testStart = DateTime.Now;

            List<string> possibleAlphaCodes = new List<string>();
            var alphabet = "abcdefghijklmnopqrstuvwxyz";
            var q = alphabet.Select(x => x.ToString());

            for (int i = 0; i < lengthOfAlphaCodeToCheck - 1; i++)
            {
                q = q.SelectMany(x => alphabet, (x, y) => x + y);
            }

            foreach (string code in q)
            {
                possibleAlphaCodes.Add(code);
            }

            int numCountriesDetected = 0;

            Console.WriteLine("List of all {0}-Character Codes that have associated Countries returned by restcountries.eu:", lengthOfAlphaCodeToCheck);

            foreach (string possibleAlphaCode in possibleAlphaCodes)
            {
                System.Threading.Thread.Sleep(100); // throttling slightly so that restcountries.eu doesn't get spammed
                // replace {0} with alpha codes
                string URL = String.Format(api, possibleAlphaCode);
                //string urlParameters = "?api_key=123";
                string urlParameters = String.Empty;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(URL);

                // add an Accept header for JSON format
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // list the data response
                HttpResponseMessage response = client.GetAsync(urlParameters).Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;

                    dynamic country = JObject.Parse(json);

                    // initialize fields as expected object types
                    int countryPopulation = 0;
                    string countryAlpha2Code = String.Empty;
                    Uri countryFlag = new Uri("http://www.google.com"); // populate w/ temporary string to avoid System.UriFormatException

                    // verify that data types are formatted correctly by converting
                    try
                    {
                        countryPopulation = country.population;
                        countryAlpha2Code = country.alpha2Code;
                        countryFlag = country.flag; // this is an image, e.g. https://restcountries.eu/data/and.svg
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Assert.Fail("Failure to convert data into expected format");
                    }

                    // verify that formats of data object values match what's expected

                    // Assert.IsTrue(countryPopulation > 0);
                    // initially I figured every country would have a population > 0, but I was wrong; 
                    // it turns out Bouvet Island (BV) and Heard Island and McDonald Islands (HM) are uninhabited,
                    // so the countryPopulation = 0 returned by the API for those countries is, in fact, accurate
                    // https://en.wikipedia.org/wiki/Bouvet_Island  https://en.wikipedia.org/wiki/Heard_Island_and_McDonald_Islands
                    if (countryPopulation < 1)
                    {
                        Console.WriteLine("Population for Country {0} = {1}", countryAlpha2Code, countryPopulation);
                    }

                    Assert.AreEqual(2, countryAlpha2Code.Length);


                    // display each alpha2 / alpha3 code along w/ country name
                    // the list generated by this writeline can be compared to https://en.wikipedia.org/wiki/ISO_3166-1#Current_codes
                    Console.WriteLine(String.Format("{0} = {1}", lengthOfAlphaCodeToCheck == 2 ? country.alpha2Code : country.alpha3Code, country.name));

                    // make sure country's flag image can be retrieved
                    Console.WriteLine("Attempting to request Flag Image: " + countryFlag.AbsoluteUri);

                    try
                    {
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(countryFlag.AbsoluteUri);
                        HttpWebResponse countryflagResponse = (HttpWebResponse)webRequest.GetResponse();

                        Assert.AreEqual(HttpStatusCode.OK, countryflagResponse.StatusCode);
                        countryflagResponse.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    numCountriesDetected++;
                }
            }

            Console.WriteLine("Number of Countries Detected = " + numCountriesDetected);
            Console.WriteLine("Test Duration = {0} minutes", Math.Round((DateTime.Now - testStart).TotalMinutes, 2));
        }


    }
}
