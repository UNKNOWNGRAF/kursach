using System;
using System.Text;
namespace WebApplication1.Services
{
    public class VigenereCipherService
    {
        private const string DEFAULT_RUSSIAN_KEY = "СЕКРЕТНЫЙКЛЮЧ";
        private const string DEFAULT_ENGLISH_KEY = "SECRETKEY";
        private const string RUSSIAN_ALPHABET = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        private const string ENGLISH_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string Encrypt(string plainText, string key = null)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            bool hasRussian = false;
            bool hasEnglish = false;
            foreach (char c in plainText)
            {
                if (IsRussianLetter(c)) hasRussian = true;
                else if (IsEnglishLetter(c)) hasEnglish = true;
                if (hasRussian && hasEnglish) break;
            }

            if (key == null)
            {
                if (hasRussian) key = DEFAULT_RUSSIAN_KEY;
                else if (hasEnglish) key = DEFAULT_ENGLISH_KEY;
                else key = DEFAULT_RUSSIAN_KEY;
            }

            key = key.ToUpper();
            StringBuilder encryptedText = new StringBuilder();
            int keyIndex = 0;

            foreach (char character in plainText)
            {
                if (IsRussianLetter(character))
                {
                    char upperChar = char.ToUpper(character);
                    bool isLower = char.IsLower(character);

                    int alphabetIndex = RUSSIAN_ALPHABET.IndexOf(upperChar);
                    if (alphabetIndex >= 0)
                    {
                        char keyChar = GetNextKeyChar(key, ref keyIndex, true);
                        int keyIndexInAlphabet = RUSSIAN_ALPHABET.IndexOf(keyChar);
                        int encryptedIndex = (alphabetIndex + keyIndexInAlphabet) % RUSSIAN_ALPHABET.Length;

                        char encryptedChar = RUSSIAN_ALPHABET[encryptedIndex];
                        encryptedText.Append(isLower ? char.ToLower(encryptedChar) : encryptedChar);
                    }
                    else
                    {
                        encryptedText.Append(character);
                    }
                }
                else if (char.IsLetter(character) && IsEnglishLetter(character))
                {
                    char offset = char.IsUpper(character) ? 'A' : 'a';
                    char keyChar = GetNextKeyChar(key, ref keyIndex, false);
                    int keyCharValue = char.ToUpper(keyChar) - 'A';
                    int plainChar = character - offset;
                    int encryptedChar = (plainChar + keyCharValue) % 26;

                    encryptedText.Append((char)(encryptedChar + offset));
                }
                else
                {
                    encryptedText.Append(character);
                }
            }

            return encryptedText.ToString();
        }
        public bool ContainsAtLeastOneLetter(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            foreach (char c in text)
            {
                if (IsRussianLetter(c) || IsEnglishLetter(c))
                    return true;
            }
            return false;
        }
        public string Decrypt(string encryptedText, string key = null)
        {
            if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

            bool hasRussian = false;
            bool hasEnglish = false;
            foreach (char c in encryptedText)
            {
                if (IsRussianLetter(c)) hasRussian = true;
                else if (IsEnglishLetter(c)) hasEnglish = true;
                if (hasRussian && hasEnglish) break;
            }

            if (key == null)
            {
                if (hasRussian) key = DEFAULT_RUSSIAN_KEY;
                else if (hasEnglish) key = DEFAULT_ENGLISH_KEY;
                else key = DEFAULT_RUSSIAN_KEY;
            }

            key = key.ToUpper();
            StringBuilder decryptedText = new StringBuilder();
            int keyIndex = 0;

            foreach (char character in encryptedText)
            {
                if (IsRussianLetter(character))
                {
                    char upperChar = char.ToUpper(character);
                    bool isLower = char.IsLower(character);

                    int alphabetIndex = RUSSIAN_ALPHABET.IndexOf(upperChar);
                    if (alphabetIndex >= 0)
                    {
                        char keyChar = GetNextKeyChar(key, ref keyIndex, true);
                        int keyIndexInAlphabet = RUSSIAN_ALPHABET.IndexOf(keyChar);
                        int decryptedIndex = (alphabetIndex - keyIndexInAlphabet + RUSSIAN_ALPHABET.Length) % RUSSIAN_ALPHABET.Length;

                        char decryptedChar = RUSSIAN_ALPHABET[decryptedIndex];
                        decryptedText.Append(isLower ? char.ToLower(decryptedChar) : decryptedChar);
                    }
                    else
                    {
                        decryptedText.Append(character);
                    }
                }
                else if (char.IsLetter(character) && IsEnglishLetter(character))
                {
                    char offset = char.IsUpper(character) ? 'A' : 'a';
                    char keyChar = GetNextKeyChar(key, ref keyIndex, false);
                    int keyCharValue = char.ToUpper(keyChar) - 'A';
                    int encryptedChar = character - offset;
                    int decryptedChar = (encryptedChar - keyCharValue + 26) % 26;

                    decryptedText.Append((char)(decryptedChar + offset));
                }
                else
                {
                    decryptedText.Append(character);
                }
            }

            return decryptedText.ToString();
        }

        private char GetNextKeyChar(string key, ref int keyIndex, bool isRussian)
        {
            while (true)
            {
                char keyChar = key[keyIndex % key.Length];
                keyIndex++;

                if (isRussian && IsRussianLetter(keyChar))
                    return char.ToUpper(keyChar);
                else if (!isRussian && IsEnglishLetter(keyChar))
                    return char.ToUpper(keyChar);

                if (keyIndex >= key.Length * 2)
                    return isRussian ? 'А' : 'A';
            }
        }

        private bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') || (c >= 'а' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        private bool IsEnglishLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        public bool ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            foreach (char c in key)
            {
                if (!IsRussianLetter(c) && !IsEnglishLetter(c))
                    return false;
            }

            return true;
        }

        public string GetSupportedAlphabets()
        {
            return "Поддерживаются русский (А-Я, а-я, Ёё) и английский (A-Z, a-z) алфавиты ";
        }
    }
}