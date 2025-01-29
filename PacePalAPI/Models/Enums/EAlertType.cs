namespace PacePalAPI.Models.Enums
{
    public enum EAlertType
    {
        DangerousWeather,
        WildAnimals,
        RoadBlock,
        PersonalEmergency,
        Other
    }

    public static class EAlertTypeExtensions
    {
        public static string ToStringValue(this EAlertType alertType)
        {
            return alertType.ToString();
        }

        public static bool TryFromString(string value, out EAlertType alertType)
        {
            return Enum.TryParse(value, true, out alertType);
        }

        public static EAlertType FromString(string value)
        {
            if (Enum.TryParse(value, true, out EAlertType result))
            {
                return result;
            }
            throw new ArgumentException($"Invalid alert type: {value}");
        }
    }
}
