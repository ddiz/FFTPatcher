﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ASMEncoding.Helpers
{
    public class ASMProcessEquivalencesResult
    {
        public string[] Lines { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class ASMEquivalenceHelper
    {
        private class ASMAddEquivalenceResult
        {
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }

        private class ASMFindEquivalencesResult
        {
            public int ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }

        public static ASMProcessEquivalencesResult ProcessEquivalences(string[] lines)
        {
            ASMProcessEquivalencesResult result = new ASMProcessEquivalencesResult();
            result.ErrorCode = 0;

            StringBuilder errorTextBuilder = new StringBuilder();

            Dictionary<string, string> eqvMap = new Dictionary<string, string>();

            ASMFindEquivalencesResult findEquivalencesResult = FindEquivalences(eqvMap, lines);
            if (findEquivalencesResult != null)
            {
                if (findEquivalencesResult.ErrorCode > 0)
                {
                    result.ErrorCode = 1;
                    errorTextBuilder.Append(findEquivalencesResult.ErrorMessage);
                }
            }

            List<string> eqvKeys = new List<string>(eqvMap.Keys);
            eqvKeys.Sort((a, b) => a.Length.CompareTo(b.Length));
            eqvKeys.Reverse();

            List<string> newLines = new List<string>();
            foreach (string line in lines)
            {
                string processLine = ASMStringHelper.RemoveLeadingBracketBlock(line);
                processLine = ASMStringHelper.RemoveLeadingSpaces(processLine);
                processLine = ASMStringHelper.RemoveComment(processLine).ToLower();
                string[] parts = ASMStringHelper.SplitLine(processLine);
                bool isEquivalence = ((!string.IsNullOrEmpty(parts[0])) && (parts[0].ToLower().Trim() == ".eqv"));

                string newLine = (string)line.Clone();

                if (!isEquivalence)
                {
                    /*
                    foreach (KeyValuePair<string, string> eqv in eqvDict)
                    {
                        //newLine = newLine.Replace(eqv.Key, eqv.Value);
                        newLine = System.Text.RegularExpressions.Regex.Replace(newLine, System.Text.RegularExpressions.Regex.Escape(eqv.Key), eqv.Value.Replace("$", "$$"), 
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                    */

                    foreach (string eqvKey in eqvKeys)
                    {
                        newLine = System.Text.RegularExpressions.Regex.Replace(newLine, System.Text.RegularExpressions.Regex.Escape(eqvKey), eqvMap[eqvKey].Replace("$", "$$"),
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    }
                }

                newLines.Add(newLine);
            }

            result.ErrorMessage = errorTextBuilder.ToString();
            result.Lines = newLines.ToArray();

            return result;
        }

        private static ASMFindEquivalencesResult FindEquivalences(Dictionary<string, string> eqvDict, string[] lines)
        {
            ASMFindEquivalencesResult result = new ASMFindEquivalencesResult();
            result.ErrorCode = 0;

            StringBuilder errorTextBuilder = new StringBuilder();

            ClearEquivalenceDict(eqvDict);

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string processLine = ASMStringHelper.RemoveLeadingBracketBlock(line);
                processLine = ASMStringHelper.RemoveLeadingSpaces(processLine);
                processLine = ASMStringHelper.RemoveComment(processLine).ToLower();
                string[] parts = ASMStringHelper.SplitLine(processLine);

                ASMAddEquivalenceResult processEquivalenceResult = ProcessEquivalenceStatement(eqvDict, parts);
                if (processEquivalenceResult != null)
                {
                    if (processEquivalenceResult.ErrorCode > 0)
                    {
                        result.ErrorCode = 1;
                        errorTextBuilder.Append(processEquivalenceResult.ErrorMessage);
                    }
                }
            }

            result.ErrorMessage = errorTextBuilder.ToString();
            return result;
        }

        private static ASMAddEquivalenceResult ProcessEquivalenceStatement(Dictionary<string, string> eqvDict, string[] parts)
        {
            if (!string.IsNullOrEmpty(parts[0]))
            {
                if (parts[0] == ".eqv")
                {
                    try
                    {
                        string[] innerParts = parts[1].Split(',');
                        if (!parts[1].Contains(","))
                        {
                            ASMAddEquivalenceResult errorResult = new ASMAddEquivalenceResult();
                            errorResult.ErrorCode = 1;
                            errorResult.ErrorMessage = "WARNING: Ignoring .eqv statement with bad argument list (no comma): \"" + parts[1] + "\"\r\n";
                            return errorResult;
                        }

                        string eqvName = ASMStringHelper.RemoveSpaces(innerParts[0]);
                        string eqvValue = ASMStringHelper.RemoveSpaces(innerParts[1]);
                        return AddEquivalenceGeneric(eqvDict, eqvName, eqvValue);
                    }
                    catch (Exception ex)
                    {
                        ASMAddEquivalenceResult errorResult = new ASMAddEquivalenceResult();
                        errorResult.ErrorCode = 1;
                        errorResult.ErrorMessage = "Error on .eqv statement: " + ex.Message + "\r\n";
                        return errorResult;
                    }
                }
            }

            return null;
        }

        private static ASMAddEquivalenceResult AddEquivalenceGeneric(Dictionary<string, string> eqvDict, string eqvName, string eqvValue)
        {
            ASMAddEquivalenceResult result = new ASMAddEquivalenceResult();

            try
            {
                eqvName = ASMStringHelper.RemoveSpaces(eqvName); //.ToUpper();
                if (!eqvDict.ContainsKey(eqvName))
                {
                    eqvDict.Add(eqvName, eqvValue);
                }
                else
                {
                    eqvDict[eqvName] = eqvValue;
                }
            }
            catch
            {
                result.ErrorCode = 1;
                result.ErrorMessage = "Error adding equivalence " + eqvName + ": Equivalence already exists!\r\n";
                return result;
            }

            result.ErrorCode = 0;

            return result;
        }

        private static void ClearEquivalenceDict(Dictionary<string, string> eqvDict)
        {
            eqvDict.Clear();
        }
    }
}
