using GrobEngine;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

public class DB2Main : IObject
{
    public struct Language
    {
        public string LanguageFullName;
        public string LanguageShortName;
    }

    public enum MenuState
    {
        Language,
        Difficulty,
        Name,
        Story,
        Game
    }

    public enum PlayerState
    {
        Walking,
        Combat,
        Death,
        Ending
    }

    public enum PlayerDir
    {
        Forward,
        Left,
        Right
    }

    public enum PlayerCombat
    {
        Attack,
        Dodge,
        Block
    }

    private Dictionary<string, string> AvailableLanguages = new Dictionary<string, string>()
    {
        {"English", "en"}
    };

    private Dictionary<string, string> AvailableDifficulties = new Dictionary<string, string>()
    {
        {"Easy", "e"},
        {"Normal", "n"},
        {"Hard", "h"},
        {"Knightmare", "k"}
    };

    private static string gameFilePath = "DB2";
    private static string nullTexturePath = gameFilePath + "/Textures/null.png";

    public class BaseGameObject
    {
        public string Name = "null";
        public string Desc = "null";

        public BaseGameObject(string n, string d) 
        { 
            Name = n;
            Desc = d;
        }
    }

    public class GraphicalGameObject : BaseGameObject
    {
        public string TexturePath = nullTexturePath;

        public GraphicalGameObject(string n, string d, string p) : base(n, d)
        { 
            TexturePath = p;
        }
    }

    public class Enemy : GraphicalGameObject
    {
        public int Health = 100;
        public int ChanceToSpawn = 100;
        public bool IsBoss = false;

        public Enemy(string n, string d, string p, int h, int c) : base(n, d, p)
        { 
            Health = h;
            ChanceToSpawn = c;
        }

        public Enemy(string n, string d, string p, int h, int c, bool b) : base(n, d, p)
        { 
            Health = h;
            ChanceToSpawn = c;
            IsBoss = b;
        }
    }

    /*private List<Enemy> EnemyList = new List<Enemy>()
    {
        new Enemy();
    };*/

    public class Item : BaseGameObject
    {
        public int Damage;

        public Item(string n, string d, int dam) : base(n, d)
        { 
            Damage = dam;
        }
    }

    public class BaseGamePlayer
    {
        public string Name;
        public int Health;
        public int MaxHealth;
        public PlayerState State;
        public PlayerDir Direction;
        public PlayerCombat Combat;

        public BaseGamePlayer(string n, int h)
        { 
            Name = n;
            Health = h;
            MaxHealth = Health;
        }
    }

    private Language SelectedLanguage;
    private string SelectedDifficulty;
    private string configFile = "config.ini";
    private string title;
    private MenuState menuState;
    private string GameDecision;
    private SpriteBatch batch;
	private Texture2D textureLayer1;
    private Texture2D textureLayer2;
    private bool needSecondTextureLayer = false; 
    private bool textureBootup;
    public static BaseGamePlayer Player;

	public DB2Main() {}

	public override void Initialize(GrobEngineMain game)
    {
        Player = new BaseGamePlayer("Player", 100);
        GameDecision = "";
        SelectedLanguage = new Language();
        //Set to English by default.
        ResetLanguage();
        menuState = MenuState.Language;
        SetGameTitle(game);
    }

    public void Game(GrobEngineMain game)
    {
        GameDecision = "";

        // we will set textures based on the location and enemy here.
        switch(Player.Direction)
        {
            case PlayerDir.Forward:
                ChangeMainTexture(game, gameFilePath + "/Textures/forward.png");
            break;
            case PlayerDir.Left:
                ChangeMainTexture(game, gameFilePath + "/Textures/left_corner.png");
            break;
            case PlayerDir.Right:
                ChangeMainTexture(game, gameFilePath + "/Textures/right_corner.png");
            break;
            default:
            break;
        }

        if (!textureBootup)
        {
            switch(Player.State)
            {
                case PlayerState.Walking:
                    PlayerHUDText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/walkdecision_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
                    
                    GameDecision = Console.ReadLine();
                    if (GameDecision.Equals("f"))
                    {
                        Player.Direction = PlayerDir.Forward;
                    }
                    else if (GameDecision.Equals("l"))
                    {
                        Player.Direction = PlayerDir.Left;
                    }
                    else if (GameDecision.Equals("r"))
                    {
                        Player.Direction = PlayerDir.Right;
                    }

                    textureBootup = true;
                break;
                case PlayerState.Combat:
                break;
                case PlayerState.Death:
                break;
                case PlayerState.Ending:
                break;
                default:
                break;
            }
        }
        else
        {
            textureBootup = false;
        }
    }

    #region FNA Events
	public override void LoadContent(GrobEngineMain game)
    {
        batch = new SpriteBatch(game.GraphicsDevice);
        ChangeMainTexture(game, nullTexturePath);
        ChangeSecondaryTexture(game, nullTexturePath);
        needSecondTextureLayer = false;
    }

    public override void UnloadContent(GrobEngineMain game)
    {
        batch.Dispose();
		textureLayer1.Dispose();
        textureLayer2.Dispose();
    }
	
	public override void Update(GrobEngineMain game, GameTime gameTime)
    {
        switch(menuState)
        {
            case MenuState.Language:
                {
                    if (!Storage.DoesConfigFileExist(configFile) || String.IsNullOrWhiteSpace(Storage.ConfigINI(configFile, "Settings", "Language")))
                    {
                        LanguageSelect(game);
                    }

                    LoadLanguage();
                    SetGameTitle(game);
                    menuState = MenuState.Difficulty;
                }
            break;
            case MenuState.Difficulty:
                DifficultySelect(game);
            break;
            case MenuState.Name:
                ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/namequestion_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
                Player.Name = Console.ReadLine();
                menuState = MenuState.Story;
            break;
            case MenuState.Story:
                string fixedStory = ReadTags(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/story_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
                ConsoleText(fixedStory, true);
                Console.ReadKey();
                Player.Direction = PlayerDir.Right;
                menuState = MenuState.Game;
                textureBootup = true;
            break;
            default:
            break;
        }
    }

    public override void Draw(GrobEngineMain game, GameTime gameTime)
    {
        game.GraphicsDevice.Clear(Color.Black);

        if (menuState == MenuState.Game)
        {
            Game(game);
        }
        else
        {
            ChangeMainTexture(game, gameFilePath + "/Textures/hint.png");
        }

        batch.Begin();
		batch.Draw(textureLayer1, Vector2.Zero, Color.White);
        if (needSecondTextureLayer)
        {
            batch.Draw(textureLayer2, Vector2.Zero, Color.White);
        }
		batch.End();
    }
    #endregion

    #region Functions
    public string LoadTextFromContent(string scriptpath, ContentManager content)
    {
        string fullScriptPath = content.RootDirectory + '/' + scriptpath;

        try
        {
            using (var stream = TitleContainer.OpenStream(fullScriptPath))
            {
                using (var reader = new StreamReader(stream))
                {
                    string script = reader.ReadToEnd();
                    return script;
                }
            }
        }
        catch (Exception ex)
        {
            //tell the main script handler something's wrong.
            throw ex;
        }

        return null;
    }

    public void ResetLanguage()
    {
        SelectedLanguage.LanguageFullName = AvailableLanguages.ElementAt(0).Key;
        SelectedLanguage.LanguageShortName = AvailableLanguages.ElementAt(0).Value;
    }

    public void SetGameTitle(GrobEngineMain game)
    {
        title = LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/title_" + SelectedLanguage.LanguageShortName + ".txt", game.Content);
        game.Window.Title = title;
        Console.Title = title;
    }

    public void LoadLanguage()
    {
        SelectedLanguage.LanguageShortName = Storage.ConfigINI(configFile, "Settings", "Language");

        if(AvailableLanguages.ContainsValue(SelectedLanguage.LanguageShortName))
        {  
            string languageKey = AvailableLanguages.FirstOrDefault(x => x.Value == SelectedLanguage.LanguageShortName).Key;
            SelectedLanguage.LanguageFullName = languageKey;
        }
        else
        {
            ResetLanguage();
        }
    }

    public string ReadTags(string input)
    {
        return input.Replace("%name%", Player.Name);
    }

    public void ChangeMainTexture(GrobEngineMain game, string path)
    {
        textureLayer1 = game.Content.Load<Texture2D>(path);
    }

    public void ChangeSecondaryTexture(GrobEngineMain game, string path)
    {
        needSecondTextureLayer = true;
        textureLayer2 = game.Content.Load<Texture2D>(path);
    }
    #endregion

    #region Menus
    public void LanguageSelect(GrobEngineMain game)
    {
        ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/languagequestion_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
        
        for (int i = 0; i < AvailableLanguages.Count; i++)
        {
            Console.WriteLine("{0} - {1}", AvailableLanguages.ElementAt(i).Key, AvailableLanguages.ElementAt(i).Value);
        }
        
        string lang = Console.ReadLine();

        if(AvailableLanguages.ContainsValue(lang))
        {
            Storage.ConfigINI(configFile, "Settings", "Language", lang);
        }
        else
        {
            ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/languageerror_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
            Console.ReadKey();
            LanguageSelect(game);
            return;
        }
    }

    public void DifficultySelect(GrobEngineMain game)
    {
        SelectedDifficulty = "";
        ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/difficultyquestion_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
        
        for (int i = 0; i < AvailableDifficulties.Count; i++)
        {
            Console.WriteLine("{0} - {1}", AvailableDifficulties.ElementAt(i).Key, AvailableDifficulties.ElementAt(i).Value);
        }
        
        string diff = Console.ReadLine();

        if(AvailableDifficulties.ContainsValue(diff))
        {
            SelectedDifficulty = diff;
            PrintDifficultyDesc(game);
            string decision = Console.ReadLine();
            if (decision.Equals("y"))
            {
                menuState = MenuState.Name;
            }
            else if (decision.Equals("n"))
            {
                DifficultySelect(game);
                return;
            }
        }
        else
        {
            ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/difficultyerror_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
            Console.ReadKey();
            DifficultySelect(game);
            return;
        }
    }

    public void PrintDifficultyDesc(GrobEngineMain game)
    {
        ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/difficulty_" + SelectedDifficulty + "_" + SelectedLanguage.LanguageShortName + ".txt", game.Content),
                    false,
                    false);
    }

    public void ConsoleText(string text = "", bool noTitle = false, bool noDifficulty = true)
    {
        Console.Clear();
        if (!noTitle)
        {
            Console.WriteLine(title);
            Console.WriteLine();
        }
        if (!noDifficulty)
        {
            string diffKey = AvailableDifficulties.FirstOrDefault(x => x.Value == SelectedDifficulty).Key;
            Console.WriteLine(diffKey);
        }
        Console.WriteLine(text);
    }

    public void PlayerHUDText(string text = "")
    {
        Console.Clear();
        Console.WriteLine("----");
        Console.WriteLine(Player.Name);
        Console.WriteLine(Player.Health + "/" + Player.MaxHealth);
        Console.WriteLine(Player.State.ToString());
        Console.WriteLine("----");
        Console.WriteLine();
        Console.WriteLine(text);
    }
    #endregion
}