using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.String;

namespace JSONSerializer
{
    [Serializable]
    class TestClass
    {
        public int i = 5;
        public string s = "Hey";

        [NonSerialized] public string ignore; // это поле не должно сериализоваться

        public MyClass[] arrayMember = {new MyClass(), new MyClass(), new MyClass()};
        public List<int> myList = new List<int>(5);
        public List<String> names = new List<string>();
        public List<MyClass> myClassList = new List<MyClass>();

        public Dictionary<int, int> dictionary = new Dictionary<int, int>();

        public TestClass()
        {
            myList.Add(56);
            myList.Add(32);
            dictionary.Add(45, 21);
            dictionary.Add(34, 56);
            names.Add("Alexander");
            names.Add("Chirikhin");

            myClassList.Add(new MyClass());
            myClassList.Add(new MyClass());
        }
    }

    class MyClass
    {
        public int Ff = 78;
        public int HeyHey = 98;
    }

    class AnotherClass
    {
        public TestClass testClass = new TestClass();
        public TestClass testClass1 = new TestClass();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(JsonSerializer.ToJson(new AnotherClass()));
            Console.ReadKey();
        }
    }

    public static class JsonSerializer
    {
        public static string ToJson(object objectToSerialize)
        {
            return ToJson(objectToSerialize, 0);
        }

        private static string ToJson(object objectToSerialize, int order)
        {
            string json = GenerateTabs(order) + "{\n";
            var fieldValues = objectToSerialize.GetType().GetFields(BindingFlags.Public |
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.Instance);

            int countOfFields = fieldValues.Length;
            int currentFieldNum = 0;

            foreach (FieldInfo fieldInfo in fieldValues)
            {
                currentFieldNum++;
                if (!fieldInfo.IsNotSerialized)
                {
                    if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType == typeof(string)
                        || fieldInfo.FieldType == typeof(decimal) || fieldInfo.FieldType == typeof(DateTime)
                        || fieldInfo.FieldType == typeof(TimeSpan) || fieldInfo.FieldType == typeof(DateTimeOffset))
                    {
                        var newFieldValue = Concat(GenerateTabs(order + 1), "\"", fieldInfo.Name + "\": \"",
                            fieldInfo.GetValue(objectToSerialize), "\",\n");

                        json += newFieldValue;
                    }
                    else if (fieldInfo.FieldType.IsArray || fieldInfo.GetValue(objectToSerialize) is IList)
                    {
                        var enumerable = (IList) fieldInfo.GetValue(objectToSerialize);
                        if (null != enumerable)
                        {
                            string serializedObjects = GenerateTabs(order + 1) + "\"" + fieldInfo.Name + "\": " + "[";

                            int currentSize = 0;
                            int maxSize = enumerable.Count;


                            foreach (var i in enumerable)
                            {
                                if (i.GetType().IsPrimitive)
                                {
                                    serializedObjects += i;
                                    if (currentSize < maxSize - 1)
                                    {
                                        serializedObjects += ", ";
                                    }

                                    if (currentSize++ >= maxSize - 1)
                                    {
                                        if (currentFieldNum < countOfFields - 1)
                                        {
                                            serializedObjects += "],\n";
                                        }
                                        else
                                        {
                                            serializedObjects += "]\n";
                                        }
                                    }
                                }
                                else if (i is string)
                                {
                                    serializedObjects += (" " + "\"" + i + "\"");
                                    if (currentSize < maxSize - 1)
                                    {
                                        serializedObjects += ", ";
                                    }

                                    if (currentSize++ >= maxSize - 1)
                                    {
                                        if (currentFieldNum < countOfFields - 1)
                                        {
                                            serializedObjects += "],\n";
                                        }
                                        else
                                        {
                                            serializedObjects += "]\n";
                                        }
                                    }
                                }
                                else
                                {
                                    if (currentSize == 0)
                                    {
                                        serializedObjects += "\n";
                                    }

                                    serializedObjects += ToJson(i, order + 2);

                                    if (currentSize < maxSize - 1)
                                    {
                                        serializedObjects += ",\n";
                                    }
                                    else
                                    {
                                        serializedObjects += "\n";
                                    }

                                    if (currentSize++ >= maxSize - 1)
                                    {
                                        if (currentFieldNum < countOfFields - 1)
                                        {
                                            serializedObjects += GenerateTabs(order + 1) + "],\n";

                                        }
                                        else
                                        {
                                            serializedObjects += GenerateTabs(order + 1) + "]\n";
                                        }
                                    }
                                }
                            }

                            json += serializedObjects;
                        }
                    }
                    else if (fieldInfo.GetValue(objectToSerialize) is IDictionary)
                    {

                    }
                    else
                    {
                        if (currentFieldNum < countOfFields)
                        {
                            string serializedObject = ToJson(fieldInfo.GetValue(objectToSerialize), order + 1) + ",\n";
                            json += serializedObject;
                        }
                        else
                        {
                            string serializedObject = ToJson(fieldInfo.GetValue(objectToSerialize), order + 1) + "\n";
                            json += serializedObject;
                        }
                    }
                }
            }

            json += GenerateTabs(order) + "}";
            return json;
        }

        private static String GenerateTabs(int count)
        {
            string tabs = "";
            for (int k = 0; k < count; ++k)
            {
                tabs += "\t";
            }

            return tabs;
        }
    }
}