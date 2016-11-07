using System;

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
                if (!double.TryParse(qPart, out qValue))
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

        public bool Matches(string mimeType)
        {
            var parts = mimeType.Split('/');
            if (parts.Length != 2)
                return false;

            var requestedType = parts[0];
            var requestedSubType = parts[1];

            return MatchesOrStar(requestedType, Type) && MatchesOrStar(requestedSubType, SubType);
        }

        private static bool MatchesOrStar(string lhs, string rhs) => lhs == rhs || rhs == @"*";
    }
}
