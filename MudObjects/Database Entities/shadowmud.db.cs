// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from shadowdb on 07/08/2008 3:03:16 AM.
// Please visit http://linq.to/db for more information.
//
using System.Data;
using DbLinq.Data.Linq;
using DbLinq.Vendor;
using ShadowMUD.MudObjects;
using DbLinq.SqlServer;

namespace ShadowMUD.MudObjects
{
    public enum TableID
    {
        Characters = 0,
        Rooms,
        Objects,
        Zones,
        Texts,
    }

    public enum PlayerState
    {
        Playing = 0, // Playing - Nominal state
        ConnectionClosed, // User disconnect, remove character.
        GetName, // Login with name
        ConfirmName, // New character, confirm name
        EnterPassword, // Login with password
        CreatePassword, // New character, create password
        ConfirmPassword, // New character, confirm password
        ChooseSex, // Choose character sex
        ChooseClass, // Choose character class
        ReadMotd, // Reading the message of the day
        MainMenu, // At the main menu
        SetPlayerDescription, // Enter a new character description prompt
        GetOldPassword, // Changing password: Get old
        GetNewPassword, // Changing password: Get new
        VerifyNewPassword, // Changing password: Verify new password
        DeleteCharacterConfirm1, // Character Delete: Confirmation 1
        DeleteCharacterConfirm2, // Character Delete: Confirmation 2
        Disconnected, // In-game link loss (leave character)
    }

    public class ShadowDb : DataContext
    {
        private const string TableUpdatedFormat = "EXEC dbo.table_updated {0}";

        public void TableUpdate(TableID table)
        {
            ExecuteCommand(TableUpdatedFormat, (int)table);
        }

        public ShadowDb(IDbConnection connection)
            : base(connection, new SqlServerVendor())
        {
        }

        public ShadowDb(IDbConnection connection, IVendor vendor)
            : base(connection, vendor)
        {
        }

        public Table<CharacterAbilities> CharacterAbilities
        {
            get { return GetTable<CharacterAbilities>(); }
        }

        public Table<CharacterFinances> CharacterFinances
        {
            get { return GetTable<CharacterFinances>(); }
        }

        public Table<CharacterQuests> CharacterQuests
        {
            get { return GetTable<CharacterQuests>(); }
        }

        public Table<Character> Characters
        {
            get { return GetTable<Character>(); }
        }

        public Table<CharacterSettings> CharacterSettings
        {
            get { return GetTable<CharacterSettings>(); }
        }

        public Table<CharacterState> CharacterState
        {
            get { return GetTable<CharacterState>(); }
        }

        public Table<CharacterStatistics> CharacterStatistics
        {
            get { return GetTable<CharacterStatistics>(); }
        }

        public Table<ExitDetail> ExitDetails
        {
            get { return GetTable<ExitDetail>(); }
        }

        public Table<ExitKeyword> ExitKeywords
        {
            get { return GetTable<ExitKeyword>(); }
        }

        public Table<CharacterInventory> Inventories
        {
            get { return GetTable<CharacterInventory>(); }
        }

        public Table<ObjectAffect> ObjectAffects
        {
            get { return GetTable<ObjectAffect>(); }
        }

        public Table<ObjectDetail> ObjectDetails
        {
            get { return GetTable<ObjectDetail>(); }
        }

        public Table<ObjectKeyword> ObjectKeywords
        {
            get { return GetTable<ObjectKeyword>(); }
        }

        public Table<ObjectFlags> ObjectFlags
        {
            get { return GetTable<ObjectFlags>(); }
        }

        public Table<ObjectWearFlags> ObjectWearFlags
        {
            get { return GetTable<ObjectWearFlags>(); }
        }

        public Table<Object> Objects
        {
            get { return GetTable<Object>(); }
        }

        public Table<Room> Rooms
        {
            get { return GetTable<Room>(); }
        }

        public Table<Text> TextTable
        {
            get { return GetTable<Text>(); }
        }

        public Table<UpdateTable> UpdateTable
        {
            get { return GetTable<UpdateTable>(); }
        }

        public Table<ZoneDetail> ZoneDetails
        {
            get { return GetTable<ZoneDetail>(); }
        }

        public Table<Zone> Zones
        {
            get { return GetTable<Zone>(); }
        }
    }
}