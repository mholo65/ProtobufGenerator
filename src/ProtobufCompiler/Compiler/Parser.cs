﻿using System.Linq;

namespace ProtobufCompiler.Compiler
{
    internal class Parser
    {
        internal bool IsHexDigit(char input)
        {
            return IsDecimalDigit(input) ||
                   ('a' <= input) && (input <= 'f') ||
                   ('A' <= input) && (input <= 'F');
        }

        internal bool IsOctalDigit(char input)
        {
            return ('0' <= input) && (input <= '7');
        }

        internal bool IsDecimalDigit(char input)
        {
            return ('0' <= input) && (input <= '9');
        }

        internal bool IsLetter(char input)
        {
            return ('a' <= input) && (input <= 'z') ||
                   ('A' <= input) && (input <= 'Z');
        }

        internal bool IsCapital(char input)
        {
            return ('A' <= input) && (input <= 'Z');
        }

        internal bool IsIdentifierChar(char input)
        {
            return IsDecimalDigit(input) || IsLetter(input) || ('_'.Equals(input));
        }

        internal bool IsIdentifier(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && IsLetter(input[0]) &&
                   input.All(IsIdentifierChar);
        }

        internal bool IsFullIdentifier(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Split('.').All(IsIdentifier);
        }

        internal bool IsNamespacedIdentifier(string input)
        {
            return !string.IsNullOrWhiteSpace(input) &&
                   ((input[0] == '.')
                       ? input.Split('.').Skip(1).All(IsIdentifier)
                       : input.Split('.').All(IsIdentifier));
        }

        internal bool IsDecimalLiteral(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.All(IsDecimalDigit);
        }

        internal bool IsOctalLiteral(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && '0'.Equals(input[0]) &&
                   input.Skip(1).All(IsOctalDigit);
        }

        internal bool IsHexLiteral(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length >= 3 &&
                   '0'.Equals(input[0]) && ('x'.Equals(input[1]) || 'X'.Equals(input[1])) &&
                   input.Skip(2).All(IsHexDigit);
        }

        internal bool IsIntegerLiteral(string input)
        {
            return !string.IsNullOrWhiteSpace(input) &&
                   (IsDecimalLiteral(input) || IsHexLiteral(input) || IsOctalLiteral(input));
        }

        internal bool IsExponent(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length >= 2 &&
                   ('e'.Equals(input[0]) || 'E'.Equals(input[0])) &&
                   ('-'.Equals(input[1]) || '+'.Equals(input[1]) || input.Skip(1).All(IsDecimalDigit));
        }

        internal bool IsDecimals(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.All(IsDecimalDigit);
        }

        internal bool IsFloatLiteral(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!input.Contains('.')) return false; // No '.' case
            if (input.Length < 2) return false; // Min 1 digit and 1 '.'
            
            // Scan to non-decimal
            var sepPlus = input.SkipWhile(IsDecimalDigit).ToArray();

            if ('.'.Equals(sepPlus[0]))
            {
                if (sepPlus.Length == 1) return true; // "1." case
                var trailingDec = sepPlus.SkipWhile(IsDecimalDigit).ToArray();
                if (!trailingDec.Any()) return true; // "1.23438" case
                if ('e'.Equals(trailingDec[0]) || 'E'.Equals(trailingDec[0])) return IsExponent(trailingDec.ToString());
            }
            else if ('e'.Equals(sepPlus[0]) || 'E'.Equals(sepPlus[0]))
            {
                return IsExponent(sepPlus.ToString());
            }
            return false;
        }

        internal bool IsBoolean(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && ("true".Equals(input) || "false".Equals(input));
        }

        internal bool IsCharEscape(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length == 2 &&
                   '\\'.Equals(input[0]) &&
                   ('a'.Equals(input[1]) || 'b'.Equals(input[1]) || 'f'.Equals(input[1]) || 'n'.Equals(input[1]) ||
                    'r'.Equals(input[1]) || 't'.Equals(input[1]) || 'v'.Equals(input[1]) || '\\'.Equals(input[1]) ||
                    '\''.Equals(input[1]) || '"'.Equals(input[1]));
        }

        internal bool IsOctalEscape(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length == 4 &&
                   '\\'.Equals(input[0]) && input.Skip(1).All(IsOctalDigit);
        }

        internal bool IsHexEscape(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length == 4 &&
                   '\\'.Equals(input[0]) && ('x'.Equals(input[1]) || 'X'.Equals(input[1])) &&
                   input.Skip(2).All(IsHexDigit);
        }

        internal bool IsCharValue(string input)
        {
            char throwaway;
            return !string.IsNullOrWhiteSpace(input) &&
                   (
                       IsHexEscape(input) || IsOctalEscape(input) || IsCharEscape(input) ||
                       (input.Length == 1 && char.TryParse(input, out throwaway))
                       );
        }

        internal bool IsQuote(char input)
        {
            return '\''.Equals(input) || '"'.Equals(input);
        }

        internal bool IsStringLiteral(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.First().Equals(input.Last()) &&
                   IsQuote(input.First()) && input.Skip(1).Take(input.Length - 2).All(t => IsCharValue(t.ToString()));
        }

        internal bool IsEmptyStatement(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length == 1 && input[0] == ';';
        }
    }
}