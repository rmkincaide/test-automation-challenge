using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebServiceChallenge
{
    // not using this right now, but I'd probably deserialize using this if I were doing a more complete set of tests
    // made w/ help from https://quicktype.io
    public partial class Country
    {
        [JsonProperty("alpha2Code")]
        public string Alpha2Code { get; set; }

        [JsonProperty("alpha3Code")]
        public string Alpha3Code { get; set; }

        [JsonProperty("altSpellings")]
        public string[] AltSpellings { get; set; }

        [JsonProperty("area")]
        public long Area { get; set; }

        [JsonProperty("borders")]
        public string[] Borders { get; set; }

        [JsonProperty("callingCodes")]
        public string[] CallingCodes { get; set; }

        [JsonProperty("capital")]
        public string Capital { get; set; }

        [JsonProperty("cioc")]
        public string Cioc { get; set; }

        [JsonProperty("currencies")]
        public Currency[] Currencies { get; set; }

        [JsonProperty("demonym")]
        public string Demonym { get; set; }

        [JsonProperty("flag")]
        public string Flag { get; set; }

        [JsonProperty("gini")]
        public double Gini { get; set; }

        [JsonProperty("languages")]
        public Language[] Languages { get; set; }

        [JsonProperty("latlng")]
        public double[] Latlng { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nativeName")]
        public string NativeName { get; set; }

        [JsonProperty("numericCode")]
        public string NumericCode { get; set; }

        [JsonProperty("population")]
        public long Population { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("regionalBlocs")]
        public RegionalBloc[] RegionalBlocs { get; set; }

        [JsonProperty("subregion")]
        public string Subregion { get; set; }

        [JsonProperty("timezones")]
        public string[] Timezones { get; set; }

        [JsonProperty("topLevelDomain")]
        public string[] TopLevelDomain { get; set; }

        [JsonProperty("translations")]
        public Translations Translations { get; set; }
    }

    public partial class Translations
    {
        [JsonProperty("br")]
        public string Br { get; set; }

        [JsonProperty("de")]
        public string De { get; set; }

        [JsonProperty("es")]
        public string Es { get; set; }

        [JsonProperty("fa")]
        public string Fa { get; set; }

        [JsonProperty("fr")]
        public string Fr { get; set; }

        [JsonProperty("hr")]
        public string Hr { get; set; }

        [JsonProperty("it")]
        public string It { get; set; }

        [JsonProperty("ja")]
        public string Ja { get; set; }

        [JsonProperty("nl")]
        public string Nl { get; set; }

        [JsonProperty("pt")]
        public string Pt { get; set; }
    }

    public partial class RegionalBloc
    {
        [JsonProperty("acronym")]
        public string Acronym { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("otherAcronyms")]
        public string[] OtherAcronyms { get; set; }

        [JsonProperty("otherNames")]
        public string[] OtherNames { get; set; }
    }

    public partial class Language
    {
        [JsonProperty("iso639_1")]
        public string Iso6391 { get; set; }

        [JsonProperty("iso639_2")]
        public string Iso6392 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nativeName")]
        public string NativeName { get; set; }
    }

    public partial class Currency
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    public partial class Country
    {
        public static Country FromJson(string json) => JsonConvert.DeserializeObject<Country>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Country self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
