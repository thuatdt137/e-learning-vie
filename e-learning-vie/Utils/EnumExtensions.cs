using e_learning_vie.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace e_learning_vie.Utils
{
    public static class EnumExtensions
    {
        public static string? GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .Name;
        }

        public static ConductLevel? GetConductEnumFromDisplayName(string displayName)
        {
            foreach(var field in typeof(ConductLevel).GetFields())
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if(attr != null && attr.Name == displayName)
                {
                    return (ConductLevel)field.GetValue(null);
                }
            }

            return null;
        }

    }
}
