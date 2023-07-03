using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ViberAPI
{
    public enum TransliterationType { Gost, ISO }
    public static class Transliteration
    {
        private static Dictionary<string, string> gost = new Dictionary<string, string>(); //ЗАТВЕРДЖЕНО постановою Кабінету Міністрів Українивід 27 січня 2010 р. № 55
        private static Dictionary<string, string> iso = new Dictionary<string, string>(); //ISO 9-95

        public static string Translit(string text)
        {
            return Translit(text, TransliterationType.Gost);
        }

        public static string Translit(string text, TransliterationType type)
        {
            string output = text;
            output = Regex.Replace(output, @"\s|\.|\(", " ");
            output = Regex.Replace(output, @"\s+", " ");
            output = Regex.Replace(output, @"[^\s\w\d-]", "");
            output = output.Trim();
            Dictionary<string, string> tdict = GetDictionaryByType(type);
            foreach (KeyValuePair<string, string> key in tdict) { output = output.Replace(key.Key, key.Value); }
            return output;
        }

        public static string Back(string text)
        {
            return Back(text, TransliterationType.ISO);
        }

        public static string Back(string text, TransliterationType type)
        {
            string output = text;
            Dictionary<string, string> tdict = GetDictionaryByType(type);
            foreach (KeyValuePair<string, string> key in tdict) { output = output.Replace(key.Value, key.Key); }
            return output;
        }

        private static Dictionary<string, string> GetDictionaryByType(TransliterationType type)
        {
            Dictionary<string, string> tdict = iso;
            if (type == TransliterationType.Gost) tdict = gost;
            return tdict;
        }

        static Transliteration()
        {
            gost.Add("А", "A"); iso.Add("А", "A"); gost.Add("а", "a"); iso.Add("а", "a");
            gost.Add("Б", "B"); iso.Add("Б", "B"); gost.Add("б", "b"); iso.Add("б", "b");
            gost.Add("В", "V"); iso.Add("В", "V"); gost.Add("в", "v"); iso.Add("в", "v");
            gost.Add("Г", "H"); iso.Add("Г", "H"); gost.Add("г", "h"); iso.Add("г", "h");
            gost.Add("Ґ", "G"); iso.Add("Ґ", "G"); gost.Add("ґ", "g"); iso.Add("ґ", "g");
            gost.Add("Д", "D"); iso.Add("Д", "D"); gost.Add("д", "d"); iso.Add("д", "d");
            gost.Add("Е", "E"); iso.Add("Е", "E"); gost.Add("е", "e"); iso.Add("е", "e");
            gost.Add("Є", "Ye"); iso.Add("Є", "Ye"); gost.Add("є", "ie"); iso.Add("є", "ie");
            gost.Add("Ё", "Jo"); iso.Add("Ё", "Yo"); gost.Add("ё", "jo"); iso.Add("ё", "yo"); // рус
            gost.Add("Ж", "Zh"); iso.Add("Ж", "Zh"); gost.Add("ж", "zh"); iso.Add("ж", "zh");
            gost.Add("З", "Z"); iso.Add("З", "Z"); gost.Add("з", "z"); iso.Add("з", "z");
            gost.Add("И", "Y"); iso.Add("И", "Y"); gost.Add("и", "y"); iso.Add("и", "y");
            gost.Add("І", "I"); iso.Add("І", "I"); gost.Add("і", "i"); iso.Add("і", "i");
            gost.Add("Ї", "Yi"); iso.Add("Ї", "Yi"); gost.Add("ї", "i"); iso.Add("ї", "i");
            gost.Add("Й", "Y"); iso.Add("Й", "Y"); gost.Add("й", "i"); iso.Add("й", "i");
            gost.Add("К", "K"); iso.Add("К", "K"); gost.Add("к", "k"); iso.Add("к", "k");
            gost.Add("Л", "L"); iso.Add("Л", "L"); gost.Add("л", "l"); iso.Add("л", "l");
            gost.Add("М", "M"); iso.Add("М", "M"); gost.Add("м", "m"); iso.Add("м", "m");
            gost.Add("Н", "N"); iso.Add("Н", "N"); gost.Add("н", "n"); iso.Add("н", "n");
            gost.Add("О", "O"); iso.Add("О", "O"); gost.Add("о", "o"); iso.Add("о", "o");
            gost.Add("П", "P"); iso.Add("П", "P"); gost.Add("п", "p"); iso.Add("п", "p");
            gost.Add("Р", "R"); iso.Add("Р", "R"); gost.Add("р", "r"); iso.Add("р", "r");
            gost.Add("С", "S"); iso.Add("С", "S"); gost.Add("с", "s"); iso.Add("с", "s");
            gost.Add("Т", "T"); iso.Add("Т", "T"); gost.Add("т", "t"); iso.Add("т", "t");
            gost.Add("У", "U"); iso.Add("У", "U"); gost.Add("у", "u"); iso.Add("у", "u");
            gost.Add("Ф", "F"); iso.Add("Ф", "F"); gost.Add("ф", "f"); iso.Add("ф", "f");
            gost.Add("Х", "Kh"); iso.Add("Х", "Kh"); gost.Add("х", "kh"); iso.Add("х", "kh");
            gost.Add("Ц", "Ts"); iso.Add("Ц", "Ts"); gost.Add("ц", "ts"); iso.Add("ц", "ts");
            gost.Add("Ч", "Ch"); iso.Add("Ч", "Ch"); gost.Add("ч", "ch"); iso.Add("ч", "ch");
            gost.Add("Ш", "Sh"); iso.Add("Ш", "Sh"); gost.Add("ш", "sh"); iso.Add("ш", "sh");
            gost.Add("Щ", "Shch"); iso.Add("Щ", "Shch"); gost.Add("щ", "shch"); iso.Add("щ", "shch");
            gost.Add("Ъ", "'"); iso.Add("Ъ", "'"); gost.Add("ъ", ""); iso.Add("ъ", ""); // рус
            gost.Add("Ы", "Y"); iso.Add("Ы", "Y"); gost.Add("ы", "y"); iso.Add("ы", "y"); // рус
            gost.Add("Ь", ""); iso.Add("Ь", ""); gost.Add("ь", ""); iso.Add("ь", "");
            gost.Add("Э", "Eh"); iso.Add("Э", "E"); gost.Add("э", "eh"); iso.Add("э", "e"); // рус
            gost.Add("Ю", "Yu"); iso.Add("Ю", "Yu"); gost.Add("ю", "iu"); iso.Add("ю", "iu");
            gost.Add("Я", "Ya"); iso.Add("Я", "Ya"); gost.Add("я", "ia"); iso.Add("я", "ia");
        }

        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            return true;
        }
    }
}
