using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ClinicAppointmentSystem.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.GetName() ?? enumValue.ToString();
        }

        public static Dictionary<int, string> GetDaysOfWeek()
        {
            return Enum.GetValues(typeof(Enums.DayOfWeekEnum))
                .Cast<Enums.DayOfWeekEnum>()
                .ToDictionary(e => (int)e, e => e.GetDisplayName());
        }
    }
}