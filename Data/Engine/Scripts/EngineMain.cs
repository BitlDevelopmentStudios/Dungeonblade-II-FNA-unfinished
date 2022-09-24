using GrobEngine;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

public class EngineMain : IEngine
{
	private IObject gameMain;
	
	public EngineMain() {}
	
	public override void Initialize(GrobEngineMain game)
    {
		gameMain = (IObject)Script.LoadScriptFromContent("DB2/Scripts/DB2Main.cs", game.Content);
		gameMain.Initialize(game);
		ApplyVideoSettings(game, 360, 360, false, false);
    }
	
	public void ApplyVideoSettings(GrobEngineMain game, int width, int height, bool fullscreen, bool vsync)
	{
		game.graphics.PreferredBackBufferWidth = width;
		game.graphics.PreferredBackBufferHeight = height;
		game.graphics.SynchronizeWithVerticalRetrace = vsync;
		game.graphics.IsFullScreen = fullscreen;
		game.graphics.ApplyChanges();
	}
	
	public override void OnDeviceCreated(GrobEngineMain game, object sender, EventArgs e)
    {
	}
	
	public override void OnDeviceReset(GrobEngineMain game, object sender, EventArgs e)
    {
    }
	
	public override void LoadContent(GrobEngineMain game)
    {
        gameMain.LoadContent(game);
    }

    public override void UnloadContent(GrobEngineMain game)
    {
        gameMain.UnloadContent(game);
    }
	
	public override void Update(GrobEngineMain game, GameTime gameTime) 
	{ 
		gameMain.Update(game, gameTime);
	}

    public override void Draw(GrobEngineMain game, GameTime gameTime) 
	{ 
		gameMain.Draw(game, gameTime);
	}
}