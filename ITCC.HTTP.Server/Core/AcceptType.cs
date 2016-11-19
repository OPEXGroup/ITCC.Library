using System;
using ITCC.HTTP.Server.Enums;

namespace ITCC.HTTP.Server.Core
{
    internal class AcceptType
    {
        public double Qvalue { get; set; } = 1;
        public string Type { get; set; } = "*";
        public string SubType { get; set; } = "*";

        public string MediaRange
        {
            get { return $"{Type}/{SubType}"; }
            set
            {
                var parts = value.Split('/');
                if (parts.Length != 2)
                    throw new ArgumentException($@"Bad media-range: {value}", nameof(value));

                Type = parts[0];
                SubType = parts[1];
            }
        }

        // See RFC 2616, 14.1
        public static AcceptType Parse(string stringValue)
        {
            var result = new AcceptType();
            var parts = stringValue.Split(';');

            if (parts.Length > 2)
                return null;

            if (parts.Length == 2)
            {
                var qPart = parts[1];
                if (!qPart.StartsWith("q="))
                    return null;

                double qValue;
                if (!double.TryParse(qPart.Substring(2), out qValue))
                    return null;

                result.Qvalue = qValue;
            }

            try
            {
                result.MediaRange = parts[0];
            }
            catch (Exception)
            {
                return null;
            }
            
            return result;
        }

        public AcceptTypeMatch Matches(string mimeType)
        {
            var parts = mimeType.Split('/');
            if (parts.Length != 2)
                return AcceptTypeMatch.NotMatch;

            var requestedType = parts[0];
            var requestedSubType = parts[1];

            var result = AcceptTypeMatch.NotMatch;

            if (requestedType == Type)
                result |= AcceptTypeMatch.TypeMatch;
            else if (Type == "*")
                result |= AcceptTypeMatch.TypeStar;
            else
                return AcceptTypeMatch.NotMatch;

            if (requestedSubType == SubType)
                result |= AcceptTypeMatch.SubtypeMatch;
            else if (SubType == "*")
                result |= AcceptTypeMatch.SubtypeStar;
            else
                return AcceptTypeMatch.NotMatch;

            return result;
        }
    }
}
