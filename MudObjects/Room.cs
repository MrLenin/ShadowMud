using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ShadowMUD.MudObjects
{
    partial class Room : ICloneable
    {
        private readonly List<Character> _characters;

        private readonly Dictionary<Exit, ExitDetail> _exitDetailMap;
        private readonly Dictionary<string, PropertyInfo> _modifiedList;

        private readonly List<Object> _contains;

        private bool _isCopy;

        public Room()
        {
            _exitDetailMap = new Dictionary<Exit, ExitDetail>(10);
            _modifiedList = new Dictionary<string, PropertyInfo>();
            _characters = new List<Character>();
            _contains = new List<Object>();
        }

        public int ZoneID { get; set; }

        public ExitDetail this[Exit exit]
        {
            get
            {
                return _exitDetailMap.ContainsKey(exit) ? _exitDetailMap[exit] : null;
            }

            set
            {
                if (_exitDetailMap.ContainsKey(exit))
                    _exitDetailMap[exit] = value;
            }
        }

        internal IEnumerable<ExitDetail> ExitDetails
        {
            get { return _exitDetailMap.Values; }
        }

        public IEnumerable<PropertyInfo> Modified
        {
            get { return _modifiedList.Values; }
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        public void AddObject(Object obj)
        {
            _contains.Add(obj);
        }

        public void AddCharacter(Character player)
        {
            _characters.Add(player);
        }

        public void RemoveCharacter(Character descriptor)
        {
            _characters.Remove(descriptor);
        }

        public bool ContainsExit(Exit exit)
        {
            return _exitDetailMap.ContainsKey(exit);
        }

        public void AddExitDetail(ExitDetail exitDetail)
        {
            _exitDetailMap[(Exit) exitDetail.ExitDirection] = exitDetail;
        }

        public void Look(Character character, bool brief)
        {
            if (character.Descriptor == null)
                return;

            // TODO: Darkness/blindness testing

            // This should test if the player should actually see world/room numbers and room flags/triggers
            character.Write(string.Format("[{0}, {1}] {2}\r\n\r\n", character.ZoneID, character.RoomID, Title));

            character.Write(Description);

            ListExits(character, false);
            ListCharacters(character);
            ListObjects(character);
        }

        private void ListObjects(Character recvCharacter)
        {
            var sb = new StringBuilder();

            if (_contains.Count > 0)
            {
                sb.Append("On the floor you can see:\r\n");

                foreach (var obj in _contains)
                {
                    sb.Append('\t');
                    sb.Append(obj.Detail.RoomDescription);
                    sb.Append("\r\n");
                }
            }
            else
                sb.Append("There is nothing on the ground that you can see.");

            recvCharacter.Write(sb.ToString());
        }

        private void ListCharacters(Character recvCharacter)
        {
            var sb = new StringBuilder();

            foreach (var character in _characters)
            {
                sb.Append(character.Name);
                sb.Append(character.Title);
                // TODO: Make this get the position state for the character
                sb.Append("is standing here.\r\n");
            }

            recvCharacter.Write(sb.ToString());
        }

        private void ListExits(Character character, bool lookExits)
        {
            var sb = new StringBuilder();

            if (!lookExits)
                sb.Append("\r\nExits: ");

            var iter = 0;

            foreach (var exit in _exitDetailMap.Values)
            {
                if (!lookExits)
                {
                    sb.Append(exit.ExitString);

                    if (iter < (_exitDetailMap.Count - 1))
                        sb.Append(", ");
                }
                else
                {
                    sb.Append(exit.ExitString);
                    sb.Append(" - ");
                    sb.Append(exit.Description);
                    sb.Append("\r\n");
                }

                iter++;
            }

            character.Write(sb.ToString());
            character.Write("\r\n\r\n");
        }

        internal void ClearModified()
        {
            _modifiedList.Clear();
        }

        public Room Clone()
        {
            var room = MemberwiseClone() as Room;

            if (room == null)
                return null;

            room._isCopy = true;

            foreach (var exit in ExitDetail.Directions.Values)
            {
                if (_exitDetailMap.ContainsKey(exit))
                    room[exit] = _exitDetailMap[exit].Clone();
            }

            return room;
        }
    }
}