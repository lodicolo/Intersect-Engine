using System;

namespace Intersect.Server.Database.PlayerData.Groups
{
    public class MissingDataException : Exception
    {
        public MissingDataException() : base("Missing data.") { }

        public MissingDataException(string message) : base(message) { }
    }
}
