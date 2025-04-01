using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using System.Net.Mail;
using System.Reflection;

public static class AuxiliaryClass
{
    private static System.Random rng = new System.Random();

    /// <summary>
    /// Fisher-Yates shuffling algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">The list of T type that's going to be suffled</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Adds a value to the list by times specified
    /// </summary>
    /// <param name="list">The list where the value is going to be added</param>
    /// <param name="value">The value that's going to be added</param>
    /// <param name="times">The number of times we would like to add this value into the list</param>
    /// <typeparam name="T"></typeparam>
    public static void AddNTimes<T>(this IList<T> list, T value, int times)
    {
        for (int i = 0; i < times; i++)
        {
            list.Add(value);
        }
    }

    /// <summary>
    /// Adds a value to the list by times specified
    /// </summary>
    /// <param name="list">The list where the value is going to be added</param>
    /// <param name="collection">The collection of values that are going to be added</param>
    /// <param name="times">The number of times we would like to add this value into the list</param>
    /// <typeparam name="T"></typeparam>
    public static void AddRangeNTimes<T>(this IList<T> list, IEnumerable<T> collection, int times)
    {
        for (int i = 0; i < times; i++)
        {
            foreach (var value in collection)
            {
                list.Add(value);
            }
        }
    }


    /// <summary>
    /// Changes the value of a Boolean variable randomly
    /// </summary>
    /// <param name="b">Is the boolean parameter that we are using</param>
    /// <param name="seedSize">Indicates how many variables the pool of possibilities will have</param>
    public static void RandomBool(this bool b, int seedSize = 4)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < seedSize; i++)
        {
            list.Add(i % 2);
        }

        list.Shuffle();

        if (list[rng.Next(list.Count())] == 0) b = false;
        else b = true;
    }


    /// <summary>
    /// Generates a new Random boolean value
    /// </summary>
    /// <param name="seedSize">Indicates how many variables the pool of possibilities will have</param>
    /// <returns></returns>
    public static bool RandomBool(int seedSize = 4)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < seedSize; i++)
        {
            list.Add(i % 2);
        }

        list.Shuffle();

        if (list[rng.Next(list.Count())] == 0) return false;
        else return true;
    }

    /// <summary>
    /// Pass an AND operator over a list of boolean values. If the list doesn't have any values. Then it returns false
    /// </summary>
    /// <param name="list"></param>
    /// <returns>The result of the AND operator between all the elements in the boolean list</returns>
    public static bool And(this IList<bool> list)
    {
        int n = list.Count;
        if (n == 0) return false;
        bool result = true;
        while (n > 0)
        {
            n--;
            result &= list[n];
        }
        return result;
    }
    
    /// <summary>
    /// Pass an OR operator over a list of boolean values. If the list doesn't have any values. Then it returns false
    /// </summary>
    /// <param name="list"></param>
    /// <returns>The result of the OR operator between all the elements in the boolean list</returns>
    public static bool Or(this IList<bool> list)
    {
        int n = list.Count;
        if (n == 0) return false;
        bool result = false;
        while (n > 0)
        {
            n--;
            result |= list[n];
        }
        return result;
    }

    /// <summary>
    /// Allows the copy of Components via Reflection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="comp"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                             BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch
                {
                } // In case of NotImplementedException being thrown. For some reason, specifying that exception didn't catch anything specific.
            }
        }

        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }

        return comp as T;
    }

    /// <summary>
    /// Recovers data from a CSV file and uses it as serialized data to generate an Instance of a class.
    /// Constrains:
    ///     The first row acts as headers and is ignored.
    ///     The CSV must have 2 columns.
    ///     The first column must be the class variable names.
    ///     The second column must be the values of those variables.
    /// </summary>
    /// <param name="csv">The csv string</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Returns the new instance of a T class based on CSV data</returns>
    public static T SerializedFromCsv<T>(this string csv)
    {
        string[] rows = csv.Split('\n');

        string json = "{\n";

        for (int i = 1; i < rows.Length; i++)
        {
            string evalColumn = "";
            string[] columns = rows[i].Split(',');
            if (columns.Length > 2)
                evalColumn = rows[i].Split(',')[Range.StartAt(1)].CreateCsvStringArray();
            else
                evalColumn = columns[1];
            
            
            json += "\"" + rows[i].Split(",")[0] + "\" : " + evalColumn + ",\n";
        }
        
        json = json.Substring(0,json.Length - 2);
        json += "\n} ";
        
        return JsonUtility.FromJson<T>(json);

    }

    /// <summary>
    /// From a CSV string recombines the elements of an Array value to use in a JSON
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static string CreateCsvStringArray(this string[] array)
    {
        string csvString = "";
        for (int i = 0; i < array.Length; i++)
        {
            csvString += array[i] + ",";
        }
        
        return csvString.Substring(0, csvString.Length - 1);
    }

    /// <summary>
    /// Add Component extension that allows to copy values from existing Gameobject's component
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="toAdd"></param>
    /// <returns></returns>
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }

    /// <summary>
    /// Transform a number into a string using currency format.
    /// </summary>
    /// <param name="number">The number to transform</param>
    /// <param name="withMoneySign">If true, the $ sign is added into the string at the begining</param>
    /// <returns>The string in currency format</returns>
    public static string ConvertToMoneyFormat(this int number, bool withMoneySign = true)
    {
        CultureInfo culture = new CultureInfo("en-US");
        culture.NumberFormat.CurrencyGroupSeparator = ".";
        culture.NumberFormat.CurrencyDecimalSeparator = ",";
        culture.NumberFormat.NumberGroupSeparator = ".";
        culture.NumberFormat.NumberDecimalSeparator = ",";

        if (!withMoneySign) return number.ToString("N0", culture);
        // Format the number as currency with the desired settings
        return number.ToString("C0", culture);
    }

    /// <summary>
    /// Counts the amount of words in string.
    /// </summary>
    /// <param name="text">The text to evaluate</param>
    /// <returns>The amount of words in a integer format</returns>
    public static int CountWords(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        string[] words = text.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Length;
    }

    /// <summary>
    /// Validates a string as an email address
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static bool ValidateEmail(this string email)
    {
        try
        {
            MailAddress mailAddress = new MailAddress(email);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }


    /// <summary>
    /// Checks a string as a Chilean ID Rut
    /// </summary>
    /// <param name="rut">The rut provided in the format 12345678-9</param>
    /// <returns></returns>
    public static bool CheckRut(this string rut)
    {
        int sum = 0;
        if (!rut.Contains("-"))
        {
            return false;
        }

        string[] parts = rut.Split('-');
        char[] rutNumbers = parts[0].ToCharArray();
        int[] multipliers = new int[] { 2, 3, 4, 5, 6, 7, 2, 3, 4 };

        foreach (char c in rutNumbers)
        {
            //Checks that the first part of the array is a digit
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        for (int i = rutNumbers.Length - 1; i >= 0; i--)
        {
            sum += (int.Parse(rutNumbers[i].ToString()) * multipliers[rutNumbers.Length - (i + 1)]);
        }

        int verifier = 11 - (sum % 11);

        switch (verifier)
        {
            case 10: return parts[1] == "K" || parts[1] == "k";
            case 11: return int.Parse(parts[1]) == 0;
            default: return verifier == int.Parse(parts[1]);
        }
    }
}
