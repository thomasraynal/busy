using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Busy.Infrastructure
{
    internal static class ExtendEnum
    {
        public static string GetAttributeDescription(this Enum enumValue)
        {
            var attribute = enumValue.GetAttributeOfType<DescriptionAttribute>();

            return attribute == null ? String.Empty : attribute.Description;
        }

        private static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var enumType = enumVal.GetType();
            var memberInfo = enumType.GetMember(enumVal.ToString());

            return memberInfo[0].GetAttribute<T>(false);
        }
    }
}
