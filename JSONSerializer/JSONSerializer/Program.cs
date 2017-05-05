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
        public int I = 5;
        public string S = "Hey";

        [NonSerialized] public string Ignore; // это поле не должно сериализоваться

        public MyClass[] ArrayMember = {new MyClass(), new MyClass(), new MyClass()};
        public List<int> MyList = new List<int>(5);
        public List<string> Names = new List<string>();
        public List<MyClass> MyClassList = new List<MyClass>();

        public Dictionary<int, int> Dictionary = new Dictionary<int, int>();

        public TestClass()
        {
            MyList.Add(56);
            MyList.Add(32);
            Dictionary.Add(45, 21);
            Dictionary.Add(34, 56);
            Names.Add("Alexander");
            Names.Add("Chirikhin");

            MyClassList.Add(new MyClass());
            MyClassList.Add(new MyClass());
        }
    }

    class MyClass
    {
        public int Ff = 78;
        public int HeyHey = 98;
    }

    class AnotherClass
    {
        public TestClass TestClass = new TestClass();
        public TestClass TestClass1 = new TestClass();
    }

    public class Program
    {
        public static void Main(string[] args)
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
            var jsonBuilder = new StringBuilder();

            jsonBuilder.Append(GenerateTabs(order) + "{\n");

            var fieldValues = objectToSerialize.GetType().GetFields(BindingFlags.Public |
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.Instance);

            var countOfFields = fieldValues.Length;
            var currentFieldNum = 0;

            foreach (var fieldInfo in fieldValues)
            {
                currentFieldNum++;
                if (fieldInfo.IsNotSerialized) continue;
                if (fieldInfo.FieldType.IsPrimitive || fieldInfo.FieldType == typeof(string)
                    || fieldInfo.FieldType == typeof(decimal) || fieldInfo.FieldType == typeof(DateTime)
                    || fieldInfo.FieldType == typeof(TimeSpan) || fieldInfo.FieldType == typeof(DateTimeOffset))
                {
                    var newFieldValue = Concat(GenerateTabs(order + 1), "\"", fieldInfo.Name + "\": \"",
                        fieldInfo.GetValue(objectToSerialize), "\",\n");

                    jsonBuilder.Append(newFieldValue);
                }
                else if (fieldInfo.FieldType.IsArray || fieldInfo.GetValue(objectToSerialize) is IList)
                {
                    var enumerable = (IList) fieldInfo.GetValue(objectToSerialize);
                    if (null == enumerable) continue;
                    var serializedObjectsBuilder = new StringBuilder();
                    serializedObjectsBuilder.Append(Concat(GenerateTabs(order + 1), "\"", fieldInfo.Name, "\": ", "["));

                    var currentSize = 0;
                    var maxSize = enumerable.Count;


                    foreach (var i in enumerable)
                    {
                        if (i.GetType().IsPrimitive)
                        {
                            serializedObjectsBuilder.Append(i);
                            if (currentSize < maxSize - 1)
                            {
                                serializedObjectsBuilder.Append(", ");
                            }

                            if (currentSize++ >= maxSize - 1)
                            {
                                if (currentFieldNum < countOfFields - 1)
                                {
                                    serializedObjectsBuilder.Append("],\n");
                                }
                                else
                                {
                                    serializedObjectsBuilder.Append("]\n");
                                }
                            }
                        }
                        else if (i is string)
                        {
                            serializedObjectsBuilder.Append(Concat("\"", i, "\""));
                            if (currentSize < maxSize - 1)
                            {
                                serializedObjectsBuilder.Append(", ");
                            }

                            if (currentSize++ >= maxSize - 1)
                            {
                                if (currentFieldNum < countOfFields - 1)
                                {
                                    serializedObjectsBuilder.Append("],\n");
                                }
                                else
                                {
                                    serializedObjectsBuilder.Append("]\n");
                                }
                            }
                        }
                        else
                        {
                            if (currentSize == 0)
                            {
                                serializedObjectsBuilder.Append("\n");
                            }

                            serializedObjectsBuilder.Append(ToJson(i, order + 2));

                            if (currentSize < maxSize - 1)
                            {
                                serializedObjectsBuilder.Append(",\n");
                            }
                            else
                            {
                                serializedObjectsBuilder.Append("\n");
                            }

                            if (currentSize++ >= maxSize - 1)
                            {
                                if (currentFieldNum < countOfFields - 1)
                                {
                                    serializedObjectsBuilder.Append(GenerateTabs(order + 1) + "],\n");

                                }
                                else
                                {
                                    serializedObjectsBuilder.Append(GenerateTabs(order + 1) + "]\n");
                                }
                            }
                        }
                    }

                    jsonBuilder.Append(serializedObjectsBuilder);
                }
                else if (fieldInfo.GetValue(objectToSerialize) is IDictionary)
                {

                }
                else
                {
                    string serializedObject;
                    if (currentFieldNum < countOfFields)
                    {
                        serializedObject = ToJson(fieldInfo.GetValue(objectToSerialize), order + 1) + ",\n";
                    }
                    else
                    {
                        serializedObject = ToJson(fieldInfo.GetValue(objectToSerialize), order + 1) + "\n";
                    }

                    jsonBuilder.Append(serializedObject);
                }
            }

            jsonBuilder.Append(GenerateTabs(order) + "}");
            return jsonBuilder.ToString();
        }

        private static string GenerateTabs(int count)
        {
            StringBuilder tabsBuilder = new StringBuilder();
            for (int k = 0; k < count; ++k)
            {
                tabsBuilder.Append("\t");
            }

            return tabsBuilder.ToString();
        }
    }
}