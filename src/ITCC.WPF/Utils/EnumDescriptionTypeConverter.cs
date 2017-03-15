using System;
using System.ComponentModel;

namespace ITCC.WPF.Utils
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value == null)
                    return string.Empty;

                var fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return string.Empty;

                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : value.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
