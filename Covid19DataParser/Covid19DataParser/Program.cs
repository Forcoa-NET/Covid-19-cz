using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

const int DATE_COLUMN_INDEX = 1;
const int CITY_CODE__COLUMN_INDEX = 10;
const int CITY_NAME__COLUMN_INDEX = 11;
const int NEW_CASES_COLUMN_INDEX = 12;
const int CURENT_CASES_COLUMN_INDEX = 13;

var filename = @"C:\Work\VSB\Covid-19-cz\obce\obec.csv";

FileStream fileStream = new(filename, FileMode.Open, FileAccess.Read);
Dictionary<string,City> cities = new();

Func<string, string> get7BitString = (string s) =>
{
    string source = s.ToLower();
    string SET1 = "áäàãčçďéěëêèíľňóöřšťúůüùýž";
    string SET2 = "aaaaccdeeeeeilnoorstuuuuyz";
    StringBuilder sb = new StringBuilder();
    foreach (char ch in source)
    {
        int index = SET1.IndexOf(ch);
        if (index >= 0)
        {
            sb.Append(SET2[index]);
        }
        else
        {
            sb.Append(ch);
        }
    }
    return sb.ToString();
};

using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
{
    string line;

    while ((line = streamReader.ReadLine()) != null)
    {
        if (line == "" || line.StartsWith("den;"))
            continue;

        var values = line.Split(";");
        var cityCode = values[CITY_CODE__COLUMN_INDEX];
        var cityName = values[CITY_NAME__COLUMN_INDEX];

        if (!cities.ContainsKey(cityCode))
        {
            cities.Add(cityCode, new City { Code = cityCode, Name = cityName });
        }

        // add entry 
        var day = values[DATE_COLUMN_INDEX];
        Int32.TryParse(values[NEW_CASES_COLUMN_INDEX], out var new_case);
        Int32.TryParse(values[CURENT_CASES_COLUMN_INDEX], out var actual_cases);
        cities[cityCode].days.Add(day, (new_case, actual_cases));
    }

    // Generate header line
    DateTime curentDate = Convert.ToDateTime("2020-03-01");
    DateTime enddate = Convert.ToDateTime("2020-11-02");

    var linestoWrite = new List<string>();
    var headerLine = "Id;CityName;";

    while (curentDate < enddate)
    {
        var datestr = curentDate.ToString("yyyy-MM-dd");
        headerLine += $"N-{datestr};A-{datestr};";
        curentDate = curentDate.AddDays(1);
    }
    linestoWrite.Add(headerLine);

    // Generate lines to write

    curentDate = Convert.ToDateTime("2020-03-01");
    enddate = Convert.ToDateTime("2020-11-02");

    foreach(var kvpCity in cities.ToList())
    {
        var lineToWrite = kvpCity.Key+";" + get7BitString(kvpCity.Value.Name) + ";";
        while (curentDate < enddate)
        {
            var daystr = curentDate.ToString("yyyy-MM-dd");

            if(kvpCity.Value.days.ContainsKey(daystr))
            {
                lineToWrite += $"{kvpCity.Value.days[daystr].Item1};{kvpCity.Value.days[daystr].Item2};";
            }
            else
            {
                lineToWrite += "NA;NA;"; 
            }

            curentDate = curentDate.AddDays(1);
        }
        // write and reset date
        linestoWrite.Add(lineToWrite);
        curentDate = Convert.ToDateTime("2020-03-01");
    }

    using (StreamWriter file = new StreamWriter(@"output.csv"))
    {
        foreach (var l in linestoWrite)
        {
            file.WriteLine(l);
        }
    }
}

public record City
{
    public string Code;
    public string Name;
    public Dictionary<string, (int, int)> days = new Dictionary<string, (int, int)>();
};
