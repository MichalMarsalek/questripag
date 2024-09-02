//using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using Xunit.Sdk;

namespace Questripag.Tests
{
    public class KeyedTestCasesAttribute : DataAttribute
    {
        public string MemberName { get; set; }
        public KeyedTestCasesAttribute(string memberName) {
            MemberName = memberName;
        }
        public override IEnumerable<object[]>? GetData(MethodInfo testMethod)
        {
            var field = testMethod.DeclaringType.GetField(MemberName, BindingFlags.Public | BindingFlags.Static);
            var valueType = field.FieldType;
            if ((valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>) && valueType.GenericTypeArguments[0] == typeof(string)))
            {
                return ((IEnumerable<string>)valueType.GetProperty("Keys").GetValue(field.GetValue(null))).Select(x => new object[] { x });
            }
            return null;
        }
    }
}
