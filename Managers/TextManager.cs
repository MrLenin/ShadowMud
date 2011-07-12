using System.Data;
using System.Linq;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Managers
{
    internal class TextManager
    {
        private const string SelectTextsFormat = "SELECT * FROM text_table WHERE \"name\" = '{0}';";
        private readonly ShadowDb _database;

        public TextManager(IDbConnection connection)
        {
            _database = new ShadowDb(connection);
        }

        public string LoadText(string name)
        {
            var query = string.Format(SelectTextsFormat, name);

            var texts = _database.ExecuteQuery<Text>(query);

            if (texts.Count() == 0)
                return null;

            var text = texts.First();

            return text.Data;
        }
    }
}