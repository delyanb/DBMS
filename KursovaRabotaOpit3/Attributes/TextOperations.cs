using KursovaRabotaOpit3.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

 namespace KursovaRabotaOpit3.Attributes;

public static class TextOperations
{
    public static string MyTrim(string str, char ch)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        int startIndex = 0;
        int endIndex = str.Length - 1;


        while (startIndex <= endIndex && str[startIndex] == ch)
        {
            startIndex++;
        }


        while (endIndex >= startIndex && str[endIndex] == ch)
        {
            endIndex--;
        }


        if (startIndex > endIndex)
        {
            return string.Empty;
        }


        return str.Substring(startIndex, endIndex - startIndex + 1);
    }
    public static string MyTrim(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        int startIndex = 0;
        int endIndex = str.Length - 1;


        while (startIndex <= endIndex && char.IsWhiteSpace(str[startIndex]))
        {
            startIndex++;
        }


        while (endIndex >= startIndex && char.IsWhiteSpace(str[endIndex]))
        {
            endIndex--;
        }


        if (startIndex > endIndex)
        {
            return string.Empty;
        }

        
        return str.Substring(startIndex, endIndex - startIndex + 1);
    }
    public static string[] MySplit(string str, char ch)
    {
        DbList<string> result = new DbList<string>();
        int startIndex = 0;

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == ch)
            {
                result.Add(str.Substring(startIndex, i - startIndex));
                startIndex = i + 1;
            }
        }


        result.Add(str.Substring(startIndex));

        return result.ToArray;
    }
    public static string[] MySplit2(string str)
    {
        DbList<string> tokens = new DbList<string>();
        bool insideParentheses = false;
        int startIndex = 0;

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '(')
            {
                insideParentheses = true;
            }
            else if (str[i] == ')')
            {
                insideParentheses = false;
            }
            else if (str[i] == ' ' && !insideParentheses)
            {
                
                tokens.Add(str.Substring(startIndex, i - startIndex));
                startIndex = i + 1;
            }
        }

        
        tokens.Add(str.Substring(startIndex));

        return tokens.ToArray() ;
    }

    public static string GetLastCharacters(string input, int length)
    {
        if (input.Length < length)
        {
            // If the length is greater than the string length, return the whole string
            return input;
        }

        // Use Substring to get the last N characters
        return input.Substring(input.Length - length);
    }

    public static string[] MySplitUntil(string str, char ch, string end)
    {
        DbList<string> result = new DbList<string>();
        int startIndex = 0;
        bool found = false;

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == ch)
            {
                if(str.Substring(startIndex, i - startIndex)==end)
                {
                    found = true;
                    break;
                }
                result.Add(str.Substring(startIndex, i - startIndex));
                startIndex = i + 1;
            }
        }
        if(str.Substring(startIndex)==end)
        {
            found = true;
        }
        if(!found)
        {
            throw new Exception("couldn't find: " + end);
        }
        


        

        return result.ToArray;
    }
    public static string MyTrimEnd(string str, char ch)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        int endIndex = str.Length - 1;


        while (endIndex >= 0 && str[endIndex] == ch)
        {
            endIndex--;
        }


        if (endIndex < 0)
        {
            return string.Empty;
        }


        return str.Substring(0, endIndex + 1);
    }
    public static string MyTrimEnd(string str, string substring)
    {
       

        if (str.EndsWith(substring))
        {
            return str.Substring(0, str.Length - substring.Length);
        }
        return str;
    }
}
