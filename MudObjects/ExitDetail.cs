using System;
using System.Collections.Generic;

namespace ShadowMUD.MudObjects
{
    public enum Exit
    {
        North = 0,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        Up,
        Down
    }

    public partial class ExitDetail : IComparable<ExitDetail>
    {
        #region Directions
        public static readonly Dictionary<string, Exit> Directions = new Dictionary<string, Exit>(10)
        {
            {"north", Exit.North},
            {"northeast", Exit.NorthEast},
            {"east", Exit.East},
            {"southeast", Exit.SouthEast},
            {"south", Exit.South},
            {"southwest", Exit.SouthWest},
            {"west", Exit.West},
            {"northwest", Exit.NorthWest},
            {"up", Exit.Up},
            {"down", Exit.Down}
        }; 
        #endregion

        #region ExitStrings
        private static readonly Dictionary<Exit, string> ExitStrings = new Dictionary<Exit, string>
        {
            {Exit.North, "North"},
            {Exit.NorthEast, "Northeast"},
            {Exit.East, "East"},
            {Exit.SouthEast, "Southeast"},
            {Exit.South, "South"},
            {Exit.SouthWest, "Southwest"},
            {Exit.West, "West"},
            {Exit.NorthWest, "Northwest"},
            {Exit.Up, "Up"},
            {Exit.Down, "Down"}
        }; 
        #endregion

        #region Opposites
        public static readonly Dictionary<Exit, Exit> Opposites = new Dictionary<Exit, Exit>(10)
        {
            {Exit.North, Exit.South},
            {Exit.NorthEast, Exit.SouthWest},
            {Exit.East, Exit.West},
            {Exit.SouthEast, Exit.NorthWest},
            {Exit.South, Exit.North},
            {Exit.SouthWest, Exit.NorthEast},
            {Exit.West, Exit.East},
            {Exit.NorthWest, Exit.SouthEast},
            {Exit.Up, Exit.Down},
            {Exit.Down, Exit.Up}
        }; 
        #endregion

        #region  Keywords

        public ExitKeyword this[int index]
        {
            get { if (_exitKeywords.Count > index) { return _exitKeywords[index]; } return null; }
            set { if (_exitKeywords.Count > index) _exitKeywords[index] = value; }
        }

        internal IEnumerable<ExitKeyword> Keywords { get { return _exitKeywords; } }

        public void AddExitKeyword(ExitKeyword exitKeyword) { _exitKeywords.Add(exitKeyword); }

        #endregion

        public string ExitString
        {
            get { return ExitStrings[(Exit) ExitDirection]; }
        }

        public int ZoneID { get; internal set; }

        #region IComparable<ExitDetail> Members

        public int CompareTo(ExitDetail other)
        {
            return ExitDirection.CompareTo(other.ExitDirection);
        }

        #endregion
    }
}