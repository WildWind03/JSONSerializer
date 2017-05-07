using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using static System.String;

namespace JSONSerializer
{
    [Serializable]
    public class TestClass
    {
        public TestNestedClass TestNestedClass = new TestNestedClass(3, 4);
        public bool TestBoolean = true;
        public int TestIntegerValue = 5;
        public string TestStringValue = "TestString\"";
        public double Hah = double.NaN; //Accroding to JSON standart NaN values shoudn't be serialized
        public decimal TestDecimalValues = new decimal(0.5);
        public DateTime TestDateTime = DateTime.Now;
        public TestNestedClass TestNestedClassNull = null;

        [NonSerialized] public string Ignore = null; //Non serialized values won't be serialized

        public TestNestedClass[] ArrayMember = {new TestNestedClass(1, 2), new TestNestedClass(3, 4)};
        public List<string> StringList = new List<string>();
        public Dictionary<int, int> Dictionary = new Dictionary<int, int>();
        public List<string> List = new List<string>();

        public TestClass()
        {
            Dictionary.Add(45, 21);
            Dictionary.Add(34, 56);

            StringList.Add("Alexander");
            StringList.Add("Chirikhin");
        }
    }

    public class TestNestedClass
    {
        public int NestedValue1;
        public int NestedValue2;

        public TestNestedClass(int value1, int value2)
        {
            NestedValue1 = value1;
            NestedValue2 = value2;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(JsonSerializer.ToJson(new TestClass()));
            Console.ReadKey();
        }
    }

    public static class JsonSerializer
    {

        private static bool IsNanOrInfinity(this object obj)
        {
            if (obj is double)
            {
                return double.IsNaN((double) obj) || double.IsInfinity((double) obj);
            }

            if (obj is float)
            {
                return float.IsNaN((float) obj) || float.IsInfinity((float) obj);
            }

            return false;
        }

        public static string ToJson(object objectToSerialize)
        {
            if (null == objectToSerialize)
            {
                return "null";
            }

            if (!objectToSerialize.GetType().IsSerializable)
            {
                return null;
            }

            if (objectToSerialize.GetType().IsPrimitive || objectToSerialize is decimal)
            {
                return IsNanOrInfinity(objectToSerialize) ? null : 
                    Format(CultureInfo.InvariantCulture, "{0}", objectToSerialize.ToString().ToLower());
            }

            if (objectToSerialize is string || 
                objectToSerialize is DateTime || 
                objectToSerialize is TimeSpan || 
                objectToSerialize is DateTimeOffset)
            {
                var serializableString = objectToSerialize.ToString().Replace("\"", "\\\"");
                return Concat("\"", serializableString, "\"");
            }

            if (objectToSerialize is IDictionary)
            {
                var jsonDictionaryBuilder = new StringBuilder();
                jsonDictionaryBuilder.Append("{");

                var iDictionary = (IDictionary) objectToSerialize;

                foreach (var i in iDictionary.Keys)
                {
                    var value = iDictionary[i];
                    jsonDictionaryBuilder.Append(Concat("\"", i, "\":", ToJson(value), ","));
                }

                var jsonEnumerableString = jsonDictionaryBuilder.ToString();
                if (jsonEnumerableString[jsonEnumerableString.Length - 1] == ',')
                {
                    jsonEnumerableString = jsonEnumerableString.Substring(0, jsonEnumerableString.Length - 1);
                }

                return jsonEnumerableString + "}";
            }

            if (objectToSerialize is IEnumerable)
            {
                var jsonEnumerableBuilder = new StringBuilder();
                jsonEnumerableBuilder.Append("[");

                foreach (var i in (IEnumerable) objectToSerialize)
                {
                    if (i.GetType().IsSerializable)
                    {
                        jsonEnumerableBuilder.Append(ToJson(i) + ",");
                    }
                }

                var jsonEnumerableString = jsonEnumerableBuilder.ToString();
                if (jsonEnumerableString[jsonEnumerableString.Length - 1] == ',')
                {
                    jsonEnumerableString = jsonEnumerableString.Substring(0, jsonEnumerableString.Length - 1);
                }

                return jsonEnumerableString + "]";
            }

            var jsonBuilder = new StringBuilder();

            jsonBuilder.Append("{");

            var fieldValues = objectToSerialize.GetType().GetFields(BindingFlags.Public |
                                                                    BindingFlags.NonPublic
                                                                    | BindingFlags.Instance);
            foreach (var fieldInfo in fieldValues)
            {
                if (fieldInfo.IsNotSerialized)
                {
                    continue;
                }

                var serializedField = ToJson(fieldInfo.GetValue(objectToSerialize));

                if (null == serializedField)
                {
                    continue;
                }

                jsonBuilder.Append(Concat("\"", fieldInfo.Name, "\":", serializedField, ","));
            }

            var serializedObjects = jsonBuilder.ToString();

            if (serializedObjects[serializedObjects.Length - 1] == ',')
            {
                serializedObjects = serializedObjects.Substring(0, serializedObjects.Length - 1);
            }

            return serializedObjects + "}";
        }
    }
}