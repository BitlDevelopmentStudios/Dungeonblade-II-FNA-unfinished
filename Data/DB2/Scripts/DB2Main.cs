using GrobEngine;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DB2Main : IObject
{
    public struct Language
    {
        public string LanguageFullName;
        public string LanguageShortName;
    }

    public enum State
    {
        Language,
        Difficulty,
        Name,
        Story,
        Game
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

    private Language SelectedLanguage;
    private string SelectedDifficulty;
    private string configFile = "config.ini";
    private string gameFilePath = "DB2";
    private string title;
    private string PlayerName;
    private State gameState;

	public DB2Main() {}

	public override void Initialize(GrobEngineMain game)
    {
        SelectedLanguage = new Language();
        //Set to English by default.
        ResetLanguage();
        gameState = State.Language;

        //set title then go to language select.
        SetGameTitle(game);
    }

    public void Game(GrobEngineMain game)
    {
        ConsoleText("game here", true);
        string decision = Console.ReadLine();
    }

    #region FNA Events
	public override void LoadContent(GrobEngineMain game)
    {
        //textureLoader.LoadContent(game);
    }

    public override void UnloadContent(GrobEngineMain game)
    {
        //textureLoader.UnloadContent(game);
    }
	
	public override void Update(GrobEngineMain game, GameTime gameTime)
    {
		//inputTest.Update(game, gameTime);

        switch(gameState)
        {
            case State.Language:
                {
                    if (!Storage.DoesConfigFileExist(configFile) || String.IsNullOrWhiteSpace(Storage.ConfigINI(configFile, "Settings", "Language")))
                    {
                        LanguageSelect(game);
                    }

                    LoadLanguage();
                    SetGameTitle(game);
                    gameState = State.Difficulty;
                }
            break;
            case State.Difficulty:
                DifficultySelect(game);
            break;
            case State.Name:
                ConsoleText(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/namequestion_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
                PlayerName = Console.ReadLine();
                gameState = State.Story;
            break;
            case State.Story:
                string fixedStory = ReadTags(LoadTextFromContent(gameFilePath + "/Resource/" + SelectedLanguage.LanguageShortName + "/story_" + SelectedLanguage.LanguageShortName + ".txt", game.Content));
                ConsoleText(fixedStory, true);
                Console.ReadKey();
                gameState = State.Game;
            break;
            default:
                Game(game);
            break;
        }
    }

    public override void Draw(GrobEngineMain game, GameTime gameTime)
    {
        game.GraphicsDevice.Clear(Color.Black);
        //textureLoader.Draw(game, gameTime);
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
        return input.Replace("%name%", PlayerName);
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
                gameState = State.Name;
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
    #endregion
}