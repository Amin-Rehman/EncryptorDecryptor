using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

/*
Quick example of usage
            string currentDirectory = Directory.GetCurrentDirectory() + "\\test.json";
            KeyObject newKey = new KeyObject(){ key1 = "Key1Value121211", key2 = "KeyValue22231232" };
            JSONFactory.writeJSONFile(currentDirectory, newKey);

            KeyObject result = JSONFactory.readJSONFile(currentDirectory);
            Console.WriteLine();
*/

namespace SharedProject
{
    /// <summary>
    /// KeyObject an object which can be used to read/write from JSON files
    /// </summary>
    public class KeyObject
    {
        public string key1 { get; set; }
        public string key2 { get; set; }
    }

    public class JSONFactory
    {
        private const string KEY_1 = "Key1";
        private const string KEY_2 = "Key2";

        /// <summary>
        /// Write a JSON file with the respective Key1 and Key2
        /// </summary>
        public static void writeJSONFile(string pathToJSON, KeyObject key)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName(KEY_1);
                    writer.WriteValue(key.key1);
                    writer.WritePropertyName(KEY_2);
                    writer.WriteValue(key.key2);
                    writer.WriteEndObject();
                }

                StreamWriter file = new StreamWriter(pathToJSON);
                file.WriteLine(sb.ToString());
                file.Close();
            }
            catch (Exception e) {
                MessageBox.Show("Exception while writing JSON file: " + e.Message);
                return ;
            }
        }

        /// <summary>
        /// Read a JSON file and return the respective key object with key1 and key2
        /// </summary>
        public static KeyObject readJSONFile(string pathToJSON)
        {
            try
            {
                KeyObject keyObject = new KeyObject();

                StreamReader file = new StreamReader(pathToJSON);
                string fileData = file.ReadToEnd();
                file.Close();
                JsonTextReader reader = new JsonTextReader(new StringReader(fileData));

                bool toSetKey1 = false;
                bool toSetKey2 = false;
                const string TOKEN_PROPERTY_NAME = "PropertyName";
                const string TOKEN_STRING_TYPE = "String";

                while (reader.Read())
                {
                    if (reader.Value != null)
                    {
                        // Check if its PropertyName or String
                        if (reader.TokenType.ToString() == TOKEN_PROPERTY_NAME)
                        {
                            if (reader.Value.ToString() == KEY_1)
                            {
                                toSetKey1 = true;
                                toSetKey2 = false;
                            }
                            else if (reader.Value.ToString() == KEY_2)
                            {
                                toSetKey1 = false;
                                toSetKey2 = true;
                            }
                            else
                            {
                                throw new ArgumentException("Unknown token type found in JSON file");
                            }
                        }
                        else if (reader.TokenType.ToString() == TOKEN_STRING_TYPE)
                        {
                            if (toSetKey1)
                            {
                                keyObject.key1 = reader.Value.ToString();
                            }
                            else if (toSetKey2)
                            {
                                keyObject.key2 = reader.Value.ToString();
                            }
                            else
                            {
                                throw new ArgumentException("Neither key1 nor key2 was read. Unknown entry found in JSON file");
                            }
                        }
                    }

                }

                if (keyObject.key1.Length == 0 || keyObject.key2.Length == 0)
                {
                    throw new ArgumentException("Key 1 or Key2 values not found. Invalid JSON file");
                }

                return keyObject;
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while reading from JSON file: " + e.Message);
                return null;
            }
        }
    }
}
